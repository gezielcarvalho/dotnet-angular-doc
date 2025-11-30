using Backend.Models.DTO.Auth;
using Backend.Models.Document;

namespace Backend.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginResponse?> RegisterAsync(RegisterRequest request);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task UpdateLastLoginAsync(Guid userId);
    string GenerateJwtToken(User user);
    Task<bool> RequestPasswordResetAsync(string email, string origin);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
}
