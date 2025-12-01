import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthGuard } from './auth-guard.service';
import { AuthService } from './auth.service';

describe('AuthGuard', () => {
    let guard: AuthGuard;
    let authService: jasmine.SpyObj<AuthService>;
    let router: jasmine.SpyObj<Router>;

    beforeEach(() => {
        const authServiceSpy = jasmine.createSpyObj('AuthService', [
            'isAuthenticated',
        ]);
        const routerSpy = jasmine.createSpyObj('Router', ['navigate']);

        TestBed.configureTestingModule({
            providers: [
                AuthGuard,
                { provide: AuthService, useValue: authServiceSpy },
                { provide: Router, useValue: routerSpy },
            ],
        });

        guard = TestBed.inject(AuthGuard);
        authService = TestBed.inject(
            AuthService,
        ) as jasmine.SpyObj<AuthService>;
        router = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    });

    it('should be created', () => {
        expect(guard).toBeTruthy();
    });

    it('should allow activation when user is authenticated', async () => {
        authService.isAuthenticated.and.returnValue(Promise.resolve(true));

        const result = await guard.canActivate({} as any, {} as any);

        expect(result).toBe(true);
        expect(router.navigate).not.toHaveBeenCalled();
    });

    it('should deny activation and redirect when user is not authenticated', async () => {
        authService.isAuthenticated.and.returnValue(Promise.resolve(false));

        const result = await guard.canActivate({} as any, {} as any);

        expect(result).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(
            ['/login'],
            jasmine.objectContaining({
                queryParams: jasmine.objectContaining({ returnUrl: undefined }),
            }),
        );
    });
});
