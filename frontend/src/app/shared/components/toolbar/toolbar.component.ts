import { Component, OnInit } from '@angular/core';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';

@Component({
    standalone: true,
    imports: [MatMenuModule, RouterLink],
    selector: 'app-toolbar',
    templateUrl: './toolbar.component.html',
})
export class ToolbarComponent implements OnInit {
    constructor() {}

    ngOnInit() {
        // TODO
    }
}
