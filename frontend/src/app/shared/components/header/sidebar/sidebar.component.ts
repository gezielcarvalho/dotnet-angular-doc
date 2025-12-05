import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { AuthService } from 'src/app/shared/services/auth.service';
import {
    faCube,
    faHome,
    faListCheck,
    faLock,
    faKitchenSet,
    faTicket,
    faChalkboard,
    faUsers,
} from '@fortawesome/free-solid-svg-icons';
import { SidebarIconComponent } from './sidebar-icon/sidebar-icon.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { RouterModule } from '@angular/router';

@Component({
    standalone: true,
    imports: [SidebarIconComponent, FontAwesomeModule, RouterModule],
    selector: 'app-sidebar',
    templateUrl: './sidebar.component.html',
})
export class SidebarComponent {
    faCube = faCube;
    faHome = faHome;
    faListCheck = faListCheck;
    faLock = faLock;
    faKitchenSet = faKitchenSet;
    faTicket = faTicket;
    faChalkboard = faChalkboard;
    faUsers = faUsers;

    private authService = inject(AuthService);

    isAdmin(): boolean {
        return this.authService.isAdmin();
    }
}
