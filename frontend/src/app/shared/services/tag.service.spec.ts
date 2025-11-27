import { TestBed } from '@angular/core/testing';
import {
    HttpClientTestingModule,
    HttpTestingController,
} from '@angular/common/http/testing';
import { TagService } from './tag.service';
import { environment } from '../../../environments/environment';

describe('TagService', () => {
    let service: TagService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
        });
        service = TestBed.inject(TagService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    describe('getTags', () => {
        it('should fetch all tags', done => {
            const mockResponse = {
                success: true,
                message: 'Tags retrieved',
                data: [
                    { id: '1', name: 'Important', color: '#FF0000' },
                    { id: '2', name: 'Archive', color: '#00FF00' },
                ],
            };

            service.getTags().subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.length).toBe(2);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags`);
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });

    describe('getTag', () => {
        it('should fetch a single tag by id', done => {
            const mockResponse = {
                success: true,
                message: 'Tag retrieved',
                data: { id: '1', name: 'Important', color: '#FF0000' },
            };

            service.getTag('1').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.name).toBe('Important');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags/1`);
            expect(req.request.method).toBe('GET');
            req.flush(mockResponse);
        });
    });

    describe('createTag', () => {
        it('should create a new tag', done => {
            const request = {
                name: 'New Tag',
                color: '#0000FF',
            };

            const mockResponse = {
                success: true,
                message: 'Tag created',
                data: { id: '1', name: 'New Tag', color: '#0000FF' },
            };

            service.createTag(request).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.name).toBe('New Tag');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags`);
            expect(req.request.method).toBe('POST');
            req.flush(mockResponse);
        });
    });

    describe('updateTag', () => {
        it('should update a tag', done => {
            const request = {
                name: 'Updated Tag',
                color: '#00FF00',
            };

            const mockResponse = {
                success: true,
                message: 'Tag updated',
                data: { id: '1', name: 'Updated Tag', color: '#00FF00' },
            };

            service.updateTag('1', request).subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.name).toBe('Updated Tag');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags/1`);
            expect(req.request.method).toBe('PUT');
            req.flush(mockResponse);
        });
    });

    describe('deleteTag', () => {
        it('should delete a tag', done => {
            const mockResponse = {
                success: true,
                message: 'Tag deleted',
                data: true,
            };

            service.deleteTag('1').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data).toBe(true);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags/1`);
            expect(req.request.method).toBe('DELETE');
            req.flush(mockResponse);
        });
    });

    describe('getTagByName', () => {
        it('should find tag by name', done => {
            const mockTags = {
                success: true,
                message: 'Tags retrieved',
                data: [
                    { id: '1', name: 'Important', color: '#FF0000' },
                    { id: '2', name: 'Archive', color: '#00FF00' },
                ],
            };

            service.getTagByName('Important').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.name).toBe('Important');
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags`);
            req.flush(mockTags);
        });

        it('should return null when tag not found', done => {
            const mockTags = {
                success: true,
                message: 'Tags retrieved',
                data: [{ id: '1', name: 'Important', color: '#FF0000' }],
            };

            service.getTagByName('NonExistent').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data).toBeNull();
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags`);
            req.flush(mockTags);
        });
    });

    describe('searchTags', () => {
        it('should search tags by name', done => {
            const mockTags = {
                success: true,
                message: 'Tags retrieved',
                data: [
                    { id: '1', name: 'Important', color: '#FF0000' },
                    { id: '2', name: 'Archive', color: '#00FF00' },
                    { id: '3', name: 'Imported', color: '#0000FF' },
                ],
            };

            service.searchTags('import').subscribe(response => {
                expect(response.success).toBe(true);
                expect(response.data?.length).toBe(2);
                done();
            });

            const req = httpMock.expectOne(`${environment.apiUrl}/tags`);
            req.flush(mockTags);
        });
    });

    describe('generateRandomColor', () => {
        it('should generate a valid hex color', () => {
            const color = service.generateRandomColor();
            expect(color).toMatch(/^#[0-9A-F]{6}$/i);
        });

        it('should generate different colors', () => {
            const colors = new Set();
            for (let i = 0; i < 20; i++) {
                colors.add(service.generateRandomColor());
            }
            // Should have some variety (at least 3 different colors in 20 tries)
            expect(colors.size).toBeGreaterThanOrEqual(3);
        });
    });
});
