using Backend.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Backend.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
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

        using var client = new SmtpClient(host, port);
        client.EnableSsl = useSsl;
        // No credentials by default for MailHog/dev

        // SmtpClient doesn't have async send in older APIs; wrap in Task.Run
        await Task.Run(() => client.Send(message));
    }
}
