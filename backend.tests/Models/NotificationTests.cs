using Backend.Models.Document;
using FluentAssertions;

namespace backend.tests.Models;

public class NotificationTests
{
    [Fact]
    public void Notification_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var notification = new Notification();

        // Assert - BaseEntity properties
        notification.Id.Should().BeEmpty();
        notification.CreatedAt.Should().Be(default(DateTime));
        notification.CreatedBy.Should().BeEmpty();
        notification.ModifiedAt.Should().BeNull();
        notification.ModifiedBy.Should().BeNull();

        // Assert - Notification properties
        notification.UserId.Should().BeEmpty();
        notification.Type.Should().BeEmpty();
        notification.Title.Should().BeEmpty();
        notification.Message.Should().BeEmpty();
        notification.RelatedEntityId.Should().BeNull();
        notification.RelatedEntityType.Should().BeNull();
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Notification_ShouldSetProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var relatedEntityId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var modifiedAt = DateTime.UtcNow.AddHours(1);
        var readAt = DateTime.UtcNow.AddMinutes(30);

        // Act
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            CreatedAt = createdAt,
            CreatedBy = "system",
            ModifiedAt = modifiedAt,
            ModifiedBy = "user123",
            UserId = userId,
            Type = "DocumentShared",
            Title = "Document Shared",
            Message = "A document has been shared with you",
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = "Document",
            IsRead = true,
            ReadAt = readAt
        };

        // Assert - BaseEntity properties
        notification.Id.Should().NotBeEmpty();
        notification.CreatedAt.Should().Be(createdAt);
        notification.CreatedBy.Should().Be("system");
        notification.ModifiedAt.Should().Be(modifiedAt);
        notification.ModifiedBy.Should().Be("user123");

        // Assert - Notification properties
        notification.UserId.Should().Be(userId);
        notification.Type.Should().Be("DocumentShared");
        notification.Title.Should().Be("Document Shared");
        notification.Message.Should().Be("A document has been shared with you");
        notification.RelatedEntityId.Should().Be(relatedEntityId);
        notification.RelatedEntityType.Should().Be("Document");
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().Be(readAt);
    }

    [Fact]
    public void Notification_ShouldHandleUnreadNotification()
    {
        // Arrange & Act
        var notification = new Notification
        {
            UserId = Guid.NewGuid(),
            Type = "CommentAdded",
            Title = "New Comment",
            Message = "Someone commented on your document",
            IsRead = false,
            ReadAt = null
        };

        // Assert
        notification.IsRead.Should().BeFalse();
        notification.ReadAt.Should().BeNull();
    }

    [Fact]
    public void Notification_ShouldHandleReadNotification()
    {
        // Arrange
        var readAt = DateTime.UtcNow;

        // Act
        var notification = new Notification
        {
            UserId = Guid.NewGuid(),
            Type = "DocumentApproved",
            Title = "Document Approved",
            Message = "Your document has been approved",
            IsRead = true,
            ReadAt = readAt
        };

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().Be(readAt);
    }

    [Fact]
    public void Notification_ShouldHandleNullRelatedEntity()
    {
        // Arrange & Act
        var notification = new Notification
        {
            UserId = Guid.NewGuid(),
            Type = "SystemNotification",
            Title = "System Update",
            Message = "System maintenance completed",
            RelatedEntityId = null,
            RelatedEntityType = null
        };

        // Assert
        notification.RelatedEntityId.Should().BeNull();
        notification.RelatedEntityType.Should().BeNull();
    }

    [Fact]
    public void Notification_ShouldHandleWithRelatedEntity()
    {
        // Arrange
        var relatedEntityId = Guid.NewGuid();

        // Act
        var notification = new Notification
        {
            UserId = Guid.NewGuid(),
            Type = "FolderShared",
            Title = "Folder Shared",
            Message = "A folder has been shared with you",
            RelatedEntityId = relatedEntityId,
            RelatedEntityType = "Folder"
        };

        // Assert
        notification.RelatedEntityId.Should().Be(relatedEntityId);
        notification.RelatedEntityType.Should().Be("Folder");
    }
}