import { Injectable } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivate,
    Router,
    RouterStateSnapshot,
} from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root',
})
export class AdminGuard implements CanActivate {
    constructor(private service: AuthService, private router: Router) {}

    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Observable<boolean> | Promise<boolean> | boolean {
        console.log(
            '[AdminGuard] canActivate check: requested url =',
            state.url,
        );
        return this.service.isAuthenticated().then((authenticated: boolean) => {
            if (!authenticated) {
                console.warn(
                    '[AdminGuard] user not authenticated -> redirecting to /login',
                    state.url,
                );
                this.router.navigate(['/login'], {
                    queryParams: { returnUrl: state.url },
                });
                return false;
            }

            if (this.service.isAdmin()) {
                console.log('[AdminGuard] user is admin -> allow', state.url);
                return true;
            }

            console.warn(
                '[AdminGuard] authenticated but not admin -> redirecting to /',
                state.url,
            );
            this.router.navigate(['/']);
            return false;
        });
    }
}
