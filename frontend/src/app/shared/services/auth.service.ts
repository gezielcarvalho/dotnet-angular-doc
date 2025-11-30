import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, of, map } from 'rxjs';
import { Router } from '@angular/router';
import { TokenService } from './token.service';
import { environment } from '../../../environments/environment';
import {
    LoginRequest,
    LoginResponse,
    RegisterRequest,
    ChangePasswordRequest,
    UserInfo,
    ApiResponse,
} from '../models/auth.models';

@Injectable({
    providedIn: 'root',
})
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);
    private tokenService = inject(TokenService);

    private loggedInSubject = new BehaviorSubject<boolean>(
        this.hasValidToken(),
    );
    public loggedIn$ = this.loggedInSubject.asObservable();

    private currentUserSubject = new BehaviorSubject<UserInfo | null>(
        this.tokenService.getUser(),
    );
    public currentUser$ = this.currentUserSubject.asObservable();

    private readonly apiUrl = `${environment.apiUrl}/auth`;

    constructor() {
        // Check token validity on service initialization
        if (this.tokenService.isTokenExpired()) {
            console.warn(
                '[AuthService] Token expired during service init; initiating logout',
            );
            // Include optional info about stored token/user
            try {
                const token = this.tokenService.getToken();
                console.debug(
                    '[AuthService] Stored token (truncated):',
                    token?.substring?.(0, 20),
                );
            } catch (e) {
                console.debug('[AuthService] Error reading stored token', e);
            }
            // call logout but we might prefer not to navigate during init
            // Do not navigate to '/' on automatic logout in constructor
            this.logout(false);
        }
    }

    private hasValidToken(): boolean {
        return (
            !!this.tokenService.getToken() &&
            !this.tokenService.isTokenExpired()
        );
    }

    login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
        return this.http
            .post<ApiResponse<LoginResponse>>(
                `${this.apiUrl}/login`,
                credentials,
            )
            .pipe(
                tap(response => {
                    if (response.success && response.data) {
                        this.handleAuthSuccess(response.data);
                    }
                }),
                catchError(error => {
                    console.error('Login error:', error);
                    return of({
                        success: false,
                        message: error.error?.message || 'Login failed',
                        data: null,
                    });
                }),
            );
    }

    register(
        userData: RegisterRequest,
    ): Observable<ApiResponse<LoginResponse>> {
        return this.http
            .post<ApiResponse<LoginResponse>>(
                `${this.apiUrl}/register`,
                userData,
            )
            .pipe(
                tap(response => {
                    if (response.success && response.data) {
                        this.handleAuthSuccess(response.data);
                    }
                }),
                catchError(error => {
                    console.error('Registration error:', error);
                    return of({
                        success: false,
                        message: error.error?.message || 'Registration failed',
                        data: null,
                    });
                }),
            );
    }

    changePassword(
        request: ChangePasswordRequest,
    ): Observable<ApiResponse<boolean>> {
        return this.http
            .post<ApiResponse<boolean>>(
                `${this.apiUrl}/change-password`,
                request,
            )
            .pipe(
                catchError(error => {
                    console.error('Change password error:', error);
                    return of({
                        success: false,
                        message:
                            error.error?.message || 'Password change failed',
                        data: false,
                    });
                }),
            );
    }

    getCurrentUser(): Observable<ApiResponse<UserInfo>> {
        return this.http.get<ApiResponse<UserInfo>>(`${this.apiUrl}/me`).pipe(
            tap(response => {
                if (response.success && response.data) {
                    this.tokenService.saveUser(response.data);
                    this.currentUserSubject.next(response.data);
                }
            }),
            catchError(error => {
                console.error('Get current user error:', error);
                return of({
                    success: false,
                    message: error.error?.message || 'Failed to get user info',
                    data: null,
                });
            }),
        );
    }

    logout(navigate: boolean = true): void {
        console.warn(
            '[AuthService] logout called; clearing token and navigating to /',
        );
        // Log a minimal stack trace to find the source of the call
        try {
            const stack = new Error().stack?.split('\n').slice(2, 6).join('\n');
            console.debug('[AuthService] logout call stack:\n', stack);
        } catch (err) {
            /* ignore stack errors */
        }
        this.tokenService.clear();
        this.loggedInSubject.next(false);
        this.currentUserSubject.next(null);
        if (navigate) {
            this.router.navigate(['/']);
        }
    }

    isAuthenticated(): Promise<boolean> {
        return Promise.resolve(this.hasValidToken());
    }

    isAuthenticatedSync(): boolean {
        return this.hasValidToken();
    }

    getCurrentUserSync(): UserInfo | null {
        return this.currentUserSubject.value;
    }

    hasRole(role: string): boolean {
        const user = this.getCurrentUserSync();
        return user?.role === role;
    }

    isAdmin(): boolean {
        return this.hasRole('Admin') || this.hasRole('SystemAdmin');
    }

    isSystemAdmin(): boolean {
        return this.hasRole('SystemAdmin');
    }

    private handleAuthSuccess(loginResponse: LoginResponse): void {
        this.tokenService.saveToken(loginResponse.token);

        const userInfo: UserInfo = {
            id: '', // Will be populated from JWT or subsequent API call
            username: loginResponse.username,
            email: loginResponse.email,
            firstName: '',
            lastName: '',
            role: loginResponse.role,
        };

        this.tokenService.saveUser(userInfo);
        this.loggedInSubject.next(true);
        this.currentUserSubject.next(userInfo);

        // Optionally fetch full user info
        this.getCurrentUser().subscribe();
    }
}
