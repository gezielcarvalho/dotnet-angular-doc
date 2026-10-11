# EDM System - Database Schema Design

## Schema Overview

The database schema follows a normalized relational design optimized for document management, security, and audit compliance. All tables include audit fields (CreatedAt, CreatedBy, ModifiedAt, ModifiedBy) for complete traceability.

## Entity Relationship Diagram (ERD)

```
┌─────────────┐         ┌──────────────┐         ┌─────────────┐
│    User     │────────<│  Permission  │>────────│   Folder    │
│             │         │              │         │             │
│ - Id        │         │ - Id         │         │ - Id        │
│ - Username  │         │ - UserId     │         │ - Name      │
│ - Email     │         │ - FolderId   │         │ - ParentId  │
│ - Role      │         │ - DocumentId │         │ - Path      │
└──────┬──────┘         │ - PermType   │         └──────┬──────┘
       │                └──────────────┘                │
       │                                                │
       │                ┌──────────────┐                │
       │                │   Document   │<───────────────┘
       │                │              │
       └───────────────>│ - Id         │
                        │ - Title      │
                        │ - FolderId   │
                        │ - FilePath   │
                        │ - FileSize   │
                        │ - MimeType   │
                        │ - Status     │
                        │ - OwnerId    │
                        └───┬──────────┘
                            │
            ┌───────────────┼───────────────┐
            │               │               │
     ┌──────▼──────┐ ┌─────▼──────┐ ┌─────▼──────┐
     │  Version    │ │    Tag     │ │  Comment   │
     │             │ │            │ │            │
     │ - Id        │ │ - Id       │ │ - Id       │
     │ - DocId     │ │ - Name     │ │ - DocId    │
     │ - VersionNo │ └────────────┘ │ - UserId   │
     │ - FilePath  │                │ - Text     │
     └─────────────┘                └────────────┘
            │
            │         ┌──────────────┐
            └────────>│  DocumentTag │
                      │              │
                      │ - DocumentId │
                      │ - TagId      │
                      └──────────────┘

     ┌──────────────┐         ┌──────────────┐
     │   Workflow   │<───────>│WorkflowStep  │
     │              │         │              │
     │ - Id         │         │ - Id         │
     │ - Name       │         │ - WorkflowId │
     │ - DocumentId │         │ - StepOrder  │
     │ - Status     │         │ - AssignedTo │
     │ - CreatedBy  │         │ - Status     │
     └──────────────┘         └──────────────┘

     ┌──────────────┐
     │  AuditLog    │
     │              │
     │ - Id         │
     │ - UserId     │
     │ - DocumentId │
     │ - Action     │
     │ - IPAddress  │
     │ - Timestamp  │
     └──────────────┘
```

## Core Tables

### 1. Users

Stores user authentication and profile information.

```sql
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Department NVARCHAR(100) NULL,
    Role NVARCHAR(50) NOT NULL DEFAULT 'User', -- Admin, Manager, User, Viewer
    IsActive BIT NOT NULL DEFAULT 1,
    LastLoginAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy UNIQUEIDENTIFIER NULL,

    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin', 'Manager', 'User', 'Viewer')),
    CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Users_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Role ON Users(Role);
```

### 2. Folders

Hierarchical folder structure for organizing documents.

```sql
CREATE TABLE Folders (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000) NULL,
    ParentFolderId UNIQUEIDENTIFIER NULL,
    Path NVARCHAR(2000) NOT NULL, -- Full path for quick lookups (e.g., /Root/IT/Policies)
    Level INT NOT NULL DEFAULT 0, -- Depth in hierarchy (0 = root)
    IsSystemFolder BIT NOT NULL DEFAULT 0, -- System folders cannot be deleted
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy UNIQUEIDENTIFIER NULL,

    CONSTRAINT FK_Folders_Parent FOREIGN KEY (ParentFolderId) REFERENCES Folders(Id),
    CONSTRAINT FK_Folders_Owner FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    CONSTRAINT FK_Folders_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Folders_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Folders_ParentId ON Folders(ParentFolderId);
CREATE INDEX IX_Folders_Path ON Folders(Path);
CREATE INDEX IX_Folders_OwnerId ON Folders(OwnerId);
```

### 3. Documents

Core document metadata and references.

