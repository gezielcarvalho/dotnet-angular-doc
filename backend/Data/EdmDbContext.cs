using Backend.Models;
using Microsoft.EntityFrameworkCore;
using EdmUser = Backend.Models.EDM.User;
using EdmTag = Backend.Models.EDM.Tag;
using Backend.Models.EDM;

namespace Backend.Data
{
    public class EdmDbContext : DbContext
    {
        public EdmDbContext(DbContextOptions<EdmDbContext> options) : base(options)
        {
        }
        
        // DbSets
        public DbSet<EdmUser> Users { get; set; } = null!;
        public DbSet<Folder> Folders { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<DocumentVersion> DocumentVersions { get; set; } = null!;
        public DbSet<EdmTag> Tags { get; set; } = null!;
        public DbSet<DocumentTag> DocumentTags { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<Workflow> Workflows { get; set; } = null!;
        public DbSet<WorkflowStep> WorkflowSteps { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User configuration
            ConfigureUser(modelBuilder);
            
            // Folder configuration
            ConfigureFolder(modelBuilder);
            
            // Document configuration
            ConfigureDocument(modelBuilder);
            
            // DocumentVersion configuration
            ConfigureDocumentVersion(modelBuilder);
            
            // Tag configuration
            ConfigureTag(modelBuilder);
            
            // DocumentTag configuration
            ConfigureDocumentTag(modelBuilder);
            
            // Permission configuration
            ConfigurePermission(modelBuilder);
            
            // Comment configuration
            ConfigureComment(modelBuilder);
            
            // Workflow configuration
            ConfigureWorkflow(modelBuilder);
            
            // WorkflowStep configuration
            ConfigureWorkflowStep(modelBuilder);
            
            // AuditLog configuration
            ConfigureAuditLog(modelBuilder);
            
            // Notification configuration
            ConfigureNotification(modelBuilder);
            
            // Global query filters for soft delete
            modelBuilder.Entity<EdmUser>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Folder>().HasQueryFilter(f => !f.IsDeleted);
            modelBuilder.Entity<Document>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<EdmTag>().HasQueryFilter(t => !t.IsDeleted);
            modelBuilder.Entity<Comment>().HasQueryFilter(c => !c.IsDeleted);
            
            // Seed data
            SeedData(modelBuilder);
        }
        
        private void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EdmUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
                
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Role);
            });
        }
        
        private void ConfigureFolder(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Folder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Path).IsRequired().HasMaxLength(2000);
                
                entity.HasOne(e => e.Owner)
                    .WithMany(u => u.OwnedFolders)
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.ParentFolder)
                    .WithMany(f => f.SubFolders)
                    .HasForeignKey(e => e.ParentFolderId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.ParentFolderId);
                entity.HasIndex(e => e.OwnerId);
                entity.HasIndex(e => e.Path);
            });
        }
        
        private void ConfigureDocument(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FileExtension).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomMetadata).HasColumnType("nvarchar(max)");
                
                entity.HasOne(e => e.Owner)
                    .WithMany(u => u.OwnedDocuments)
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Folder)
                    .WithMany(f => f.Documents)
                    .HasForeignKey(e => e.FolderId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.FolderId);
                entity.HasIndex(e => e.OwnerId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
        
        private void ConfigureDocumentVersion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.MimeType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ChangeComment).HasMaxLength(1000);
                
                entity.HasOne(e => e.Document)
                    .WithMany(d => d.Versions)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.VersionNumber);
            });
        }
        
        private void ConfigureTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EdmTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
                
                entity.HasIndex(e => e.Name).IsUnique();
            });
        }
        
        private void ConfigureDocumentTag(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentTag>(entity =>
            {
                entity.HasKey(e => new { e.DocumentId, e.TagId });
                
                entity.HasOne(e => e.Document)
                    .WithMany(d => d.DocumentTags)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.DocumentTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
        
        private void ConfigurePermission(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PermissionType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.GrantedBy).IsRequired().HasMaxLength(100);
                
                entity.HasOne(e => e.Folder)
                    .WithMany(f => f.Permissions)
                    .HasForeignKey(e => e.FolderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Document)
                    .WithMany(d => d.Permissions)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Permissions)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.FolderId);
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.PermissionType);
            });
        }
        
        private void ConfigureComment(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
                
                entity.HasOne(e => e.Document)
                    .WithMany(d => d.Comments)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(e => e.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.UserId);
            });
        }
        
        private void ConfigureWorkflow(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workflow>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.WorkflowType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CompletedBy).HasMaxLength(100);
                
                entity.HasOne(e => e.Document)
                    .WithMany(d => d.Workflows)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.Status);
            });
        }
        
        private void ConfigureWorkflowStep(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkflowStep>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StepName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Comment).HasMaxLength(1000);
                entity.Property(e => e.CompletedBy).HasMaxLength(100);
                
                entity.HasOne(e => e.Workflow)
                    .WithMany(w => w.Steps)
                    .HasForeignKey(e => e.WorkflowId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.AssignedToUser)
                    .WithMany(u => u.AssignedWorkflowSteps)
                    .HasForeignKey(e => e.AssignedToUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => e.WorkflowId);
                entity.HasIndex(e => e.AssignedToUserId);
            });
        }
        
        private void ConfigureAuditLog(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Details).HasColumnType("nvarchar(max)");
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.EntityId);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
        
        private void ConfigureNotification(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RelatedEntityType).HasMaxLength(100);
                
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
        
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Create admin user
            var adminId = Guid.NewGuid();
            modelBuilder.Entity<EdmUser>().HasData(new EdmUser
            {
                Id = adminId,
                Username = "admin",
                Email = "admin@edm.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Administrator",
                Role = "SystemAdmin",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
            
            // Create root folder
            var rootFolderId = Guid.NewGuid();
            modelBuilder.Entity<Folder>().HasData(new Folder
            {
                Id = rootFolderId,
                Name = "Root",
                Description = "Root folder for all documents",
                Path = "/",
                Level = 0,
                IsSystemFolder = true,
                OwnerId = adminId,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
            
            // Seed default tags
            modelBuilder.Entity<EdmTag>().HasData(
                new EdmTag
                {
                    Id = Guid.NewGuid(),
                    Name = "Important",
                    Description = "Important documents",
                    Color = "#FF0000",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new EdmTag
                {
                    Id = Guid.NewGuid(),
                    Name = "Draft",
                    Description = "Draft documents",
                    Color = "#FFA500",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                },
                new EdmTag
                {
                    Id = Guid.NewGuid(),
                    Name = "Final",
                    Description = "Final version documents",
                    Color = "#008000",
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System"
                }
            );
        }
        
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }
        
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (
                    e.State == EntityState.Added || e.State == EntityState.Modified));
            
            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                    if (string.IsNullOrEmpty(entity.CreatedBy))
                    {
                        entity.CreatedBy = "System";
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.ModifiedAt = DateTime.UtcNow;
                    // Keep CreatedAt and CreatedBy unchanged
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                }
            }
        }
    }
}
