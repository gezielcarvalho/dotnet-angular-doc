import { Component, OnInit } from '@angular/core';

import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../admin.service';
import { AuthService } from 'src/app/shared/services/auth.service';

@Component({
    imports: [RouterModule, FormsModule],
    selector: 'app-admin-dashboard',
    templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit {
    isAdmin = false;
    running = false;
    resultMessage: string | null = null;
    createdCount: number | null = null;
    // specific user creation
    userIdToCreate = '';
    creatingUser = false;
    createUserResultMessage: string | null = null;
    createUserCreated: boolean | null = null;

    constructor(
        private adminService: AdminService,
        private authService: AuthService,
        private router: Router,
    ) {}

    ngOnInit(): void {
        this.isAdmin = this.authService.isAdmin();
    }

    runMigration(): void {
        if (!this.isAdmin) return;
        this.running = true;
        this.resultMessage = null;
        this.createdCount = null;
        this.adminService.runPersonalFolderMigration().subscribe(
            resp => {
                this.running = false;
                if (resp.success) {
                    this.createdCount = resp.data ?? 0;
                    this.resultMessage = resp.message;
                } else {
                    this.resultMessage =
                        resp.message || 'Migration returned an error';
                }
            },
            err => {
                this.running = false;
                this.resultMessage =
                    'Migration failed: ' +
                    (err?.message ?? JSON.stringify(err));
            },
        );
    }

    createPersonalFolderForUser(): void {
        if (!this.isAdmin || !this.userIdToCreate) return;
        this.creatingUser = true;
        this.createUserResultMessage = null;
        this.createUserCreated = null;
        this.adminService
            .createPersonalFolderForUser(this.userIdToCreate)
            .subscribe(
                resp => {
                    this.creatingUser = false;
                    if (resp.success) {
                        this.createUserCreated = !!resp.data;
                        this.createUserResultMessage = resp.message;
                    } else {
                        this.createUserResultMessage =
                            resp.message || 'Create returned an error';
                    }
                },
                err => {
                    this.creatingUser = false;
                    this.createUserResultMessage =
                        'Create failed: ' +
                        (err?.message ?? JSON.stringify(err));
                },
            );
    }

    goHome(): void {
        this.router.navigate(['/']);
    }
}
