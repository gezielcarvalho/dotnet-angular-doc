import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FolderService } from './folder.service';
import { environment } from '../../../environments/environment';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('FolderService', () => {
    let service: FolderService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
        service = TestBed.inject(FolderService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getFolders', () => {
        it('should fetch all folders', done => {
            const mockResponse = {
                success: true,
                message: 'Folders retrieved',
                data: [
                    {
                        id: '1',
                        name: 'Folder 1',
                        parentFolderId: null,
                        path: '/Folder 1',
                        createdAt: '2025-01-01T00:00:00Z',
                        canWrite: true,
                    },
                ],
            };

            service.getFolders().subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.length).toBe(1);
                expect(response.data?.[0].createdAt).toBeInstanceOf(Date);
                expect(response.data?.[0].canWrite).toBe(true);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/folders`);
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });

        it('should fetch folders by parent id', done => {
            const mockResponse = {
                success: true,
                message: 'Folders retrieved',
                data: [
                    {
                        id: '2',
                        name: 'Subfolder',
                        parentFolderId: '1',
                        path: '/Folder 1/Subfolder',
                        createdAt: '2025-01-01T00:00:00Z',
                    },
                ],
            };

            service.getFolders({ parentFolderId: '1' }).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.[0].parentFolderId).toBe('1');
                done();
            });

            const req = httpMock.expectOne(
                request =>
                    request.url === `${environment.apiUrl}/folders` &&
                    request.params.get('parentFolderId') === '1',
            );
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });

        it('should fetch folders by parent id and requiredPermission', done => {
            const mockResponse = {
                success: true,
                message: 'Folders retrieved',
                data: [],
            };

            service
                .getFolders({
                    parentFolderId: '1',
                    requiredPermission: 'Write',
                })
                .subscribe(response => {
                    expect(response.success).toBe(true);
                    done();
                });

            const req = httpMock.expectOne(
                request =>
                    request.url === `${environment.apiUrl}/folders` &&
                    request.params.get('parentFolderId') === '1' &&
                    request.params.get('requiredPermission') === 'Write',
            );
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });

    describe('getFolder', () => {
        it('should fetch a single folder by id', done => {
            const mockResponse = {
                success: true,
                message: 'Folder retrieved',
                data: {
                    id: '1',
                    name: 'Folder 1',
                    parentFolderId: null,
                    path: '/Folder 1',
                    createdAt: '2025-01-01T00:00:00Z',
                },
            };

            service.getFolder('1').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.id).toBe('1');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/folders/1`);
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });

    describe('createFolder', () => {
        it('should create a new folder', done => {
            const request = {
                name: 'New Folder',
                parentFolderId: undefined,
                description: 'Test folder',
            };

            const mockResponse = {
                success: true,
                message: 'Folder created',
                data: {
                    id: '1',
                    name: 'New Folder',
                    parentFolderId: null,
                    path: '/New Folder',
                    createdAt: '2025-01-01T00:00:00Z',
                },
            };

            service.createFolder(request).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.name).toBe('New Folder');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/folders`);
            expect(req.request.method).toBe('POST');
            req.flush(mockResponse);
        });
    });

    describe('updateFolder', () => {
        it('should update folder metadata', done => {
            const request = {
                name: 'Updated Folder',
                description: 'Updated description',
            };

            const mockResponse = {
                success: true,
                message: 'Folder updated',
                data: {
                    id: '1',
                    name: 'Updated Folder',
                    parentFolderId: null,
                    path: '/Updated Folder',
                    createdAt: '2025-01-01T00:00:00Z',
                },
            };

            service.updateFolder('1', request).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.name).toBe('Updated Folder');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/folders/1`);
            expect(req.request.method).toBe('PUT');
            req.flush(mockResponse);
        });
    });

    describe('deleteFolder', () => {
        it('should delete a folder', done => {
            const mockResponse = {
                success: true,
                message: 'Folder deleted',
                data: true,
            };

            service.deleteFolder('1').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data).toBe(true);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/folders/1`);
            expect(req.request.method).toBe('DELETE');
            req.flush(mockResponse);
        });
    });

    describe('buildBreadcrumbs', () => {
        it('should build breadcrumbs from path', () => {
            const path = '/Folder1/Folder2/Folder3';
            const breadcrumbs = service.buildBreadcrumbs(path);

            expect(breadcrumbs.length).toBe(3);
            expect(breadcrumbs[0]).toEqual({
                name: 'Folder1',
                path: '/Folder1',
            });
            expect(breadcrumbs[1]).toEqual({
                name: 'Folder2',
                path: '/Folder1/Folder2',
            });
            expect(breadcrumbs[2]).toEqual({
                name: 'Folder3',
                path: '/Folder1/Folder2/Folder3',
            });
        });

        it('should return empty array for root path', () => {
            const breadcrumbs = service.buildBreadcrumbs('/');
            expect(breadcrumbs.length).toBe(0);
        });

        it('should return empty array for empty path', () => {
            const breadcrumbs = service.buildBreadcrumbs('');
            expect(breadcrumbs.length).toBe(0);
        });
    });

    describe('getRootFolders', () => {
        it('should fetch root folders', done => {
            const mockResponse = {
                success: true,
                message: 'Folders retrieved',
                data: [],
            };

            service.getRootFolders().subscribe(response => {
                expect(response.success).toBe(true);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/folders`);
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });
});
