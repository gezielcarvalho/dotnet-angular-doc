import { TestBed } from '@angular/core/testing';
import {
    HttpClientTestingModule,
    HttpTestingController,
} from '@angular/common/http/testing';
import { DocumentService } from './document.service';
import { environment } from '../../../environments/environment';

describe('DocumentService', () => {
    let service: DocumentService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
        });
        service = TestBed.inject(DocumentService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getDocuments', () => {
        it('should fetch documents with filters', done => {
            const mockResponse = {
                success: true,
                message: 'Documents retrieved',
                data: {
                    items: [
                        {
                            id: '1',
                            title: 'Test Doc',
                            folderId: 'folder1',
                            currentVersion: 1,
                            fileSizeBytes: 1024,
                            mimeType: 'application/pdf',
                            createdAt: '2025-01-01T00:00:00Z',
                            modifiedAt: '2025-01-01T00:00:00Z',
                        },
                    ],
                    totalCount: 1,
                    pageNumber: 1,
                    pageSize: 10,
                },
            };

            const params = {
                folderId: 'folder1',
                search: 'test',
                pageNumber: 1,
                pageSize: 10,
            };

            service.getDocuments(params).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.items.length).toBe(1);
                expect(response.data?.items[0].createdAt).toBeInstanceOf(Date);
                done();
            });

            const req = httpMock.expectOne(
                request => request.url === `${environment.apiUrl}/documents`,
            );
            expect(req.request.method).toBe('GET');
            expect(req.request.params.get('folderId')).toBe('folder1');
            expect(req.request.params.get('SearchTerm')).toBe('test');
            req.flush(mockResponse);
        });

        it('should handle errors gracefully', done => {
            service.getDocuments().subscribe(response => {
                expect(response.success).toBe(false);
                expect(response.message).toBeTruthy();
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/documents`);
            req.error(new ProgressEvent('error'));
        });
    });

    describe('getDocument', () => {
        it('should fetch a single document by id', done => {
            const mockResponse = {
                success: true,
                message: 'Document retrieved',
                data: {
                    id: '1',
                    title: 'Test Doc',
                    folderId: 'folder1',
                    currentVersion: 1,
                    fileSizeBytes: 1024,
                    mimeType: 'application/pdf',
                    createdAt: '2025-01-01T00:00:00Z',
                    modifiedAt: '2025-01-01T00:00:00Z',
                },
            };

            service.getDocument('1').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.id).toBe('1');
                expect(response.data?.createdAt).toBeInstanceOf(Date);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/documents/1`);
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });

    describe('createDocument', () => {
        it('should create a document with file upload', done => {
            const file = new File(['content'], 'test.pdf', {
                type: 'application/pdf',
            });
            const request = {
                title: 'New Document',
                folderId: 'folder1',
                file: file,
                description: 'Test description',
                tagIds: ['tag1', 'tag2'],
            };

            const mockResponse = {
                success: true,
                message: 'Document created',
                data: {
                    id: '1',
                    title: 'New Document',
                    folderId: 'folder1',
                    currentVersion: 1,
                    fileSizeBytes: 1024,
                    mimeType: 'application/pdf',
                    createdAt: '2025-01-01T00:00:00Z',
                    modifiedAt: '2025-01-01T00:00:00Z',
                },
            };

            service.createDocument(request).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.title).toBe('New Document');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/documents`);
            expect(req.request.method).toBe('POST');
            expect(req.request.body).toBeInstanceOf(FormData);
            req.flush(mockResponse);
        });
    });

    describe('updateDocument', () => {
        it('should update document metadata', done => {
            const request = {
                title: 'Updated Title',
                description: 'Updated description',
            };

            const mockResponse = {
                success: true,
                message: 'Document updated',
                data: {
                    id: '1',
                    title: 'Updated Title',
                    folderId: 'folder1',
                    currentVersion: 1,
                    fileSizeBytes: 1024,
                    mimeType: 'application/pdf',
                    createdAt: '2025-01-01T00:00:00Z',
                    modifiedAt: '2025-01-01T00:00:00Z',
                },
            };

            service.updateDocument('1', request).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.title).toBe('Updated Title');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/documents/1`);
            expect(req.request.method).toBe('PUT');
            req.flush(mockResponse);
        });
    });

    describe('deleteDocument', () => {
        it('should delete a document', done => {
            const mockResponse = {
                success: true,
                message: 'Document deleted',
                data: true,
            };

            service.deleteDocument('1').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data).toBe(true);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/documents/1`);
            expect(req.request.method).toBe('DELETE');
            req.flush(mockResponse);
        });
    });

    describe('downloadDocument', () => {
        it('should download a document', done => {
            const blob = new Blob(['file content'], {
                type: 'application/pdf',
            });

            service.downloadDocument('1').subscribe(result => {
                expect(result).toBeInstanceOf(Blob);
                done();
            });

            const req = httpMock.expectOne(
                `${environment.apiUrl}/documents/1/download`,
            );
            expect(req.request.method).toBe('GET');
            expect(req.request.responseType).toBe('blob');
            req.flush(blob);
        });

        it('should download a specific version', done => {
            const blob = new Blob(['file content'], {
                type: 'application/pdf',
            });

            service.downloadDocument('1', 2).subscribe(result => {
                expect(result).toBeInstanceOf(Blob);
                done();
            });

            const req = httpMock.expectOne(
                `${environment.apiUrl}/documents/1/download?version=2`,
            );
            expect(req.request.method).toBe('GET');
            req.flush(blob);
        });
    });

    describe('triggerFileDownload', () => {
        it('should trigger browser download', () => {
            const blob = new Blob(['content'], { type: 'text/plain' });
            const createElementSpy = spyOn(
                document,
                'createElement',
            ).and.callThrough();
            const createObjectURLSpy = spyOn(
                window.URL,
                'createObjectURL',
            ).and.returnValue('blob:url');
            const revokeObjectURLSpy = spyOn(window.URL, 'revokeObjectURL');

            service.triggerFileDownload(blob, 'test.txt');

            expect(createElementSpy).toHaveBeenCalledWith('a');
            expect(createObjectURLSpy).toHaveBeenCalledWith(blob);
            expect(revokeObjectURLSpy).toHaveBeenCalled();
        });
    });
});
