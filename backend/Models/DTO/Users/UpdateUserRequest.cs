using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTO.Users;

public class UpdateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public string? Department { get; set; }

    public bool IsActive { get; set; }
}