```sql
CREATE TABLE Documents (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Title NVARCHAR(500) NOT NULL,
    Description NVARCHAR(2000) NULL,
    FolderId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL, -- Relative path to file on disk
    FileSize BIGINT NOT NULL, -- Size in bytes
    MimeType NVARCHAR(100) NOT NULL,
    FileExtension NVARCHAR(50) NOT NULL,
    Checksum NVARCHAR(64) NOT NULL, -- SHA-256 hash for integrity

    -- Version Control
    CurrentVersion INT NOT NULL DEFAULT 1,
    IsLatestVersion BIT NOT NULL DEFAULT 1,

    -- Status & Lifecycle
    Status NVARCHAR(50) NOT NULL DEFAULT 'Draft', -- Draft, Published, Archived, Deleted

    -- Ownership & Access
    OwnerId UNIQUEIDENTIFIER NOT NULL,
    IsPublic BIT NOT NULL DEFAULT 0,

    -- Metadata
    Tags NVARCHAR(500) NULL, -- Comma-separated for quick filtering
    CustomMetadata NVARCHAR(MAX) NULL, -- JSON for flexible fields

    -- Retention & Compliance
    RetentionDate DATETIME2 NULL, -- Date when document can be deleted
    ExpirationDate DATETIME2 NULL, -- Date when document expires

    -- Statistics
    ViewCount INT NOT NULL DEFAULT 0,
    DownloadCount INT NOT NULL DEFAULT 0,

    -- Audit Fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    ModifiedAt DATETIME2 NULL,
    ModifiedBy UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    DeletedBy UNIQUEIDENTIFIER NULL,

    CONSTRAINT FK_Documents_Folder FOREIGN KEY (FolderId) REFERENCES Folders(Id),
    CONSTRAINT FK_Documents_Owner FOREIGN KEY (OwnerId) REFERENCES Users(Id),
    CONSTRAINT FK_Documents_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Documents_ModifiedBy FOREIGN KEY (ModifiedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Documents_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES Users(Id),
    CONSTRAINT CK_Documents_Status CHECK (Status IN ('Draft', 'Published', 'Archived', 'Deleted'))
);

CREATE INDEX IX_Documents_FolderId ON Documents(FolderId);
CREATE INDEX IX_Documents_OwnerId ON Documents(OwnerId);
CREATE INDEX IX_Documents_Status ON Documents(Status);
CREATE INDEX IX_Documents_CreatedAt ON Documents(CreatedAt);
CREATE INDEX IX_Documents_FileName ON Documents(FileName);

-- Full-Text Search Index
CREATE FULLTEXT CATALOG DocumentCatalog AS DEFAULT;
CREATE FULLTEXT INDEX ON Documents(Title, Description, Tags)
    KEY INDEX PK__Documents ON DocumentCatalog;
```

### 4. DocumentVersions

Version history for documents.

```sql
CREATE TABLE DocumentVersions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    VersionNumber INT NOT NULL,
    FilePath NVARCHAR(1000) NOT NULL,
    FileSize BIGINT NOT NULL,
    Checksum NVARCHAR(64) NOT NULL,
    VersionComment NVARCHAR(1000) NULL,
    IsCurrentVersion BIT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT FK_Versions_Document FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Versions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT UQ_Versions_DocVersion UNIQUE (DocumentId, VersionNumber)
);

CREATE INDEX IX_Versions_DocumentId ON DocumentVersions(DocumentId);
CREATE INDEX IX_Versions_CreatedAt ON DocumentVersions(CreatedAt);
```

### 5. Tags

Reusable tags for document categorization.

```sql
CREATE TABLE Tags (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500) NULL,
    Color NVARCHAR(7) NULL, -- Hex color code (e.g., #FF5733)
    UsageCount INT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,

    CONSTRAINT FK_Tags_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);

CREATE INDEX IX_Tags_Name ON Tags(Name);
```

### 6. DocumentTags

Many-to-many relationship between documents and tags.

```sql
CREATE TABLE DocumentTags (
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    TagId UNIQUEIDENTIFIER NOT NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,

    PRIMARY KEY (DocumentId, TagId),
    CONSTRAINT FK_DocTags_Document FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DocTags_Tag FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE,
    CONSTRAINT FK_DocTags_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);
```

### 7. Permissions

Granular access control for folders and documents.

```sql
CREATE TABLE Permissions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

    -- Target (Folder or Document)
    FolderId UNIQUEIDENTIFIER NULL,
    DocumentId UNIQUEIDENTIFIER NULL,

    -- Subject (User or Group - we'll use User for now)
    UserId UNIQUEIDENTIFIER NOT NULL,

    -- Permission Type
    PermissionType NVARCHAR(50) NOT NULL, -- Read, Write, Delete, Share, Admin

    -- Inheritance
    IsInherited BIT NOT NULL DEFAULT 0,
    InheritedFromFolderId UNIQUEIDENTIFIER NULL,

    -- Audit
    GrantedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    GrantedBy UNIQUEIDENTIFIER NOT NULL,
    ExpiresAt DATETIME2 NULL,

    CONSTRAINT FK_Permissions_Folder FOREIGN KEY (FolderId) REFERENCES Folders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Permissions_Document FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Permissions_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Permissions_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Permissions_InheritedFrom FOREIGN KEY (InheritedFromFolderId) REFERENCES Folders(Id),
    CONSTRAINT CK_Permissions_Target CHECK ((FolderId IS NOT NULL AND DocumentId IS NULL) OR (FolderId IS NULL AND DocumentId IS NOT NULL)),
    CONSTRAINT CK_Permissions_Type CHECK (PermissionType IN ('Read', 'Write', 'Delete', 'Share', 'Admin'))
);

CREATE INDEX IX_Permissions_FolderId ON Permissions(FolderId);
CREATE INDEX IX_Permissions_DocumentId ON Permissions(DocumentId);
CREATE INDEX IX_Permissions_UserId ON Permissions(UserId);
```

