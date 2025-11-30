using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
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
        message.Body = htmlBody;
        message.IsBodyHtml = true;
        // Use AlternateView to make sure the HTML view is explicit and encoded as UTF-8
        var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, "text/html");
        message.AlternateViews.Clear();
        message.AlternateViews.Add(htmlView);
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
            await Task.Run(() => client.Send(message));
            _logger.LogInformation("Email sent to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }
}
