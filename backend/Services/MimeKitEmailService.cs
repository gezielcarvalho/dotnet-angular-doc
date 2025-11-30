using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Threading.Tasks;
using System.Text;
using MimeKit.Text;
using System.Net;

namespace Backend.Services;

public class MimeKitEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MimeKitEmailService> _logger;

    public MimeKitEmailService(IConfiguration configuration, ILogger<MimeKitEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _logger.LogInformation("MimeKitEmailService initialized - will encode parts as Base64");
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpSection = _configuration.GetSection("Smtp");
        var host = smtpSection["Host"] ?? "localhost";
        var port = int.Parse(smtpSection["Port"] ?? "25");
        var useSsl = bool.Parse(smtpSection["UseSsl"] ?? "false");
        var from = smtpSection["From"] ?? "noreply@local";

        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(from));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        var plainText = StripHtmlAndRenderPlainText(htmlBody);
        var textPart = new TextPart(TextFormat.Plain)
        {
            Text = plainText,
            ContentTransferEncoding = ContentEncoding.Base64
        };

        var htmlPart = new TextPart(TextFormat.Html)
        {
            Text = htmlBody,
            ContentTransferEncoding = ContentEncoding.Base64
        };

        var multipart = new MultipartAlternative { textPart, htmlPart };
        msg.Body = multipart;

        using var client = new SmtpClient();
        try
        {
            _logger.LogInformation("Connecting to SMTP {Host}:{Port} (ssl={UseSsl})", host, port, useSsl);
            await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);
            // No auth for MailHog/dev
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);
            _logger.LogInformation("MailKit: Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via MailKit to {To}", to);
            throw;
        }
    }

    private string StripHtmlAndRenderPlainText(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        try
        {
            var decoded = WebUtility.HtmlDecode(html);
            var sb = new StringBuilder();
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
