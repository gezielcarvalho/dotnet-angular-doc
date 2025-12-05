import { Component } from '@angular/core';
import { LogoComponent } from '../logo/logo.component';
import { LoginButtonComponent } from './login-button/login-button.component';

@Component({
    standalone: true,
    imports: [LogoComponent, LoginButtonComponent],
    selector: 'app-navigation',
    templateUrl: './navigation.component.html',
})
export class NavigationComponent {}
