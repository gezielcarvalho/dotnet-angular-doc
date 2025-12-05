import { Component } from '@angular/core';
import { NavigationComponent } from './navigation/navigation.component';
import { SidebarComponent } from './sidebar/sidebar.component';

@Component({
    standalone: true,
    imports: [NavigationComponent, SidebarComponent],
    selector: 'app-header',
    templateUrl: './header.component.html',
})
export class HeaderComponent {
    title = 'DS Catalog';
}
