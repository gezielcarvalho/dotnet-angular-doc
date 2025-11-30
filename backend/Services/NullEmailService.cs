using Backend.Services.Interfaces;
using System.Threading.Tasks;

namespace Backend.Services;

public class NullEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        // No-op email sender for tests and fallback
        return Task.CompletedTask;
    }
}
