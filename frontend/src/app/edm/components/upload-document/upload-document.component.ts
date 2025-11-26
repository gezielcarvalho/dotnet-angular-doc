import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
    FormBuilder,
    FormGroup,
    ReactiveFormsModule,
    Validators,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { DocumentService } from '../../../shared/services/document.service';
import { FolderService } from '../../../shared/services/folder.service';
import { TagService } from '../../../shared/services/tag.service';
import {
    FolderDTO,
    TagDTO,
    CreateDocumentRequest,
} from '../../../shared/models/edm.models';

@Component({
    selector: 'app-upload-document',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    templateUrl: './upload-document.component.html',
    styleUrls: ['./upload-document.component.css'],
})
export class UploadDocumentComponent implements OnInit {
    uploadForm!: FormGroup;
    selectedFile: File | null = null;
    selectedFileName = '';
    isLoading = false;
    errorMessage = '';
    uploadProgress = 0;

    folders: FolderDTO[] = [];
    tags: TagDTO[] = [];
    selectedTagIds: string[] = [];

    currentFolderId: string | null = null;

    // File validation
    maxFileSize = 100 * 1024 * 1024; // 100MB
    allowedExtensions = [
        '.pdf',
        '.doc',
        '.docx',
        '.xls',
        '.xlsx',
        '.ppt',
        '.pptx',
        '.txt',
        '.jpg',
        '.jpeg',
        '.png',
        '.gif',
        '.zip',
        '.rar',
    ];

    constructor(
        private fb: FormBuilder,
        private documentService: DocumentService,
        private folderService: FolderService,
        private tagService: TagService,
        private router: Router,
        private route: ActivatedRoute,
    ) {}

    ngOnInit(): void {
        this.uploadForm = this.fb.group({
            title: ['', [Validators.required, Validators.maxLength(255)]],
            description: ['', [Validators.maxLength(1000)]],
            folderId: ['', [Validators.required]],
        });

        // Check for folder ID in query params
        this.route.queryParams.subscribe(params => {
            this.currentFolderId = params['folderId'] || null;
            if (this.currentFolderId) {
                this.uploadForm.patchValue({ folderId: this.currentFolderId });
            }
        });

        this.loadFolders();
        this.loadTags();
    }

    loadFolders(): void {
        this.folderService.getRootFolders().subscribe({
            next: response => {
                if (response.success && response.data) {
                    this.folders = response.data;
                    // Load subfolders recursively if needed
                    this.loadAllFolders();
                }
            },
            error: error => {
                console.error('Load folders error:', error);
            },
        });
    }

    loadAllFolders(): void {
        // For simplicity, we'll just show root folders
        // In a real app, you might want to load all folders or implement a tree picker
        this.folderService.getFolders().subscribe({
            next: response => {
                if (response.success && response.data) {
                    this.folders = response.data;
                }
            },
        });
    }

    loadTags(): void {
        this.tagService.getTags().subscribe({
            next: response => {
                if (response.success && response.data) {
                    this.tags = response.data;
                }
            },
            error: error => {
                console.error('Load tags error:', error);
            },
        });
    }

    onFileSelected(event: Event): void {
        const input = event.target as HTMLInputElement;
        if (input.files && input.files.length > 0) {
            const file = input.files[0];

            // Validate file extension
            const fileExtension = this.getFileExtension(file.name);
            if (!this.allowedExtensions.includes(fileExtension.toLowerCase())) {
                this.errorMessage = `File type ${fileExtension} is not allowed. Allowed types: ${this.allowedExtensions.join(
                    ', ',
                )}`;
                this.selectedFile = null;
                this.selectedFileName = '';
                input.value = '';
                return;
            }

            // Validate file size
            if (file.size > this.maxFileSize) {
                this.errorMessage = `File size exceeds maximum allowed size of ${this.formatFileSize(
                    this.maxFileSize,
                )}`;
                this.selectedFile = null;
                this.selectedFileName = '';
                input.value = '';
                return;
            }

            this.selectedFile = file;
            this.selectedFileName = file.name;
            this.errorMessage = '';

            // Auto-fill title if empty
            if (!this.uploadForm.value.title) {
                const titleWithoutExt = file.name.replace(/\.[^/.]+$/, '');
                this.uploadForm.patchValue({ title: titleWithoutExt });
            }
        }
    }

    onTagToggle(tagId: string): void {
        const index = this.selectedTagIds.indexOf(tagId);
        if (index > -1) {
            this.selectedTagIds.splice(index, 1);
        } else {
            this.selectedTagIds.push(tagId);
        }
    }

    isTagSelected(tagId: string): boolean {
        return this.selectedTagIds.includes(tagId);
    }

    onSubmit(): void {
        if (this.uploadForm.invalid) {
            this.uploadForm.markAllAsTouched();
            return;
        }

        if (!this.selectedFile) {
            this.errorMessage = 'Please select a file to upload';
            return;
        }

        this.isLoading = true;
        this.errorMessage = '';
        this.uploadProgress = 0;

        const request: CreateDocumentRequest = {
            title: this.uploadForm.value.title,
            description: this.uploadForm.value.description || undefined,
            folderId: this.uploadForm.value.folderId,
            tagIds:
                this.selectedTagIds.length > 0
                    ? this.selectedTagIds
                    : undefined,
            file: this.selectedFile,
        };

        this.documentService.createDocument(request).subscribe({
            next: response => {
                this.isLoading = false;
                if (response.success && response.data) {
                    // Navigate back to document list or to the new document
                    this.router.navigate(['/edm/documents'], {
                        queryParams: { folderId: request.folderId },
                    });
                } else {
                    this.errorMessage =
                        response.message || 'Failed to upload document';
                }
            },
            error: error => {
                this.isLoading = false;
                this.errorMessage =
                    'An error occurred while uploading document';
                console.error('Upload error:', error);
            },
        });
    }

    cancel(): void {
        if (this.currentFolderId) {
            this.router.navigate(['/edm/documents'], {
                queryParams: { folderId: this.currentFolderId },
            });
        } else {
            this.router.navigate(['/edm/documents']);
        }
    }

    getFileExtension(filename: string): string {
        const lastDot = filename.lastIndexOf('.');
        return lastDot > -1 ? filename.substring(lastDot) : '';
    }

    formatFileSize(bytes: number): string {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return (
            Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i]
        );
    }

    get selectedFileSize(): string {
        return this.selectedFile
            ? this.formatFileSize(this.selectedFile.size)
            : '';
    }

    isFieldInvalid(fieldName: string): boolean {
        const field = this.uploadForm.get(fieldName);
        return !!(field && field.invalid && (field.dirty || field.touched));
    }

    getFieldError(fieldName: string): string {
        const field = this.uploadForm.get(fieldName);
        if (field?.hasError('required')) {
            return `${this.getFieldLabel(fieldName)} is required`;
        }
        if (field?.hasError('maxlength')) {
            const maxLength = field.errors?.['maxlength'].requiredLength;
            return `${this.getFieldLabel(
                fieldName,
            )} must not exceed ${maxLength} characters`;
        }
        return '';
    }

    getFieldLabel(fieldName: string): string {
        const labels: { [key: string]: string } = {
            title: 'Title',
            description: 'Description',
            folderId: 'Folder',
        };
        return labels[fieldName] || fieldName;
    }
}
