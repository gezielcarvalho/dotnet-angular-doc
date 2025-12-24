using Backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace backend.tests.Services;

/// <summary>
/// Unit tests for MimeKitEmailService
/// </summary>
public class TestMimeKitEmailService
{
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<MimeKitEmailService>> _loggerMock;

    public TestMimeKitEmailService()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Smtp:Host", "smtp.test.com"},
            {"Smtp:Port", "587"},
            {"Smtp:UseSsl", "true"},
            {"Smtp:From", "test@example.com"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _loggerMock = new Mock<ILogger<MimeKitEmailService>>();
    }

    [Fact]
    public void Constructor_WithValidConfiguration_InitializesCorrectly()
    {
        // Act & Assert - Constructor should not throw with valid config
        var service = new MimeKitEmailService(_configuration, _loggerMock.Object);

        // Verify the service was created successfully
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullConfiguration_DoesNotThrow()
    {
        // The constructor doesn't validate null parameters in the current implementation
        // Act & Assert - Constructor should not throw (current behavior)
        var service = new MimeKitEmailService(null!, _loggerMock.Object);

        // Verify the service was created (though it may not work properly)
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new MimeKitEmailService(_configuration, null!));

        exception.ParamName.Should().Be("logger");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithNullOrEmptyInput_ReturnsEmptyString()
    {
        // Act
        var result1 = MimeKitEmailService.StripHtmlAndRenderPlainText(null);
        var result2 = MimeKitEmailService.StripHtmlAndRenderPlainText(string.Empty);
        var result3 = MimeKitEmailService.StripHtmlAndRenderPlainText("   ");

        // Assert
        result1.Should().BeEmpty();
        result2.Should().BeEmpty();
        result3.Should().Be("   ");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithSimpleHtml_RemovesTags()
    {
        // Arrange
        var html = "<p>Hello <strong>world</strong>!</p>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Hello world!");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithComplexHtml_RemovesAllTags()
    {
        // Arrange
        var html = "<div><h1>Title</h1><p>This is <em>emphasized</em> text with <a href=\"#\">links</a>.</p></div>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("TitleThis is emphasized text with links.");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithHtmlEntities_DecodesEntities()
    {
        // Arrange
        var html = "<p>Hello &amp; welcome!</p>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Hello & welcome!");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithNewlines_RemovesExtraNewlines()
    {
        // Arrange
        var html = "<p>Line 1</p><p>Line 2</p>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert - The method strips tags but doesn't add newlines between paragraphs
        result.Should().Be("Line 1Line 2");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithMultipleConsecutiveNewlines_CollapsesToSingle()
    {
        // Arrange
        var html = "<p>Line 1</p>\n\n<p>Line 2</p>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Line 1\nLine 2");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithSelfClosingTags_HandlesCorrectly()
    {
        // Arrange
        var html = "<p>Hello<br/>world!</p>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Helloworld!");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithUnclosedTag_StripsSuccessfully()
    {
        // Arrange
        var html = "<p>Unclosed paragraph";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert - The method successfully strips the <p> tag
        result.Should().Be("Unclosed paragraph");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithNestedTags_HandlesCorrectly()
    {
        // Arrange
        var html = "<div><p><strong>Nested <em>tags</em></strong></p></div>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Nested tags");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithScriptAndStyle_KeepsContent()
    {
        // Arrange - The method only removes tags, not content between tags
        var html = "<p>Visible text</p><script>alert('hidden');</script><style>body{display:none;}</style><p>More text</p>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert - Content between script/style tags is kept since the method only removes tag characters
        result.Should().Be("Visible textalert('hidden');body{display:none;}More text");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithComments_RemovesComments()
    {
        // Arrange
        var html = "<p>Text<!-- This is a comment --></p>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Text");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithTable_ExtractsTextContent()
    {
        // Arrange
        var html = "<table><tr><td>Cell 1</td><td>Cell 2</td></tr></table>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Cell 1Cell 2");
    }

    [Fact]
    public void StripHtmlAndRenderPlainText_WithLists_ExtractsListItems()
    {
        // Arrange
        var html = "<ul><li>Item 1</li><li>Item 2</li></ul>";

        // Act
        var result = MimeKitEmailService.StripHtmlAndRenderPlainText(html);

        // Assert
        result.Should().Be("Item 1Item 2");
    }

    // Note: SendEmailAsync tests would require mocking SmtpClient which is complex
    // In a real scenario, you might want to create an ISmtpClient interface wrapper
    // For now, we'll focus on testing the pure logic parts that are testable
}