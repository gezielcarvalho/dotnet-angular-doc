// ============================================================================
// Common Models
// ============================================================================

export interface PagedResponse<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    totalCount: number;
    hasPrevious: boolean;
    hasNext: boolean;
}

export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T | null;
}

// ============================================================================
// Document Models
// ============================================================================

export interface DocumentDTO {
    id: string;
    title: string;
    description?: string;
    fileName: string;
    fileExtension: string;
    fileSizeBytes: number;
    fileSizeFormatted: string;
    status: string;
    currentVersion: number;
    folderId: string;
    folderPath: string;
    ownerId: string;
    ownerName: string;
    createdAt: Date | string;
    modifiedAt?: Date | string;
    tags: string[];
    hasActiveWorkflow: boolean;
}

export interface DocumentVersionDTO {
    id: string;
    versionNumber: number;
    fileName: string;
    filePath: string;
    fileSizeBytes: number;
    changeComment?: string;
    createdAt: Date | string;
    createdBy: string;
}

export interface CreateDocumentRequest {
    title: string;
    description?: string;
    folderId: string;
    tagIds?: string[];
    file: File;
}

export interface UpdateDocumentRequest {
    title: string;
    description?: string;
    status?: string;
    tagIds?: string[];
}

export interface UploadDocumentVersionRequest {
    file: File;
    changeComment?: string;
}

// ============================================================================
// Folder Models
// ============================================================================

export interface FolderDTO {
    id: string;
    name: string;
    description?: string;
    path: string;
    level: number;
    parentFolderId?: string;
    parentFolderName?: string;
    isSystemFolder: boolean;
    ownerId: string;
    ownerName: string;
    createdAt: Date | string;
    subFolderCount: number;
    documentCount: number;
    canWrite: boolean;
}

export interface CreateFolderRequest {
    name: string;
    description?: string;
    parentFolderId?: string;
}

export interface UpdateFolderRequest {
    name: string;
    description?: string;
}

// ============================================================================
// Tag Models
// ============================================================================

export interface TagDTO {
    id: string;
    name: string;
    description?: string;
    color: string;
    documentCount: number;
}

export interface CreateTagRequest {
    name: string;
    description?: string;
    color?: string;
}

export interface UpdateTagRequest {
    name: string;
    description?: string;
    color?: string;
}

// ============================================================================
// Permission Models
// ============================================================================

export interface PermissionDTO {
    id: string;
    userId: string;
    userName: string;
    folderId?: string;
    folderPath?: string;
    documentId?: string;
    documentTitle?: string;
    permissionType: string;
    createdAt: Date | string;
}

export interface CreatePermissionRequest {
    userId: string;
    folderId?: string;
    documentId?: string;
    permissionType: PermissionType;
}

export enum PermissionType {
    Read = 'Read',
    Write = 'Write',
    Delete = 'Delete',
    Admin = 'Admin',
}

// ============================================================================
// User Models
// ============================================================================

export interface UserDTO {
    id: string;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    role: string;
    department?: string;
    isActive: boolean;
    lastLoginAt?: Date | string;
    createdAt: Date | string;
}

// ============================================================================
// Comment Models
// ============================================================================

export interface CommentDTO {
    id: string;
    content: string;
    userId: string;
    userName: string;
    parentCommentId?: string;
    createdAt: Date | string;
    modifiedAt?: Date | string;
    replies: CommentDTO[];
}

export interface CreateCommentRequest {
    content: string;
    documentId: string;
    parentCommentId?: string;
}

export interface UpdateCommentRequest {
    content: string;
}

// ============================================================================
// Notification Models
// ============================================================================

export interface NotificationDTO {
    id: string;
    type: string;
    title: string;
    message: string;
    relatedEntityId?: string;
    relatedEntityType?: string;
    isRead: boolean;
    createdAt: Date | string;
}

// ============================================================================
// Workflow Models
// ============================================================================

export interface WorkflowDTO {
    id: string;
    name: string;
    description?: string;
    documentId: string;
    documentTitle: string;
    workflowType: string;
    status: string;
    currentStepOrder: number;
    dueDate?: Date | string;
    completedAt?: Date | string;
    createdAt: Date | string;
    steps: WorkflowStepDTO[];
}

export interface WorkflowStepDTO {
    id: string;
    stepOrder: number;
    stepName: string;
    assignedToUserId: string;
    assignedToUserName: string;
    status: string;
    comment?: string;
    dueDate?: Date | string;
    completedAt?: Date | string;
    completedBy?: string;
}

export interface CreateWorkflowRequest {
    name: string;
    description?: string;
    documentId: string;
    workflowType: string;
    dueDate?: Date | string;
    steps: CreateWorkflowStepRequest[];
}

export interface CreateWorkflowStepRequest {
    stepName: string;
    assignedToUserId: string;
    stepOrder: number;
    dueDate?: Date | string;
}

// ============================================================================
// Enums for Document Status
// ============================================================================

export enum DocumentStatus {
    Draft = 'Draft',
    UnderReview = 'UnderReview',
    Approved = 'Approved',
    Published = 'Published',
    Archived = 'Archived',
}

// ============================================================================
// Filter/Query Models
// ============================================================================

export interface DocumentFilterParams {
    search?: string;
    folderId?: string;
    tagIds?: string[];
    status?: string;
    ownerId?: string;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
}

export interface FolderFilterParams {
    search?: string;
    parentFolderId?: string;
    pageNumber?: number;
    pageSize?: number;
    requiredPermission?: string;
}
