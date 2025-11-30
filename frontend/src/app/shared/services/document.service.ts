import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, map, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    DocumentDTO,
    DocumentVersionDTO,
    CreateDocumentRequest,
    UpdateDocumentRequest,
    UploadDocumentVersionRequest,
    PagedResponse,
    ApiResponse,
    DocumentFilterParams,
} from '../../document/components/document.models';

@Injectable({
    providedIn: 'root',
})
export class DocumentService {
    private http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/documents`;

    /**
     * Get paginated list of documents with optional filtering
     */
    getDocuments(
        params?: DocumentFilterParams,
    ): Observable<ApiResponse<PagedResponse<DocumentDTO>>> {
        let httpParams = new HttpParams();

        if (params) {
            if (params.folderId) {
                httpParams = httpParams.set('folderId', params.folderId);
            }
            if (params.search) {
                httpParams = httpParams.set('SearchTerm', params.search);
            }
            if (params.status) {
                httpParams = httpParams.set('status', params.status);
            }
            if (params.pageNumber) {
                httpParams = httpParams.set(
                    'PageNumber',
                    params.pageNumber.toString(),
                );
            }
            if (params.pageSize) {
                httpParams = httpParams.set(
                    'PageSize',
                    params.pageSize.toString(),
                );
            }
            if (params.tagIds && params.tagIds.length > 0) {
                params.tagIds.forEach(tagId => {
                    httpParams = httpParams.append('tagIds', tagId);
                });
            }
        }

        return this.http
            .get<ApiResponse<PagedResponse<DocumentDTO>>>(this.apiUrl, {
                params: httpParams,
            })
            .pipe(
                map(response => {
                    // Convert date strings to Date objects
                    if (response.success && response.data) {
                        response.data.items = response.data.items.map(doc =>
                            this.convertDates(doc),
                        );
                    }
                    return response;
                }),
                catchError(
                    this.handleError<PagedResponse<DocumentDTO>>(
                        'getDocuments',
                    ),
                ),
            );
    }

    /**
     * Get a single document by ID
     */
    getDocument(id: string): Observable<ApiResponse<DocumentDTO>> {
        return this.http
            .get<ApiResponse<DocumentDTO>>(`${this.apiUrl}/${id}`)
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = this.convertDates(response.data);
                    }
                    return response;
                }),
                catchError(this.handleError<DocumentDTO>('getDocument')),
            );
    }

    /**
     * Create a new document with file upload
     */
    createDocument(
        request: CreateDocumentRequest,
    ): Observable<ApiResponse<DocumentDTO>> {
        const formData = new FormData();
        formData.append('title', request.title);
        formData.append('folderId', request.folderId);
        formData.append('file', request.file);

        if (request.description) {
            formData.append('description', request.description);
        }

        if (request.tagIds && request.tagIds.length > 0) {
            request.tagIds.forEach(tagId => {
                formData.append('tagIds', tagId);
            });
        }

        return this.http
            .post<ApiResponse<DocumentDTO>>(this.apiUrl, formData)
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = this.convertDates(response.data);
                    }
                    return response;
                }),
                catchError(this.handleError<DocumentDTO>('createDocument')),
            );
    }

    /**
     * Update document metadata (not the file)
     */
    updateDocument(
        id: string,
        request: UpdateDocumentRequest,
    ): Observable<ApiResponse<DocumentDTO>> {
        return this.http
            .put<ApiResponse<DocumentDTO>>(`${this.apiUrl}/${id}`, request)
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = this.convertDates(response.data);
                    }
                    return response;
                }),
                catchError(this.handleError<DocumentDTO>('updateDocument')),
            );
    }

    /**
     * Upload a new version of a document
     */
    uploadNewVersion(
        documentId: string,
        request: UploadDocumentVersionRequest,
    ): Observable<ApiResponse<DocumentVersionDTO>> {
        const formData = new FormData();
        formData.append('file', request.file);

        if (request.changeComment) {
            formData.append('changeComment', request.changeComment);
        }

        return this.http
            .post<ApiResponse<DocumentVersionDTO>>(
                `${this.apiUrl}/${documentId}/upload-version`,
                formData,
            )
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = this.convertVersionDates(response.data);
                    }
                    return response;
                }),
                catchError(
                    this.handleError<DocumentVersionDTO>('uploadNewVersion'),
                ),
            );
    }

    /**
     * Get all versions of a document
     */
    getDocumentVersions(
        documentId: string,
    ): Observable<ApiResponse<DocumentVersionDTO[]>> {
        return this.http
            .get<ApiResponse<DocumentVersionDTO[]>>(
                `${this.apiUrl}/${documentId}/versions`,
            )
            .pipe(
                map(response => {
                    if (response.success && response.data) {
                        response.data = response.data.map(v =>
                            this.convertVersionDates(v),
                        );
                    }
                    return response;
                }),
                catchError(
                    this.handleError<DocumentVersionDTO[]>(
                        'getDocumentVersions',
                    ),
                ),
            );
    }

    /**
     * Download a document file
     */
    downloadDocument(documentId: string, version?: number): Observable<Blob> {
        let url = `${this.apiUrl}/${documentId}/download`;
        if (version) {
            url += `?version=${version}`;
        }

        return this.http.get(url, { responseType: 'blob' }).pipe(
            catchError(error => {
                console.error('Download error:', error);
                return of(new Blob());
            }),
        );
    }

    /**
     * Delete a document (soft delete)
     */
    deleteDocument(id: string): Observable<ApiResponse<boolean>> {
        return this.http
            .delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`)
            .pipe(catchError(this.handleError<boolean>('deleteDocument')));
    }

    /**
     * Helper to trigger file download in browser
     */
    triggerFileDownload(blob: Blob, filename: string): void {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
        window.URL.revokeObjectURL(url);
    }

    /**
     * Convert date strings to Date objects for DocumentDTO
     */
    private convertDates(doc: DocumentDTO): DocumentDTO {
        return {
            ...doc,
            createdAt: doc.createdAt ? new Date(doc.createdAt) : doc.createdAt,
            modifiedAt: doc.modifiedAt
                ? new Date(doc.modifiedAt)
                : doc.modifiedAt,
        };
    }

    /**
     * Convert date strings to Date objects for DocumentVersionDTO
     */
    private convertVersionDates(
        version: DocumentVersionDTO,
    ): DocumentVersionDTO {
        return {
            ...version,
            createdAt: version.createdAt
                ? new Date(version.createdAt)
                : version.createdAt,
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