### 8. Comments

Document comments and annotations.

```sql
CREATE TABLE Comments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    ParentCommentId UNIQUEIDENTIFIER NULL, -- For threaded comments
    UserId UNIQUEIDENTIFIER NOT NULL,
    Text NVARCHAR(2000) NOT NULL,
    IsResolved BIT NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ModifiedAt DATETIME2 NULL,

    CONSTRAINT FK_Comments_Document FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Comments_Parent FOREIGN KEY (ParentCommentId) REFERENCES Comments(Id),
    CONSTRAINT FK_Comments_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Comments_DocumentId ON Comments(DocumentId);
CREATE INDEX IX_Comments_UserId ON Comments(UserId);
```

### 9. Workflows

Document approval workflows.

```sql
CREATE TABLE Workflows (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000) NULL,
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    WorkflowType NVARCHAR(50) NOT NULL, -- Approval, Review, Publishing
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, InProgress, Approved, Rejected, Cancelled
    CurrentStepOrder INT NOT NULL DEFAULT 1,
    DueDate DATETIME2 NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CompletedAt DATETIME2 NULL,
    CompletedBy UNIQUEIDENTIFIER NULL,

    CONSTRAINT FK_Workflows_Document FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
    CONSTRAINT FK_Workflows_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Workflows_CompletedBy FOREIGN KEY (CompletedBy) REFERENCES Users(Id),
    CONSTRAINT CK_Workflows_Type CHECK (WorkflowType IN ('Approval', 'Review', 'Publishing')),
    CONSTRAINT CK_Workflows_Status CHECK (Status IN ('Pending', 'InProgress', 'Approved', 'Rejected', 'Cancelled'))
);

CREATE INDEX IX_Workflows_DocumentId ON Workflows(DocumentId);
CREATE INDEX IX_Workflows_Status ON Workflows(Status);
```

### 10. WorkflowSteps

Individual steps in a workflow.

```sql
CREATE TABLE WorkflowSteps (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    WorkflowId UNIQUEIDENTIFIER NOT NULL,
    StepOrder INT NOT NULL,
    StepName NVARCHAR(255) NOT NULL,
    AssignedToUserId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, InProgress, Approved, Rejected, Skipped
    Comment NVARCHAR(1000) NULL,
    DueDate DATETIME2 NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedAt DATETIME2 NULL,
    CompletedBy UNIQUEIDENTIFIER NULL,

    CONSTRAINT FK_WorkflowSteps_Workflow FOREIGN KEY (WorkflowId) REFERENCES Workflows(Id) ON DELETE CASCADE,
    CONSTRAINT FK_WorkflowSteps_AssignedTo FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id),
    CONSTRAINT FK_WorkflowSteps_CompletedBy FOREIGN KEY (CompletedBy) REFERENCES Users(Id),
    CONSTRAINT CK_WorkflowSteps_Status CHECK (Status IN ('Pending', 'InProgress', 'Approved', 'Rejected', 'Skipped')),
    CONSTRAINT UQ_WorkflowSteps_Order UNIQUE (WorkflowId, StepOrder)
);

CREATE INDEX IX_WorkflowSteps_WorkflowId ON WorkflowSteps(WorkflowId);
CREATE INDEX IX_WorkflowSteps_AssignedTo ON WorkflowSteps(AssignedToUserId);
```

### 11. AuditLogs

Complete audit trail of all system activities.

```sql
CREATE TABLE AuditLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NULL, -- NULL for system actions
    Action NVARCHAR(100) NOT NULL, -- View, Download, Edit, Delete, Share, Upload, etc.
    EntityType NVARCHAR(50) NOT NULL, -- Document, Folder, User, Permission, etc.
    EntityId UNIQUEIDENTIFIER NULL,
    Details NVARCHAR(MAX) NULL, -- JSON for additional context
    IPAddress NVARCHAR(45) NULL, -- IPv6 compatible
    UserAgent NVARCHAR(500) NULL,
    Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType, EntityId);
CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs(Timestamp);
CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
```

### 12. Notifications

User notifications for document activities.

