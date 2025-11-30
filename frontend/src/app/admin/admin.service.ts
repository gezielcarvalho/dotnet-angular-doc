import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiResponse } from '../document/components/document.models';

@Injectable({
    providedIn: 'root',
})
export class AdminService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/admin`;

    constructor() {}

    /**
     * Trigger a server-side on-demand migration that ensures personal folders for existing users
     */
    runPersonalFolderMigration(): Observable<ApiResponse<number>> {
        return this.http.post<ApiResponse<number>>(
            `${this.apiUrl}/run-personal-folder-migration`,
            {},
        );
    }
}
