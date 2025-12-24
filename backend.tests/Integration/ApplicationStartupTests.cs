using FluentAssertions;
using Backend;

namespace backend.tests.Integration;

public class ProgramTests
{
    [Fact]
    public void GetEmailServiceType_WithMimeKitProvider_ReturnsMimeKitService()
    {
        // Act
        var result = ConfigurationUtils.GetEmailServiceType("mimekit");

        // Assert
        result.Should().Be(typeof(Backend.Services.MimeKitEmailService));
    }

    [Fact]
    public void GetEmailServiceType_WithTrueProvider_ReturnsMimeKitService()
    {
        // Act
        var result = ConfigurationUtils.GetEmailServiceType("true");

        // Assert
        result.Should().Be(typeof(Backend.Services.MimeKitEmailService));
    }

    [Fact]
    public void GetEmailServiceType_WithSmtpProvider_ReturnsSmtpService()
    {
        // Act
        var result = ConfigurationUtils.GetEmailServiceType("smtp");

        // Assert
        result.Should().Be(typeof(Backend.Services.SmtpEmailService));
    }

    [Fact]
    public void GetEmailServiceType_WithNullOrEmptyProvider_ReturnsSmtpService()
    {
        // Act
        var result = ConfigurationUtils.GetEmailServiceType(null);

        // Assert
        result.Should().Be(typeof(Backend.Services.SmtpEmailService));
    }

    [Fact]
    public void GetEmailServiceType_WithCaseInsensitiveMimeKit_ReturnsMimeKitService()
    {
        // Act
        var result = ConfigurationUtils.GetEmailServiceType("MIMEKIT");

        // Assert
        result.Should().Be(typeof(Backend.Services.MimeKitEmailService));
    }
}