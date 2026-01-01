using Backend.Models.Document;
using FluentAssertions;

namespace backend.tests.Models;

public class CommentTests
{
    [Fact]
    public void Comment_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var comment = new Comment();

        // Assert - BaseEntity properties
        comment.Id.Should().BeEmpty();
        comment.CreatedAt.Should().Be(default(DateTime));
        comment.CreatedBy.Should().BeEmpty();
        comment.ModifiedAt.Should().BeNull();
        comment.ModifiedBy.Should().BeNull();

        // Assert - SoftDeletableEntity properties
        comment.IsDeleted.Should().BeFalse();
        comment.DeletedAt.Should().BeNull();
        comment.DeletedBy.Should().BeNull();

        // Assert - Comment properties
        comment.DocumentId.Should().BeEmpty();
        comment.ParentCommentId.Should().BeNull();
        comment.UserId.Should().BeEmpty();
        comment.Text.Should().BeEmpty();
        comment.IsResolved.Should().BeFalse();

        // Assert - Navigation properties
        comment.Document.Should().BeNull();
        comment.User.Should().BeNull();
        comment.ParentComment.Should().BeNull();
        comment.Replies.Should().NotBeNull();
        comment.Replies.Should().BeEmpty();
    }

    [Fact]
    public void Comment_ShouldSetProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var modifiedAt = DateTime.UtcNow.AddHours(1);
        var deletedAt = DateTime.UtcNow.AddHours(2);

        // Act
        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            ParentCommentId = parentCommentId,
            UserId = userId,
            Text = "This is a test comment",
            IsResolved = true,
            CreatedAt = createdAt,
            CreatedBy = "testuser",
            ModifiedAt = modifiedAt,
            ModifiedBy = "testuser2",
            IsDeleted = true,
            DeletedAt = deletedAt,
            DeletedBy = "admin"
        };

        // Assert - BaseEntity properties
        comment.Id.Should().NotBeEmpty();
        comment.CreatedAt.Should().Be(createdAt);
        comment.CreatedBy.Should().Be("testuser");
        comment.ModifiedAt.Should().Be(modifiedAt);
        comment.ModifiedBy.Should().Be("testuser2");

        // Assert - SoftDeletableEntity properties
        comment.IsDeleted.Should().BeTrue();
        comment.DeletedAt.Should().Be(deletedAt);
        comment.DeletedBy.Should().Be("admin");

        // Assert - Comment properties
        comment.DocumentId.Should().Be(documentId);
        comment.ParentCommentId.Should().Be(parentCommentId);
        comment.UserId.Should().Be(userId);
        comment.Text.Should().Be("This is a test comment");
        comment.IsResolved.Should().BeTrue();
    }

    [Fact]
    public void Comment_ShouldHandleNullParentCommentId()
    {
        // Arrange & Act
        var comment = new Comment
        {
            DocumentId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Text = "Comment without parent",
            ParentCommentId = null
        };

        // Assert
        comment.ParentCommentId.Should().BeNull();
        comment.ParentComment.Should().BeNull();
    }

    [Fact]
    public void Comment_ShouldInitializeRepliesCollection()
    {
        // Arrange & Act
        var comment = new Comment();

        // Assert
        comment.Replies.Should().NotBeNull();
        comment.Replies.Should().BeEmpty();
    }

    [Fact]
    public void Comment_ShouldAllowAddingReplies()
    {
        // Arrange
        var comment = new Comment();
        var reply = new Comment
        {
            DocumentId = comment.DocumentId,
            UserId = Guid.NewGuid(),
            Text = "This is a reply",
            ParentCommentId = comment.Id
        };

        // Act
        comment.Replies.Add(reply);

        // Assert
        comment.Replies.Should().HaveCount(1);
        comment.Replies.First().Should().Be(reply);
        comment.Replies.First().Text.Should().Be("This is a reply");
    }

    [Fact]
    public void Comment_ShouldHandleSoftDelete()
    {
        // Arrange
        var comment = new Comment
        {
            Text = "Original comment"
        };

        // Act - Soft delete
        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedBy = "moderator";

        // Assert
        comment.IsDeleted.Should().BeTrue();
        comment.DeletedAt.Should().NotBeNull();
        comment.DeletedBy.Should().Be("moderator");
        comment.Text.Should().Be("Original comment"); // Original data preserved
    }
}