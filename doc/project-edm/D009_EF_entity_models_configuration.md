# EDM System - C# Entity Models & EF Configuration

## Entity Models Design

This document defines the C# entity classes and Entity Framework Core configuration for the EDM system. All entities follow best practices with proper navigation properties, data annotations, and fluent API configurations.

## Base Entity Classes

### BaseEntity (Abstract)

```csharp
using System;

namespace Backend.Models.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public Guid? ModifiedBy { get; set; }

        // Navigation properties
        public virtual User CreatedByUser { get; set; }
        public virtual User ModifiedByUser { get; set; }
    }
}
```

### SoftDeletableEntity (Abstract)

```csharp
namespace Backend.Models.Common
{
    public abstract class SoftDeletableEntity : BaseEntity
    {
        public DateTime? DeletedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public bool IsDeleted => DeletedAt.HasValue;

        // Navigation property
        public virtual User DeletedByUser { get; set; }
    }
}
```

## Core Entities

### 1. User

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Department { get; set; }

        [Required]
        [MaxLength(50)]
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        // Computed property
        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual ICollection<Document> OwnedDocuments { get; set; }
        public virtual ICollection<Folder> OwnedFolders { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Workflow> CreatedWorkflows { get; set; }
        public virtual ICollection<WorkflowStep> AssignedWorkflowSteps { get; set; }
        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }

    public enum UserRole
    {
        Admin,
        Manager,
        User,
        Viewer
    }
}
```

### 2. Folder

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class Folder : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        public Guid? ParentFolderId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Path { get; set; }

        public int Level { get; set; } = 0;

        public bool IsSystemFolder { get; set; } = false;

        [Required]
        public Guid OwnerId { get; set; }

        // Navigation properties
        public virtual Folder ParentFolder { get; set; }
        public virtual ICollection<Folder> SubFolders { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual User Owner { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }
    }
}
```

### 3. Document

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class Document : SoftDeletableEntity
    {
        [Required]
        [MaxLength(500)]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        public Guid FolderId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        [Required]
        [MaxLength(100)]
        public string MimeType { get; set; }

        [Required]
        [MaxLength(50)]
        public string FileExtension { get; set; }

        [Required]
        [MaxLength(64)]
        public string Checksum { get; set; }

        // Version Control
        public int CurrentVersion { get; set; } = 1;
        public bool IsLatestVersion { get; set; } = true;

        // Status & Lifecycle
        [Required]
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        // Ownership & Access
        [Required]
        public Guid OwnerId { get; set; }
        public bool IsPublic { get; set; } = false;

        // Metadata
        [MaxLength(500)]
        public string Tags { get; set; }

        public string CustomMetadata { get; set; } // JSON

        // Retention & Compliance
        public DateTime? RetentionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        // Statistics
        public int ViewCount { get; set; } = 0;
        public int DownloadCount { get; set; } = 0;

        // Navigation properties
        public virtual Folder Folder { get; set; }
        public virtual User Owner { get; set; }
        public virtual ICollection<DocumentVersion> Versions { get; set; }
        public virtual ICollection<DocumentTag> DocumentTags { get; set; }
        public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Workflow> Workflows { get; set; }
    }

    public enum DocumentStatus
    {
        Draft,
        Published,
        Archived,
        Deleted
    }
}
```

### 4. DocumentVersion

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class DocumentVersion : BaseEntity
    {
        [Required]
        public Guid DocumentId { get; set; }

        public int VersionNumber { get; set; }

        [Required]
        [MaxLength(1000)]
        public string FilePath { get; set; }

        public long FileSize { get; set; }

        [Required]
        [MaxLength(64)]
        public string Checksum { get; set; }

        [MaxLength(1000)]
        public string VersionComment { get; set; }

        public bool IsCurrentVersion { get; set; } = false;

        // Navigation properties
        public virtual Document Document { get; set; }
    }
}
```

### 5. Tag

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class Tag : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(7)]
        public string Color { get; set; } // Hex color code

        public int UsageCount { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<DocumentTag> DocumentTags { get; set; }
    }
}
```

### 6. DocumentTag (Join Table)

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class DocumentTag
    {
        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public Guid TagId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }

        // Navigation properties
        public virtual Document Document { get; set; }
        public virtual Tag Tag { get; set; }
        public virtual User CreatedByUser { get; set; }
    }
}
```

