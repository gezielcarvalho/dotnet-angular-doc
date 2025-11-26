import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    TagDTO,
    CreateTagRequest,
    UpdateTagRequest,
    ApiResponse,
} from '../models/edm.models';

@Injectable({
    providedIn: 'root',
})
export class TagService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/tags`;

    /**
     * Get all tags
     */
    getTags(): Observable<ApiResponse<TagDTO[]>> {
        return this.http
            .get<ApiResponse<TagDTO[]>>(this.apiUrl)
            .pipe(catchError(this.handleError<TagDTO[]>('getTags')));
    }

    /**
     * Get a single tag by ID
     */
    getTag(id: string): Observable<ApiResponse<TagDTO>> {
        return this.http
            .get<ApiResponse<TagDTO>>(`${this.apiUrl}/${id}`)
            .pipe(catchError(this.handleError<TagDTO>('getTag')));
    }

    /**
     * Create a new tag (requires Admin role)
     */
    createTag(request: CreateTagRequest): Observable<ApiResponse<TagDTO>> {
        return this.http
            .post<ApiResponse<TagDTO>>(this.apiUrl, request)
            .pipe(catchError(this.handleError<TagDTO>('createTag')));
    }

    /**
     * Update a tag (requires Admin role)
     */
    updateTag(
        id: string,
        request: UpdateTagRequest,
    ): Observable<ApiResponse<TagDTO>> {
        return this.http
            .put<ApiResponse<TagDTO>>(`${this.apiUrl}/${id}`, request)
            .pipe(catchError(this.handleError<TagDTO>('updateTag')));
    }

    /**
     * Delete a tag (requires Admin role, soft delete)
     */
    deleteTag(id: string): Observable<ApiResponse<boolean>> {
        return this.http
            .delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`)
            .pipe(catchError(this.handleError<boolean>('deleteTag')));
    }

    /**
     * Get tag by name (client-side filter)
     */
    getTagByName(name: string): Observable<ApiResponse<TagDTO | null>> {
        return new Observable(observer => {
            this.getTags().subscribe(response => {
                if (response.success && response.data) {
                    const tag =
                        response.data.find(
                            t => t.name.toLowerCase() === name.toLowerCase(),
                        ) || null;
                    observer.next({
                        success: true,
                        message: tag ? 'Tag found' : 'Tag not found',
                        data: tag,
                    });
                } else {
                    observer.next({
                        success: false,
                        message: response.message,
                        data: null,
                    });
                }
                observer.complete();
            });
        });
    }

    /**
     * Search tags by name (client-side filter)
     */
    searchTags(searchTerm: string): Observable<ApiResponse<TagDTO[]>> {
        return new Observable(observer => {
            this.getTags().subscribe(response => {
                if (response.success && response.data) {
                    const filteredTags = response.data.filter(tag =>
                        tag.name
                            .toLowerCase()
                            .includes(searchTerm.toLowerCase()),
                    );
                    observer.next({
                        success: true,
                        message: 'Tags found',
                        data: filteredTags,
                    });
                } else {
                    observer.next(response);
                }
                observer.complete();
            });
        });
    }

    /**
     * Generate a random color for new tags
     */
    generateRandomColor(): string {
        const colors = [
            '#3B82F6', // blue
            '#10B981', // green
            '#F59E0B', // amber
            '#EF4444', // red
            '#8B5CF6', // violet
            '#EC4899', // pink
            '#14B8A6', // teal
            '#F97316', // orange
            '#6366F1', // indigo
            '#84CC16', // lime
        ];
        return colors[Math.floor(Math.random() * colors.length)];
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
