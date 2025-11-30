using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Auth;

public class RequestPasswordResetRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // The frontend URL that will handle the reset link, e.g. https://app.local
    public string? Origin { get; set; }
}