### 7. Permission

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Permission
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Target (Folder or Document)
        public Guid? FolderId { get; set; }
        public Guid? DocumentId { get; set; }

        // Subject
        [Required]
        public Guid UserId { get; set; }

        // Permission Type
        [Required]
        public PermissionType PermissionType { get; set; }

        // Inheritance
        public bool IsInherited { get; set; } = false;
        public Guid? InheritedFromFolderId { get; set; }

        // Audit
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public Guid GrantedBy { get; set; }
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        public virtual Folder Folder { get; set; }
        public virtual Document Document { get; set; }
        public virtual User User { get; set; }
        public virtual User GrantedByUser { get; set; }
        public virtual Folder InheritedFromFolder { get; set; }
    }

    public enum PermissionType
    {
        Read,
        Write,
        Delete,
        Share,
        Admin
    }
}
```

### 8. Comment

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class Comment : BaseEntity
    {
        [Required]
        public Guid DocumentId { get; set; }

        public Guid? ParentCommentId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Text { get; set; }

        public bool IsResolved { get; set; } = false;

        public DateTime? ModifiedAt { get; set; }

        // Navigation properties
        public virtual Document Document { get; set; }
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; }
        public virtual User User { get; set; }
    }
}
```

### 9. Workflow

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class Workflow : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public Guid DocumentId { get; set; }

        [Required]
        public WorkflowType WorkflowType { get; set; }

        [Required]
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;

        public int CurrentStepOrder { get; set; } = 1;

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedBy { get; set; }

        // Navigation properties
        public virtual Document Document { get; set; }
        public virtual User CompletedByUser { get; set; }
        public virtual ICollection<WorkflowStep> Steps { get; set; }
    }

    public enum WorkflowType
    {
        Approval,
        Review,
        Publishing
    }

    public enum WorkflowStatus
    {
        Pending,
        InProgress,
        Approved,
        Rejected,
        Cancelled
    }
}
```

### 10. WorkflowStep

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using Backend.Models.Common;

namespace Backend.Models
{
    public class WorkflowStep : BaseEntity
    {
        [Required]
        public Guid WorkflowId { get; set; }

        public int StepOrder { get; set; }

        [Required]
        [MaxLength(255)]
        public string StepName { get; set; }

        [Required]
        public Guid AssignedToUserId { get; set; }

        [Required]
        public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;

        [MaxLength(1000)]
        public string Comment { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }
        public Guid? CompletedBy { get; set; }

        // Navigation properties
        public virtual Workflow Workflow { get; set; }
        public virtual User AssignedToUser { get; set; }
        public virtual User CompletedByUser { get; set; }
    }

    public enum WorkflowStepStatus
    {
        Pending,
        InProgress,
        Approved,
        Rejected,
        Skipped
    }
}
```

### 11. AuditLog

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid? UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; }

        [Required]
        [MaxLength(50)]
        public string EntityType { get; set; }

        public Guid? EntityId { get; set; }

        public string Details { get; set; } // JSON

        [MaxLength(45)]
        public string IPAddress { get; set; }

        [MaxLength(500)]
        public string UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; }
    }
}
```

### 12. Notification

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(1000)]
        public string Message { get; set; }

        [MaxLength(50)]
        public string EntityType { get; set; }

        public Guid? EntityId { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; }
    }
}
```

## Entity Framework DbContext

### EdmDbContext

```csharp
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using Backend.Models.Common;

namespace Backend.Data
{
    public class EdmDbContext : DbContext
    {
        public EdmDbContext(DbContextOptions<EdmDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentVersion> DocumentVersions { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<WorkflowStep> WorkflowSteps { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            ConfigureUser(modelBuilder);
            ConfigureFolder(modelBuilder);
            ConfigureDocument(modelBuilder);
            ConfigureDocumentVersion(modelBuilder);
            ConfigureTag(modelBuilder);
            ConfigureDocumentTag(modelBuilder);
            ConfigurePermission(modelBuilder);
            ConfigureComment(modelBuilder);
            ConfigureWorkflow(modelBuilder);
            ConfigureWorkflowStep(modelBuilder);
            ConfigureAuditLog(modelBuilder);
            ConfigureNotification(modelBuilder);

            // Global query filters (Soft Delete)
            modelBuilder.Entity<Document>().HasQueryFilter(d => !d.DeletedAt.HasValue);
        }

        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Role);

                entity.Property(e => e.Role)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // Self-referencing relationships
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ModifiedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ModifiedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureFolder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Folder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ParentFolderId);
                entity.HasIndex(e => e.Path);
                entity.HasIndex(e => e.OwnerId);

                // Self-referencing hierarchy
                entity.HasOne(e => e.ParentFolder)
                    .WithMany(e => e.SubFolders)
                    .HasForeignKey(e => e.ParentFolderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Owner)
                    .WithMany(e => e.OwnedFolders)
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureDocument(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.FolderId);
                entity.HasIndex(e => e.OwnerId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.FileName);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.HasOne(e => e.Folder)
                    .WithMany(e => e.Documents)
                    .HasForeignKey(e => e.FolderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Owner)
                    .WithMany(e => e.OwnedDocuments)
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureDocumentVersion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => new { e.DocumentId, e.VersionNumber }).IsUnique();

                entity.HasOne(e => e.Document)
                    .WithMany(e => e.Versions)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private void ConfigureTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });
        }

        private void ConfigureDocumentTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentTag>(entity =>
            {
                entity.HasKey(e => new { e.DocumentId, e.TagId });

                entity.HasOne(e => e.Document)
                    .WithMany(e => e.DocumentTags)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Tag)
                    .WithMany(e => e.DocumentTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigurePermission(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.FolderId);
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.PermissionType)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.HasOne(e => e.Folder)
                    .WithMany(e => e.Permissions)
                    .HasForeignKey(e => e.FolderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Document)
                    .WithMany(e => e.Permissions)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Permissions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.GrantedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.GrantedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.InheritedFromFolder)
                    .WithMany()
                    .HasForeignKey(e => e.InheritedFromFolderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.UserId);

                entity.HasOne(e => e.Document)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Self-referencing for threaded comments
                entity.HasOne(e => e.ParentComment)
                    .WithMany(e => e.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureWorkflow(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.Status);

                entity.Property(e => e.WorkflowType)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.HasOne(e => e.Document)
                    .WithMany(e => e.Workflows)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CompletedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CompletedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureWorkflowStep(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkflowStep>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WorkflowId);
                entity.HasIndex(e => e.AssignedToUserId);
                entity.HasIndex(e => new { e.WorkflowId, e.StepOrder }).IsUnique();

                entity.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.HasOne(e => e.Workflow)
                    .WithMany(e => e.Steps)
                    .HasForeignKey(e => e.WorkflowId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.AssignedToUser)
                    .WithMany(e => e.AssignedWorkflowSteps)
                    .HasForeignKey(e => e.AssignedToUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CompletedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CompletedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => new { e.EntityType, e.EntityId });

                entity.HasOne(e => e.User)
                    .WithMany(e => e.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private void ConfigureNotification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Notifications)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.ModifiedAt = DateTime.UtcNow;
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                }
            }
        }
    }
}
```

