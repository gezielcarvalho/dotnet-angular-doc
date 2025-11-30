using Backend.Data;
using Backend.Models.DTO.Auth;
using Backend.Models.Document;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Services;

public class AuthService : IAuthService
{
    private readonly DocumentDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Backend.Services.Interfaces.IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(DocumentDbContext context, IConfiguration configuration, Backend.Services.Interfaces.IEmailService emailService, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted);

        if (user == null || !user.IsActive)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        await UpdateLastLoginAsync(user.Id);

        var token = GenerateJwtToken(user);
        var refreshToken = Guid.NewGuid().ToString();

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            }
        };
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if username already exists
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            return null;

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return null;

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Department = request.Department,
            Role = "User", // Default role
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.Username
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var refreshToken = Guid.NewGuid().ToString();

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        return new LoginResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            }
        };
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.IsDeleted)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = user.Username;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted);
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "EdmSystem";
        var audience = jwtSettings["Audience"] ?? "EdmClient";
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<bool> RequestPasswordResetAsync(string email, string origin)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);
        // Do not reveal whether the email exists — return true even if not found
        if (user == null)
            return true;

        var token = Guid.NewGuid().ToString("N");
        var expires = DateTime.UtcNow.AddHours(1);

        var prt = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            ExpiresAt = expires,
            Used = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResetTokens.Add(prt);
        await _context.SaveChangesAsync();

        // Sanitize origin and fallback to configured Frontend Url if not present/valid
        string baseOrigin = origin?.Trim() ?? string.Empty;
        var configuredFrontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:4200";
        if (!Uri.TryCreate(baseOrigin, UriKind.Absolute, out var baseUri))
        {
            _logger.LogWarning("Invalid origin provided for password reset: {Origin}. Falling back to configured frontend URL {FrontendUrl}", baseOrigin, configuredFrontendUrl);
            baseUri = new Uri(configuredFrontendUrl);
        }
        var tokenEncoded = System.Net.WebUtility.UrlEncode(token);
        var resetUri = new Uri(baseUri, $"/reset-password?token={tokenEncoded}");
        var resetLink = resetUri.ToString();
        var subject = "Password reset request";
        // Include an explicit plaintext link as a fallback for email clients that corrupt anchors
        var body = $"<p>We received a request to reset your password. Click the link below to reset it (link expires in 1 hour):</p>" +
               $"<p><a href=\"{resetLink}\">Reset Password</a></p>" +
               $"<p>Or paste this link into your browser: {resetLink}</p>" +
               $"<p>If you didn't request this, ignore this email.</p>";

        _logger.LogInformation("RequestPasswordReset: Email={Email}, Origin={Origin}, ResetLink={ResetLink}", user.Email, origin, resetLink);
        _logger.LogDebug("RequestPasswordReset body: {Body}", body);

        await _emailService.SendEmailAsync(user.Email, subject, body);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var prt = await _context.PasswordResetTokens
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Token == token && !p.Used && p.ExpiresAt > DateTime.UtcNow);

        if (prt == null)
            return false;

        var user = prt.User;
        if (user == null || user.IsDeleted)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.ModifiedAt = DateTime.UtcNow;
        user.ModifiedBy = user.Username;

        prt.Used = true;
        await _context.SaveChangesAsync();

        return true;
    }
}
