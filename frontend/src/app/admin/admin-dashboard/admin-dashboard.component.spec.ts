import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AdminDashboardComponent } from './admin-dashboard.component';
import { AdminService } from '../admin.service';
import { AuthService } from 'src/app/shared/services/auth.service';
import { of } from 'rxjs';

describe('AdminDashboardComponent', () => {
    let component: AdminDashboardComponent;
    let fixture: ComponentFixture<AdminDashboardComponent>;
    let mockAdminService: jasmine.SpyObj<AdminService>;

    beforeEach(async () => {
        mockAdminService = jasmine.createSpyObj('AdminService', [
            'runPersonalFolderMigration',
            'createPersonalFolderForUser',
        ]);

        await TestBed.configureTestingModule({
            imports: [AdminDashboardComponent, RouterTestingModule],
            providers: [
                { provide: AdminService, useValue: mockAdminService },
                { provide: AuthService, useValue: { isAdmin: () => false } },
            ],
        }).compileComponents();
    });

    it('should display unauthorized message when not admin', () => {
        fixture = TestBed.createComponent(AdminDashboardComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        const compiled = fixture.nativeElement as HTMLElement;
        expect(compiled.textContent).toContain(
            'You are not authorized to view admin tools.',
        );
    });

    it('should show run button for admin and call migration on click', () => {
        // replace AuthService with admin true
        TestBed.overrideProvider(AuthService, {
            useValue: { isAdmin: () => true },
        });
        mockAdminService.runPersonalFolderMigration.and.returnValue(
            of({ success: true, message: 'done', data: 1 }),
        );

        fixture = TestBed.createComponent(AdminDashboardComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        const compiled = fixture.nativeElement as HTMLElement;
        const button = compiled.querySelector('button');
        expect(button).toBeTruthy();

        // Click and assert the service was called
        (button as HTMLButtonElement).click();
        expect(mockAdminService.runPersonalFolderMigration).toHaveBeenCalled();
    });

    it('should call createPersonalFolderForUser when clicking create button', () => {
        TestBed.overrideProvider(AuthService, {
            useValue: { isAdmin: () => true },
        });
        mockAdminService.createPersonalFolderForUser.and.returnValue(
            of({ success: true, message: 'created', data: true }),
        );

        fixture = TestBed.createComponent(AdminDashboardComponent);
        component = fixture.componentInstance;
        fixture.detectChanges();

        // set a user id in the input and click the create button
        const compiled = fixture.nativeElement as HTMLElement;
        const input = compiled.querySelector(
            '#userIdInput',
        ) as HTMLInputElement;
        const createButton = compiled.querySelector(
            '#createPersonalFolderBtn',
        ) as HTMLButtonElement;
        expect(input).toBeTruthy();
        expect(createButton).toBeTruthy();

        input.value = '00000000-0000-0000-0000-000000000000';
        input.dispatchEvent(new Event('input'));
        fixture.detectChanges();

        createButton.click();
        expect(
            mockAdminService.createPersonalFolderForUser,
        ).toHaveBeenCalledWith('00000000-0000-0000-0000-000000000000');
    });
});
