using Xunit;
using Backend.Services;

namespace Backend.Tests;

public class ProgramTests
{
    [Fact]
    public void GetEmailServiceType_ReturnsMimeKitEmailService_WhenProviderIsMimeKit()
    {
        // Arrange
        var provider = "mimekit";

        // Act
        var result = ConfigurationUtils.GetEmailServiceType(provider);

        // Assert
        Assert.Equal(typeof(MimeKitEmailService), result);
    }

    [Fact]
    public void GetEmailServiceType_ReturnsMimeKitEmailService_WhenUseMimeKitIsTrue()
    {
        // Arrange
        var provider = "true";

        // Act
        var result = ConfigurationUtils.GetEmailServiceType(provider);

        // Assert
        Assert.Equal(typeof(MimeKitEmailService), result);
    }

    [Fact]
    public void GetEmailServiceType_ReturnsSmtpEmailService_WhenProviderIsSmtp()
    {
        // Arrange
        var provider = "smtp";

        // Act
        var result = ConfigurationUtils.GetEmailServiceType(provider);

        // Assert
        Assert.Equal(typeof(SmtpEmailService), result);
    }

    [Fact]
    public void GetEmailServiceType_ReturnsSmtpEmailService_WhenProviderIsNull()
    {
        // Arrange
        string provider = null;

        // Act
        var result = ConfigurationUtils.GetEmailServiceType(provider);

        // Assert
        Assert.Equal(typeof(SmtpEmailService), result);
    }

    [Fact]
    public void GetEmailServiceType_ReturnsSmtpEmailService_WhenProviderIsEmpty()
    {
        // Arrange
        var provider = "";

        // Act
        var result = ConfigurationUtils.GetEmailServiceType(provider);

        // Assert
        Assert.Equal(typeof(SmtpEmailService), result);
    }

    [Fact]
    public void GetEmailServiceType_IsCaseInsensitive()
    {
        // Arrange
        var provider = "MIMEKIT";

        // Act
        var result = ConfigurationUtils.GetEmailServiceType(provider);

        // Assert
        Assert.Equal(typeof(MimeKitEmailService), result);
    }
}