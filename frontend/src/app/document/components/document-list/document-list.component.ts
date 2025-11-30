import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { DocumentService } from '../../../shared/services/document.service';
import { FolderService } from '../../../shared/services/folder.service';
import { TagService } from '../../../shared/services/tag.service';
import {
    DocumentDTO,
    FolderDTO,
    TagDTO,
    PagedResponse,
    DocumentFilterParams,
    DocumentStatus,
} from '../document.models';

@Component({
    selector: 'app-document-list',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './document-list.component.html',
    styleUrls: ['./document-list.component.css'],
})
export class DocumentListComponent implements OnInit, OnDestroy {
    private destroy$ = new Subject<void>();
    private searchSubject = new Subject<string>();

    documents: DocumentDTO[] = [];
    currentFolder: FolderDTO | null = null;
    tags: TagDTO[] = [];

    isLoading = false;
    errorMessage = '';

    // Pagination
    currentPage = 1;
    pageSize = 20;
    totalPages = 0;
    totalCount = 0;

    // Filters
    searchTerm = '';
    selectedStatus = '';
    selectedTagIds: string[] = [];
    sortBy = 'createdAt';
    sortOrder: 'asc' | 'desc' = 'desc';

    // Status options
    statusOptions = [
        { value: '', label: 'All Statuses' },
        { value: DocumentStatus.Draft, label: 'Draft' },
        { value: DocumentStatus.UnderReview, label: 'Under Review' },
        { value: DocumentStatus.Approved, label: 'Approved' },
        { value: DocumentStatus.Published, label: 'Published' },
        { value: DocumentStatus.Archived, label: 'Archived' },
    ];

    constructor(
        private documentService: DocumentService,
        private folderService: FolderService,
        private tagService: TagService,
        private router: Router,
        private route: ActivatedRoute,
    ) {}

    ngOnInit(): void {
        // Setup search debounce
        this.searchSubject
            .pipe(
                debounceTime(300),
                distinctUntilChanged(),
                takeUntil(this.destroy$),
            )
            .subscribe(searchTerm => {
                this.searchTerm = searchTerm;
                this.currentPage = 1;
                this.loadDocuments();
            });

        // Load tags
        this.loadTags();

        // Check for folder parameter in route
        this.route.queryParams
            .pipe(takeUntil(this.destroy$))
            .subscribe(params => {
                const folderId = params['folderId'];
                if (folderId) {
                    this.loadFolder(folderId);
                } else {
                    this.currentFolder = null;
                }
                this.loadDocuments();
            });
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadDocuments(): void {
        this.isLoading = true;
        this.errorMessage = '';

        const filters: DocumentFilterParams = {
            search: this.searchTerm || undefined,
            folderId: this.currentFolder?.id || undefined,
            status: this.selectedStatus || undefined,
            tagIds:
                this.selectedTagIds.length > 0
                    ? this.selectedTagIds
                    : undefined,
            pageNumber: this.currentPage,
            pageSize: this.pageSize,
            sortBy: this.sortBy,
            sortOrder: this.sortOrder,
        };

        this.documentService.getDocuments(filters).subscribe({
            next: response => {
                this.isLoading = false;
                if (response.success && response.data) {
                    this.documents = response.data.items;
                    this.totalPages = response.data.totalPages;
                    this.totalCount = response.data.totalCount;
                    this.currentPage = response.data.pageNumber;
                } else {
                    this.errorMessage =
                        response.message || 'Failed to load documents';
                }
            },
            error: error => {
                this.isLoading = false;
                this.errorMessage = 'An error occurred while loading documents';
                console.error('Load documents error:', error);
            },
        });
    }

    loadFolder(folderId: string): void {
        this.folderService.getFolder(folderId).subscribe({
            next: response => {
                if (response.success && response.data) {
                    this.currentFolder = response.data;
                }
            },
            error: error => {
                console.error('Load folder error:', error);
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

    onSearchChange(searchTerm: string): void {
        this.searchSubject.next(searchTerm);
    }

    onStatusChange(): void {
        this.currentPage = 1;
        this.loadDocuments();
    }

    onTagToggle(tagId: string): void {
        const index = this.selectedTagIds.indexOf(tagId);
        if (index > -1) {
            this.selectedTagIds.splice(index, 1);
        } else {
            this.selectedTagIds.push(tagId);
        }
        this.currentPage = 1;
        this.loadDocuments();
    }

    isTagSelected(tagId: string): boolean {
        return this.selectedTagIds.includes(tagId);
    }

    clearFilters(): void {
        this.searchTerm = '';
        this.selectedStatus = '';
        this.selectedTagIds = [];
        this.currentPage = 1;
        this.loadDocuments();
    }

    onPageChange(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
            this.loadDocuments();
        }
    }

    onSort(field: string): void {
        if (this.sortBy === field) {
            this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
        } else {
            this.sortBy = field;
            this.sortOrder = 'asc';
        }
        this.loadDocuments();
    }

    viewDocument(document: DocumentDTO): void {
        this.router.navigate(['/document/documents', document.id]);
    }

    downloadDocument(document: DocumentDTO, event: Event): void {
        event.stopPropagation();

        this.documentService.downloadDocument(document.id).subscribe({
            next: blob => {
                this.documentService.triggerFileDownload(
                    blob,
                    document.fileName,
                );
            },
            error: error => {
                console.error('Download error:', error);
                alert('Failed to download document');
            },
        });
    }

    deleteDocument(document: DocumentDTO, event: Event): void {
        event.stopPropagation();

        if (confirm(`Are you sure you want to delete "${document.title}"?`)) {
            this.documentService.deleteDocument(document.id).subscribe({
                next: response => {
                    if (response.success) {
                        this.loadDocuments();
                    } else {
                        alert(response.message || 'Failed to delete document');
                    }
                },
                error: error => {
                    console.error('Delete error:', error);
                    alert('Failed to delete document');
                },
            });
        }
    }

    uploadDocument(): void {
        this.router.navigate(['/document/documents/upload'], {
            queryParams: { folderId: this.currentFolder?.id },
        });
    }

    getStatusClass(status: string): string {
        const statusClasses: { [key: string]: string } = {
            [DocumentStatus.Draft]: 'bg-gray-100 text-gray-800',
            [DocumentStatus.UnderReview]: 'bg-yellow-100 text-yellow-800',
            [DocumentStatus.Approved]: 'bg-green-100 text-green-800',
            [DocumentStatus.Published]: 'bg-blue-100 text-blue-800',
            [DocumentStatus.Archived]: 'bg-red-100 text-red-800',
        };
        return statusClasses[status] || 'bg-gray-100 text-gray-800';
    }

    formatDate(date: Date | string | undefined): string {
        if (!date) return 'N/A';
        const d = typeof date === 'string' ? new Date(date) : date;
        return d.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
        });
    }

    get paginationRange(): number[] {
        const range: number[] = [];
        const delta = 2;
        const rangeWithDots: number[] = [];

        for (
            let i = Math.max(2, this.currentPage - delta);
            i <= Math.min(this.totalPages - 1, this.currentPage + delta);
            i++
        ) {
            range.push(i);
        }

        if (this.currentPage - delta > 2) {
            rangeWithDots.push(1, -1);
        } else {
            rangeWithDots.push(1);
        }

        rangeWithDots.push(...range);

        if (this.currentPage + delta < this.totalPages - 1) {
            rangeWithDots.push(-1, this.totalPages);
        } else if (this.totalPages > 1) {
            rangeWithDots.push(this.totalPages);
        }

        return rangeWithDots;
    }
}