## Migration Commands

### Initial Migration

```bash
# Add migration
dotnet ef migrations add InitialCreate --context EdmDbContext

# Update database
dotnet ef database update --context EdmDbContext

# Generate SQL script
dotnet ef migrations script --context EdmDbContext --output migration.sql
```

### Seed Data

```csharp
// Add to EdmDbContext OnModelCreating
private void SeedData(ModelBuilder modelBuilder)
{
    // Default admin user
    var adminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    modelBuilder.Entity<User>().HasData(new User
    {
        Id = adminId,
        Username = "admin",
        Email = "admin@edm.local",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        FirstName = "System",
        LastName = "Administrator",
        Role = UserRole.Admin,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = adminId
    });

    // Root folder
    var rootFolderId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    modelBuilder.Entity<Folder>().HasData(new Folder
    {
        Id = rootFolderId,
        Name = "Root",
        Path = "/",
        Level = 0,
        IsSystemFolder = true,
        OwnerId = adminId,
        CreatedAt = DateTime.UtcNow,
        CreatedBy = adminId
    });

    // Default tags
    modelBuilder.Entity<Tag>().HasData(
        new Tag { Id = Guid.NewGuid(), Name = "Important", Color = "#FF0000", CreatedAt = DateTime.UtcNow, CreatedBy = adminId },
        new Tag { Id = Guid.NewGuid(), Name = "Confidential", Color = "#FFA500", CreatedAt = DateTime.UtcNow, CreatedBy = adminId },
        new Tag { Id = Guid.NewGuid(), Name = "Draft", Color = "#808080", CreatedAt = DateTime.UtcNow, CreatedBy = adminId },
        new Tag { Id = Guid.NewGuid(), Name = "Final", Color = "#008000", CreatedAt = DateTime.UtcNow, CreatedBy = adminId }
    );
}
```

## Usage Examples

### Creating a Document

```csharp
var document = new Document
{
    Title = "Project Proposal",
    Description = "Q1 2025 Project Proposal",
    FolderId = folderId,
    FileName = "proposal.pdf",
    FilePath = "/documents/2025/11/doc_123.pdf",
    FileSize = 1024000,
    MimeType = "application/pdf",
    FileExtension = ".pdf",
    Checksum = "sha256hash...",
    OwnerId = currentUserId,
    CreatedBy = currentUserId,
    Status = DocumentStatus.Draft
};

await _context.Documents.AddAsync(document);
await _context.SaveChangesAsync();
```

### Querying with Includes

```csharp
var document = await _context.Documents
    .Include(d => d.Folder)
    .Include(d => d.Owner)
    .Include(d => d.DocumentTags)
        .ThenInclude(dt => dt.Tag)
    .Include(d => d.Versions)
    .FirstOrDefaultAsync(d => d.Id == documentId);
```

---

**Document Version**: 1.0  
**Last Updated**: November 25, 2025  
**Author**: EDM Project Team  
**Status**: Design Complete - Ready for Implementation
