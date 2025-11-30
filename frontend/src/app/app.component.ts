import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    RouterModule,
    Router,
    NavigationStart,
    NavigationEnd,
    NavigationCancel,
    NavigationError,
} from '@angular/router';
import { HomeComponent } from './public/screens/home/home.component';
import { HeaderComponent } from './shared/components/header/header.component';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [CommonModule, RouterModule, HomeComponent, HeaderComponent],
    templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
    title = 'ASP.NET Core + Angular Starter';
    constructor(private router: Router) {}

    ngOnInit(): void {
        // Log router events to help diagnose unwanted redirects
        this.router.events.subscribe(event => {
            if (event instanceof NavigationStart) {
                console.log('[Router] NavigationStart -> url:', event.url);
            } else if (event instanceof NavigationEnd) {
                console.log('[Router] NavigationEnd -> url:', event.url);
            } else if (event instanceof NavigationCancel) {
                console.log('[Router] NavigationCancel -> url:', event.url);
            } else if (event instanceof NavigationError) {
                console.log(
                    '[Router] NavigationError -> url:',
                    event.url,
                    'error:',
                    event.error,
                );
            }
        });
    }
}
