import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { AdminGuard } from './admin-guard.service';
import { AuthService } from './auth.service';

describe('AdminGuard', () => {
    let guard: AdminGuard;
    let router: Router;
    let mockAuthService: jasmine.SpyObj<AuthService>;

    beforeEach(() => {
        mockAuthService = jasmine.createSpyObj('AuthService', [
            'isAuthenticated',
            'isAdmin',
        ]);
        TestBed.configureTestingModule({
            imports: [RouterTestingModule],
            providers: [{ provide: AuthService, useValue: mockAuthService }],
        });
        guard = TestBed.inject(AdminGuard);
        router = TestBed.inject(Router);
    });

    it('should allow admin users', async () => {
        mockAuthService.isAuthenticated.and.returnValue(Promise.resolve(true));
        mockAuthService.isAdmin.and.returnValue(true);
        const result = await guard.canActivate(
            {} as any,
            { url: '/admin' } as any,
        );
        expect(result).toBe(true);
    });

    it('should redirect unauthenticated users to /login', async () => {
        mockAuthService.isAuthenticated.and.returnValue(Promise.resolve(false));
        mockAuthService.isAdmin.and.returnValue(false);
        spyOn(router, 'navigate');
        const result = await guard.canActivate(
            {} as any,
            { url: '/admin' } as any,
        );
        expect(result).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(['/login'], {
            queryParams: { returnUrl: '/admin' },
        });
    });

    it('should redirect non-admin users to /', async () => {
        mockAuthService.isAuthenticated.and.returnValue(Promise.resolve(true));
        mockAuthService.isAdmin.and.returnValue(false);
        spyOn(router, 'navigate');
        const result = await guard.canActivate(
            {} as any,
            { url: '/admin' } as any,
        );
        expect(result).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(['/']);
    });
});
