
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventManagementApi.Data;
using EventManagementApi.DTOs.Auth;
using EventManagementApi.Models.Entities;
using EventManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EventManagementApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<User> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    // REGISTER PARTICIPANT
    public async Task<(AuthResponse? Response, string? Error)>
        RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Check whether the email already exists
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email);

        if (existingUser)
        {
            return (null, "An account with this email already exists.");
        }

        // Create the user account
        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = "Participant",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Hash the password before saving
        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        // Add user account
        _context.Users.Add(user);

        // Automatically create the participant profile
        var participant = new Participant
        {
            User = user
        };

        _context.Participants.Add(participant);

        // Save both records together
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("DATABASE ERROR:");
            Console.WriteLine(ex.InnerException?.Message);

            throw;
        }

        // Generate JWT token
        return (GenerateToken(user), null);
    }

    // LOGIN
    public async Task<(AuthResponse? Response, string? Error)>
        LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

        if (user == null)
        {
            return (null, "Invalid email or password.");
        }

        // Check account status
        if (!user.IsActive)
        {
            return (null, "This account is inactive.");
        }

        // Verify password
        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return (null, "Invalid email or password.");
        }

        // Rehash password if the hashing algorithm needs updating
        if (verificationResult ==
            PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(
                user,
                request.Password);

            await _context.SaveChangesAsync();
        }

        // Generate JWT token
        return (GenerateToken(user), null);
    }

    // GENERATE JWT TOKEN
    private AuthResponse GenerateToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT signing key is missing.");

        var jwtIssuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is missing.");

        var jwtAudience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is missing.");

        var expiryMinutes =
            _configuration.GetValue<int?>("Jwt:ExpiresInMinutes") ?? 60;

        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        // JWT claims
        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            new Claim(
                ClaimTypes.Email,
                user.Email),

            new Claim(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),

            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        // Signing key
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        // Create JWT
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        // Return authentication response
        return new AuthResponse
        {
            UserId = user.UserId,
            Email = user.Email,
            Role = user.Role,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
