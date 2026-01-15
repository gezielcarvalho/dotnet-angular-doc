using Backend.Models.Document;
using FluentAssertions;

namespace backend.tests.Models;

public class DocumentTagTests
{
    [Fact]
    public void DocumentTag_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var documentTag = new DocumentTag();

        // Assert
        documentTag.DocumentId.Should().BeEmpty();
        documentTag.TagId.Should().BeEmpty();
        documentTag.Document.Should().BeNull();
        documentTag.Tag.Should().BeNull();
    }

    [Fact]
    public void DocumentTag_ShouldSetProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        // Act
        var documentTag = new DocumentTag
        {
            DocumentId = documentId,
            TagId = tagId
        };

        // Assert
        documentTag.DocumentId.Should().Be(documentId);
        documentTag.TagId.Should().Be(tagId);
    }
}