```sql
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- DocumentShared, WorkflowAssigned, CommentAdded, etc.
    Title NVARCHAR(255) NOT NULL,
    Message NVARCHAR(1000) NULL,
    EntityType NVARCHAR(50) NULL, -- Document, Workflow, Comment
    EntityId UNIQUEIDENTIFIER NULL,
    IsRead BIT NOT NULL DEFAULT 0,
    ReadAt DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),

    CONSTRAINT FK_Notifications_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
CREATE INDEX IX_Notifications_IsRead ON Notifications(IsRead);
CREATE INDEX IX_Notifications_CreatedAt ON Notifications(CreatedAt);
```

## Views for Common Queries

### Active Documents View

```sql
CREATE VIEW vw_ActiveDocuments AS
SELECT
    d.Id,
    d.Title,
    d.FileName,
    d.FileSize,
    d.MimeType,
    d.Status,
    d.CurrentVersion,
    d.CreatedAt,
    f.Path AS FolderPath,
    u.FirstName + ' ' + u.LastName AS OwnerName,
    d.ViewCount,
    d.DownloadCount
FROM Documents d
INNER JOIN Folders f ON d.FolderId = f.Id
INNER JOIN Users u ON d.OwnerId = u.Id
WHERE d.Status != 'Deleted' AND d.DeletedAt IS NULL;
```

### User Permissions View

```sql
CREATE VIEW vw_UserDocumentPermissions AS
SELECT
    p.UserId,
    d.Id AS DocumentId,
    d.Title,
    p.PermissionType,
    p.IsInherited,
    f.Path AS FolderPath
FROM Permissions p
LEFT JOIN Documents d ON p.DocumentId = d.Id
LEFT JOIN Folders f ON p.FolderId = f.Id OR d.FolderId = f.Id
WHERE p.ExpiresAt IS NULL OR p.ExpiresAt > GETUTCDATE();
```

## Stored Procedures

### Get Document with Permissions

```sql
CREATE PROCEDURE sp_GetDocumentWithPermissions
    @DocumentId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    -- Check if user has permission to view document
    IF EXISTS (
        SELECT 1 FROM Permissions
        WHERE (DocumentId = @DocumentId OR FolderId IN (
            SELECT FolderId FROM Documents WHERE Id = @DocumentId
        ))
        AND UserId = @UserId
        AND PermissionType IN ('Read', 'Write', 'Admin')
    ) OR EXISTS (
        SELECT 1 FROM Documents WHERE Id = @DocumentId AND OwnerId = @UserId
    )
    BEGIN
        SELECT * FROM vw_ActiveDocuments WHERE Id = @DocumentId;
    END
    ELSE
    BEGIN
        RAISERROR('Access denied', 16, 1);
    END
END;
```

## Indexes Strategy

**Performance Considerations:**

- Foreign key columns are indexed automatically
- Commonly filtered columns (Status, CreatedAt, etc.) have dedicated indexes
- Full-text search for document content and metadata
- Composite indexes for common query patterns

**Maintenance:**

- Regular index fragmentation checks
- Statistics updates weekly
- Archive old audit logs quarterly

## Data Retention Policy

```sql
-- Soft delete old audit logs (keep 2 years)
CREATE TABLE AuditLogsArchive (LIKE AuditLogs);

-- Scheduled job to move old logs
CREATE PROCEDURE sp_ArchiveOldAuditLogs
AS
BEGIN
    INSERT INTO AuditLogsArchive
    SELECT * FROM AuditLogs
    WHERE Timestamp < DATEADD(YEAR, -2, GETUTCDATE());

    DELETE FROM AuditLogs
    WHERE Timestamp < DATEADD(YEAR, -2, GETUTCDATE());
END;
```

## Migration Strategy

### Phase 1: Core Tables

1. Users
2. Folders
3. Documents
4. DocumentVersions

### Phase 2: Security & Metadata

5. Tags
6. DocumentTags
7. Permissions

### Phase 3: Collaboration

8. Comments
9. Notifications

### Phase 4: Workflows

10. Workflows
11. WorkflowSteps

### Phase 5: Compliance

12. AuditLogs

## Sample Data Seeding

```sql
-- Default Admin User
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, Role)
VALUES ('admin', 'admin@edm.local', 'hashed_password_here', 'System', 'Administrator', 'Admin');

-- Root Folder
INSERT INTO Folders (Name, Path, Level, IsSystemFolder, OwnerId, CreatedBy)
VALUES ('Root', '/', 0, 1, @AdminUserId, @AdminUserId);

-- Default Tags
INSERT INTO Tags (Name, Color, CreatedBy)
VALUES
    ('Important', '#FF0000', @AdminUserId),
    ('Confidential', '#FFA500', @AdminUserId),
    ('Draft', '#808080', @AdminUserId),
    ('Final', '#008000', @AdminUserId);
```

---

**Document Version**: 1.0  
**Last Updated**: November 25, 2025  
**Author**: EDM Project Team  
**Status**: Design Complete - Ready for Review
