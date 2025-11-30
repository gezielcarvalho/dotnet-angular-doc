import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    FolderDTO,
    CreateFolderRequest,
    UpdateFolderRequest,
    ApiResponse,
    FolderFilterParams,
} from '../../document/components/document.models';

@Injectable({
    providedIn: 'root',
})
export class FolderService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/folders`;

    /**
     * Get folders (optionally filtered by parent folder)
     */
    getFolders(
        params?: FolderFilterParams,
    ): Observable<ApiResponse<FolderDTO[]>> {
        let httpParams = new HttpParams();

        if (params) {
            if (params.parentFolderId) {
                httpParams = httpParams.set(
                    'parentFolderId',
                    params.parentFolderId,
                );
            }
            if (params.search) {
                httpParams = httpParams.set('search', params.search);
            }
        }

        return this.http
            .get<ApiResponse<FolderDTO[]>>(this.apiUrl, { params: httpParams })
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = response.data.map(folder =>
                            this.convertDates(folder),
                        );
                    }
                    return response;
                }),
                catchError(this.handleError<FolderDTO[]>('getFolders')),
            );
    }

    /**
     * Get root folders (folders with no parent)
     */
    getRootFolders(): Observable<ApiResponse<FolderDTO[]>> {
        return this.getFolders();
    }

    /**
     * Get subfolders of a specific folder
     */
    getSubFolders(
        parentFolderId: string,
    ): Observable<ApiResponse<FolderDTO[]>> {
        return this.getFolders({ parentFolderId });
    }

    /**
     * Get a single folder by ID
     */
    getFolder(id: string): Observable<ApiResponse<FolderDTO>> {
        return this.http
            .get<ApiResponse<FolderDTO>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = this.convertDates(response.data);
                    }
                    return response;
                }),
                catchError(this.handleError<FolderDTO>('getFolder')),
            );
    }

    /**
     * Create a new folder
     */
    createFolder(
        request: CreateFolderRequest,
    ): Observable<ApiResponse<FolderDTO>> {
        return this.http
            .post<ApiResponse<FolderDTO>>(this.apiUrl, request)
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = this.convertDates(response.data);
                    }
                    return response;
                }),
                catchError(this.handleError<FolderDTO>('createFolder')),
            );
    }

    /**
     * Update folder metadata
     */
    updateFolder(
        id: string,
        request: UpdateFolderRequest,
    ): Observable<ApiResponse<FolderDTO>> {
        return this.http
            .put<ApiResponse<FolderDTO>>(`${this.apiUrl}/${id}`, request)
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = this.convertDates(response.data);
                    }
                    return response;
                }),
                catchError(this.handleError<FolderDTO>('updateFolder')),
            );
    }

    /**
     * Delete a folder (soft delete)
     */
    deleteFolder(id: string): Observable<ApiResponse<boolean>> {
        return this.http
            .delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`)
            .pipe(catchError(this.handleError<boolean>('deleteFolder')));
    }

    /**
     * Build breadcrumb path from folder path string
     */
    buildBreadcrumbs(path: string): { name: string; path: string }[] {
        if (!path || path === '/') return [];

        const parts = path.split('/').filter(p => p.length > 0);
        const breadcrumbs: { name: string; path: string }[] = [];

        let currentPath = '';
        parts.forEach(part => {
            currentPath += `/${part}`;
            breadcrumbs.push({
                name: part,
                path: currentPath,
            });
        });

        return breadcrumbs;
    }

    /**
     * Get folder tree structure (recursive)
     */
    getFolderTree(
        parentFolderId?: string,
    ): Observable<ApiResponse<FolderDTO[]>> {
        return this.getFolders({ parentFolderId });
    }

    /**
     * Convert date strings to Date objects
     */
    private convertDates(folder: FolderDTO): FolderDTO {
        return {
            ...folder,
            createdAt: folder.createdAt
                ? new Date(folder.createdAt)
                : folder.createdAt,
        };
    }

    /**
     * Generic error handler
     */
    private handleError<T>(operation = 'operation') {
        return (error: any): Observable<ApiResponse<T>> => {
            console.error(`${operation} failed:`, error);

            const errorMessage =
                error.error?.message || error.message || `${operation} failed`;

            return of({
                success: false,
                message: errorMessage,
                data: null,
            });
        };
    }
}
