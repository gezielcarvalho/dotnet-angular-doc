import { TestBed } from '@angular/core/testing';
import {
    HttpClientTestingModule,
    HttpTestingController,
} from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { TokenService } from './token.service';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
    let service: AuthService;
    let httpMock: HttpTestingController;
    let tokenService: jasmine.SpyObj<TokenService>;
    let router: jasmine.SpyObj<Router>;

    beforeEach(() => {
        const tokenServiceSpy = jasmine.createSpyObj('TokenService', [
            'saveToken',
            'getToken',
            'removeToken',
            'saveUser',
            'getUser',
            'removeUser',
            'clear',
            'isTokenExpired',
        ]);

        const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                AuthService,
                { provide: TokenService, useValue: tokenServiceSpy },
                { provide: Router, useValue: routerSpy },
            ],
        });

        service = TestBed.inject(AuthService);
        httpMock = TestBed.inject(HttpTestingController);
        tokenService = TestBed.inject(
            TokenService,
        ) as jasmine.SpyObj<TokenService>;
        router = TestBed.inject(Router) as jasmine.SpyObj<Router>;

        tokenService.isTokenExpired.and.returnValue(false);
        tokenService.getToken.and.returnValue(null);
        tokenService.getUser.and.returnValue(null);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('login', () => {
        it('should login successfully', done => {
            const credentials = {
                username: 'testuser',
                password: 'password123',
            };
            const mockResponse = {
                success: true,
                message: 'Login successful',
                data: {
                    token: 'jwt-token',
                    username: 'testuser',
                    email: 'test@example.com',
                    role: 'User',
                },
            };

            service.login(credentials).subscribe(response => {
                expect(response.success).toBe(true);
                expect(tokenService.saveToken).toHaveBeenCalledWith(
                    'jwt-token',
                );
                expect(tokenService.saveUser).toHaveBeenCalled();
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
            expect(req.request.method).toBe('POST');
            req.flush(mockResponse);

            // Flush the getCurrentUser request
            const meReq = httpMock.match(`${environment.apiUrl}/auth/me`);
            if (meReq.length > 0) {
                meReq[0].flush({
                    success: true,
                    message: 'User retrieved',
                    data: {
                        id: '1',
                        username: 'testuser',
                        email: 'test@example.com',
                        firstName: 'Test',
                        lastName: 'User',
                        role: 'User',
                    },
                });
            }
        });

        it('should handle login error', done => {
            const credentials = {
                username: 'testuser',
                password: 'wrong',
            };

            service.login(credentials).subscribe(response => {
                expect(response.success).toBe(false);
                expect(response.message).toContain('failed');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
            req.error(new ProgressEvent('error'), {
                status: 401,
                statusText: 'Unauthorized',
            });
        });
    });

    describe('register', () => {
        it('should register successfully', done => {
            const userData = {
                username: 'newuser',
                email: 'new@example.com',
                password: 'password123',
                confirmPassword: 'password123',
                firstName: 'New',
                lastName: 'User',
            };
            const mockResponse = {
                success: true,
                message: 'Registration successful',
                data: {
                    token: 'jwt-token',
                    username: 'newuser',
                    email: 'new@example.com',
                    role: 'User',
                },
            };

            service.register(userData).subscribe(response => {
                expect(response.success).toBe(true);
                expect(tokenService.saveToken).toHaveBeenCalledWith(
                    'jwt-token',
                );
                done();
            });

            const req = httpMock.expectOne(
                `${environment.apiUrl}/auth/register`,
            );
            expect(req.request.method).toBe('POST');
            req.flush(mockResponse);

            // Flush the getCurrentUser request
            const meReq = httpMock.match(`${environment.apiUrl}/auth/me`);
            if (meReq.length > 0) {
                meReq[0].flush({
                    success: true,
                    message: 'User retrieved',
                    data: {
                        id: '1',
                        username: 'newuser',
                        email: 'new@example.com',
                        firstName: 'New',
                        lastName: 'User',
                        role: 'User',
                    },
                });
            }
        });
    });

    describe('logout', () => {
        it('should logout and clear tokens', () => {
            service.logout();

            expect(tokenService.clear).toHaveBeenCalled();
            expect(router.navigate).toHaveBeenCalledWith(['/']);
        });

        it('should update observables on logout', done => {
            let callCount = 0;
            service.loggedIn$.subscribe(isLoggedIn => {
                callCount++;
                if (callCount === 2) {
                    // First call is the initial value
                    expect(isLoggedIn).toBe(false);
                    done();
                }
            });

            service.logout();
        });
    });

    describe('authentication checks', () => {
        it('should return true when authenticated', async () => {
            tokenService.getToken.and.returnValue('valid-token');
            tokenService.isTokenExpired.and.returnValue(false);

            const result = await service.isAuthenticated();
            expect(result).toBe(true);
        });

        it('should return false when not authenticated', async () => {
            tokenService.getToken.and.returnValue(null);

            const result = await service.isAuthenticated();
            expect(result).toBe(false);
        });

        it('should return false when token is expired', async () => {
            tokenService.getToken.and.returnValue('expired-token');
            tokenService.isTokenExpired.and.returnValue(true);

            const result = await service.isAuthenticated();
            expect(result).toBe(false);
        });
    });

    describe('role checks', () => {
        it('should return true for admin role', () => {
            spyOn(service, 'getCurrentUserSync').and.returnValue({
                id: '1',
                username: 'admin',
                email: 'admin@example.com',
                firstName: 'Admin',
                lastName: 'User',
                role: 'Admin',
            });

            expect(service.isAdmin()).toBe(true);
        });

        it('should return true for SystemAdmin role', () => {
            spyOn(service, 'getCurrentUserSync').and.returnValue({
                id: '1',
                username: 'sysadmin',
                email: 'sys@example.com',
                firstName: 'System',
                lastName: 'Admin',
                role: 'SystemAdmin',
            });

            expect(service.isSystemAdmin()).toBe(true);
            expect(service.isAdmin()).toBe(true);
        });

        it('should return false for regular user', () => {
            spyOn(service, 'getCurrentUserSync').and.returnValue({
                id: '1',
                username: 'user',
                email: 'user@example.com',
                firstName: 'Regular',
                lastName: 'User',
                role: 'User',
            });

            expect(service.isAdmin()).toBe(false);
            expect(service.isSystemAdmin()).toBe(false);
        });
    });

    describe('getCurrentUser', () => {
        it('should fetch current user info', done => {
            const mockResponse = {
                success: true,
                message: 'User info retrieved',
                data: {
                    id: '1',
                    username: 'testuser',
                    email: 'test@example.com',
                    firstName: 'Test',
                    lastName: 'User',
                    role: 'User',
                },
            };

            service.getCurrentUser().subscribe(response => {
                expect(response.success).toBe(true);
                expect(tokenService.saveUser).toHaveBeenCalledWith(
                    mockResponse.data,
                );
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/auth/me`);
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });
});
