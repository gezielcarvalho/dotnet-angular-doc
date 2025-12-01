import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AdminService } from './admin.service';
import { environment } from '../../environments/environment';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';

describe('AdminService', () => {
    let service: AdminService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
    imports: [],
    providers: [provideHttpClient(withInterceptorsFromDi()), provideHttpClientTesting()]
});
        service = TestBed.inject(AdminService);
        httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should post to run-personal-folder-migration', done => {
        const mockResponse = {
            success: true,
            message: 'Migration completed',
            data: 2,
        };

        service.runPersonalFolderMigration().subscribe(response => {
            expect(response.success).toBe(true);
            expect(response.data).toBe(2);
            done();
        });

        const req = httpMock.expectOne(
            `${environment.apiUrl}/admin/run-personal-folder-migration`,
        );
        expect(req.request.method).toBe('POST');
        req.flush(mockResponse);
    });
});
