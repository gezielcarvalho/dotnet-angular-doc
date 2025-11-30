import { Injectable } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivate,
    Route,
    Router,
    RouterStateSnapshot,
} from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root',
})
export class AuthGuard implements CanActivate {
    constructor(private service: AuthService, private router: Router) {}
    canActivate(
        route: ActivatedRouteSnapshot,
        state: RouterStateSnapshot,
    ): Observable<boolean> | Promise<boolean> | boolean {
        console.log(
            '[AuthGuard] canActivate check: requested url =',
            state.url,
        );
        return this.service.isAuthenticated().then((authenticated: boolean) => {
            if (authenticated) {
                console.log(
                    '[AuthGuard] canActivate: authenticated -> allow',
                    state.url,
                );
                return true;
            } else {
                console.warn(
                    '[AuthGuard] canActivate: not authenticated -> redirecting to /login',
                    state.url,
                );
                this.router.navigate(['/login'], {
                    queryParams: { returnUrl: state.url },
                });
                return false;
            }
        });
    }
}
