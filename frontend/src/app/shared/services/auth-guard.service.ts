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
        return this.service.isAuthenticated().then((authenticated: boolean) => {
            if (authenticated) {
                return true;
            } else {
                // redirect to login and include returnUrl so user can be sent back
                this.router.navigate(['/login'], {
                    queryParams: { returnUrl: state.url },
                });
                return false;
            }
        });
    }
}
