import { Component, Input } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
    standalone: true,
    imports: [RouterModule],
    selector: 'app-sidebar-icon',
    templateUrl: './sidebar-icon.component.html',
})
export class SidebarIconComponent {
    @Input() text: string = '';
    @Input() routerLink: string[] | never | undefined;
}
