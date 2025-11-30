using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpSection = _configuration.GetSection("Smtp");
        var host = smtpSection["Host"] ?? "localhost";
        var port = int.Parse(smtpSection["Port"] ?? "25");
        var useSsl = bool.Parse(smtpSection["UseSsl"] ?? "false");
        var from = smtpSection["From"] ?? "noreply@local";

        using var message = new MailMessage();
        message.From = new MailAddress(from);
        message.To.Add(new MailAddress(to));
        message.Subject = subject;
        // Rely on AlternateViews rather than message.Body to control content type & encoding
        // Use both plain-text and HTML alternate views and specify Base64 transfer to avoid quoted-printable artifacts
        var plainText = StripHtmlAndRenderPlainText(htmlBody);
        var plainView = AlternateView.CreateAlternateViewFromString(plainText, Encoding.UTF8, MediaTypeNames.Text.Plain);
        plainView.ContentType.CharSet = Encoding.UTF8.WebName;
        plainView.TransferEncoding = TransferEncoding.Base64;

        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);
        htmlView.ContentType.CharSet = Encoding.UTF8.WebName;
        htmlView.TransferEncoding = TransferEncoding.Base64;

        message.AlternateViews.Clear();
        message.AlternateViews.Add(plainView);
        message.AlternateViews.Add(htmlView);

        // Ensure encoding headers are set for the message
        message.SubjectEncoding = Encoding.UTF8;
        message.HeadersEncoding = Encoding.UTF8;
        // Ensure proper encoding in outgoing messages to avoid line-wrapping/quoted-printable artifacts
        message.BodyEncoding = Encoding.UTF8;
        message.SubjectEncoding = Encoding.UTF8;
        message.HeadersEncoding = Encoding.UTF8;

        using var client = new SmtpClient(host, port);
        client.EnableSsl = useSsl;
        _logger.LogInformation("Sending email to {To}, subject {Subject} via {Host}:{Port} (Ssl={UseSsl})", to, subject, host, port, useSsl);
        // No credentials by default for MailHog/dev

        // SmtpClient doesn't have async send in older APIs; wrap in Task.Run
        try
        {
            // Log alternate view encodings for debugging
            foreach (var av in message.AlternateViews)
            {
                _logger.LogInformation("AlternateView: MediaType={MediaType}, Charset={CharSet}, TransferEncoding={TransferEncoding}", av.ContentType.MediaType, av.ContentType.CharSet, av.TransferEncoding);
            }
            await Task.Run(() => client.Send(message));
            _logger.LogInformation("Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    // Rudimentary helper to strip some basic HTML and include the html link as a plaintext url
    private string StripHtmlAndRenderPlainText(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        try
        {
            // Simple conversion: remove tags and decode HTML content
            var decoded = System.Net.WebUtility.HtmlDecode(html);
            // Remove tags roughly
            var sb = new System.Text.StringBuilder();
            bool inTag = false;
            foreach (var ch in decoded)
            {
                if (ch == '<') { inTag = true; continue; }
                if (ch == '>') { inTag = false; continue; }
                if (!inTag) sb.Append(ch);
            }
            return sb.ToString().Replace("\r\n", "\n").Replace("\n\n", "\n");
        }
        catch
        {
            return html;
        }
    }
}
