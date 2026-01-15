namespace Backend;

/// <summary>
/// Configuration utilities extracted from Program.cs for testability.
/// </summary>
public static class ConfigurationUtils
{
    /// <summary>
    /// Determines the email service type based on configuration.
    /// </summary>
    public static Type GetEmailServiceType(string smtpProvider)
    {
        if (string.Equals(smtpProvider, "mimekit", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(smtpProvider, "true", StringComparison.OrdinalIgnoreCase))
        {
            return typeof(Backend.Services.MimeKitEmailService);
        }
        else
        {
            return typeof(Backend.Services.SmtpEmailService);
        }
    }
}