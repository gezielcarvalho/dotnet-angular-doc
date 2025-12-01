import { Component, OnInit } from '@angular/core';

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
import { AuthService } from '../../../shared/services/auth.service';
import { FolderDTO, TagDTO, CreateDocumentRequest } from '../document.models';

@Component({
    selector: 'app-upload-document',
    imports: [ReactiveFormsModule],
    templateUrl: './upload-document.component.html',
    styleUrls: ['./upload-document.component.css']
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
        private authService: AuthService,
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

        // Load folders (readable). We'll mark writable folders and disable non-writable ones in the UI.
        this.loadFolders();
        this.loadTags();
    }

    loadFolders(): void {
        // Request readable folders and enable writeable ones via the CanWrite flag
        this.folderService.getFolders().subscribe({
            next: response => {
                console.debug('[UploadDocument] getFolders response', response);
                if (!response.success) {
                    this.errorMessage =
                        response.message || 'Unable to load folders.';
                    return;
                }
                if (response.data) {
                    this.folders = response.data;
                    // Try to find the special Users parent folder and add current user's personal folder to list
                    const usersParent = this.folders.find(
                        f => f.name === 'Users',
                    );
                    const currentUser = this.authService.getCurrentUserSync();
                    if (usersParent && currentUser) {
                        this.folderService
                            .getSubFolders(usersParent.id)
                            .subscribe({
                                next: subResp => {
                                    if (subResp.success && subResp.data) {
                                        const personal = subResp.data.find(
                                            sf => sf.ownerId === currentUser.id,
                                        );
                                        if (personal) {
                                            // Ensure personal folder is present in root list (so users can directly upload there)
                                            // Only add if not present already
                                            if (
                                                !this.folders.some(
                                                    f => f.id === personal.id,
                                                )
                                            ) {
                                                this.folders.push(personal);
                                            }
                                            // Auto-select personal folder if no folder selected
                                            if (
                                                !this.uploadForm.value.folderId
                                            ) {
                                                this.uploadForm.patchValue({
                                                    folderId: personal.id,
                                                });
                                            }
                                        }
                                    }
                                },
                            });
                    } else if (currentUser) {
                        // Users folder not at root; try under 'Root' as a fallback
                        const rootFolder = this.folders.find(
                            f => f.name === 'Root',
                        );
                        if (rootFolder) {
                            this.folderService
                                .getSubFolders(rootFolder.id)
                                .subscribe({
                                    next: rootSubResp => {
                                        if (
                                            rootSubResp.success &&
                                            rootSubResp.data
                                        ) {
                                            const usersFromRoot =
                                                rootSubResp.data.find(
                                                    sf => sf.name === 'Users',
                                                );
                                            if (usersFromRoot) {
                                                this.folderService
                                                    .getSubFolders(
                                                        usersFromRoot.id,
                                                    )
                                                    .subscribe({
                                                        next: usersSubResp => {
                                                            if (
                                                                usersSubResp.success &&
                                                                usersSubResp.data
                                                            ) {
                                                                const personal =
                                                                    usersSubResp.data.find(
                                                                        sf =>
                                                                            sf.ownerId ===
                                                                            currentUser.id,
                                                                    );
                                                                if (personal) {
                                                                    if (
                                                                        !this.folders.some(
                                                                            f =>
                                                                                f.id ===
                                                                                personal.id,
                                                                        )
                                                                    ) {
                                                                        this.folders.push(
                                                                            personal,
                                                                        );
                                                                    }
                                                                    if (
                                                                        !this
                                                                            .uploadForm
                                                                            .value
                                                                            .folderId
                                                                    ) {
                                                                        this.uploadForm.patchValue(
                                                                            {
                                                                                folderId:
                                                                                    personal.id,
                                                                            },
                                                                        );
                                                                    }
                                                                }
                                                            }
                                                        },
                                                        error: err =>
                                                            console.error(
                                                                '[UploadDocument] error fetching Users subfolders under Root:',
                                                                err,
                                                            ),
                                                    });
                                            }
                                        }
                                    },
                                    error: err =>
                                        console.error(
                                            '[UploadDocument] error fetching Root subfolders:',
                                            err,
                                        ),
                                });
                        }
                    }
                    console.debug(
                        '[UploadDocument] loaded folders',
                        this.folders,
                    );
                    if (this.folders.length === 0) {
                        // If no folders returned, also try to fetch only writable ones as a fallback
                        this.errorMessage =
                            'No folders were returned by the server. Trying to fetch writable folders instead.';
                        this.folderService
                            .getFolders({ requiredPermission: 'Write' })
                            .subscribe({
                                next: writeResponse => {
                                    if (
                                        writeResponse.success &&
                                        writeResponse.data &&
                                        writeResponse.data.length > 0
                                    ) {
                                        this.folders = writeResponse.data;
                                        this.errorMessage = '';
                                    } else {
                                        this.errorMessage =
                                            'No folders available to you. Contact an administrator if you think this is an error.';
                                    }
                                    console.debug(
                                        '[UploadDocument] writable fallback',
                                        writeResponse,
                                    );
                                },
                                error: err => {
                                    console.error(
                                        '[UploadDocument] writable fallback error',
                                        err,
                                    );
                                    this.errorMessage =
                                        'An error occurred while fetching writable folders. Contact your administrator.';
                                },
                            });
                    } else {
                        this.errorMessage = '';
                    }
                }
            },
            error: error => {
                console.error('Load folders error:', error);
                // Show user-friendly message
                this.errorMessage =
                    'An error occurred while loading folders. Please check your permissions or try again later.';
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
            this.handleFile(input.files[0], input);
        }
    }

    onDragOver(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
    }

    onDragLeave(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();
    }

    onDrop(event: DragEvent): void {
        event.preventDefault();
        event.stopPropagation();

        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            this.handleFile(files[0]);
        }
    }

    private handleFile(file: File, input?: HTMLInputElement): void {
        // Validate file extension
        const fileExtension = this.getFileExtension(file.name);
        if (!this.allowedExtensions.includes(fileExtension.toLowerCase())) {
            this.errorMessage = `File type ${fileExtension} is not allowed. Allowed types: ${this.allowedExtensions.join(
                ', ',
            )}`;
            this.selectedFile = null;
            this.selectedFileName = '';
            if (input) input.value = '';
            return;
        }

        // Validate file size
        if (file.size > this.maxFileSize) {
            this.errorMessage = `File size exceeds maximum allowed size of ${this.formatFileSize(
                this.maxFileSize,
            )}`;
            this.selectedFile = null;
            this.selectedFileName = '';
            if (input) input.value = '';
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

        // Check selected folder write permission (client-side safeguard)
        const selectedFolderId = this.uploadForm.value.folderId;
        const selectedFolder = this.folders.find(
            f => f.id === selectedFolderId,
        );
        if (!selectedFolder || !selectedFolder.canWrite) {
            this.isLoading = false;
            this.errorMessage =
                'You do not have write permission to the selected folder.';
            return;
        }

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
                    this.router.navigate(['/document/documents'], {
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

    selectedFolderIsWritable(): boolean {
        const selectedFolderId = this.uploadForm.value.folderId;
        if (!selectedFolderId) return false;
        const folder = this.folders.find(f => f.id === selectedFolderId);
        return !!folder && !!folder.canWrite;
    }

    get hasNoWritableFolders(): boolean {
        return this.folders.length > 0 && !this.folders.some(f => f.canWrite);
    }

    cancel(): void {
        if (this.currentFolderId) {
            this.router.navigate(['/document/documents'], {
                queryParams: { folderId: this.currentFolderId },
            });
        } else {
            this.router.navigate(['/document/documents']);
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
