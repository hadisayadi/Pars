using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pars.Application.Auth;
using Pars.Application.Auth.DTOs;
using Pars.Domain.Entities.Auth;
using Pars.Infrastructure.Persistence;

namespace Pars.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly ParsDbContext _context;
    private readonly IJwtTokenService _jwt;
    private readonly IPasswordHasher<User> _hasher;

    public AuthService(ParsDbContext context, IJwtTokenService jwt, IPasswordHasher<User> hasher)
    {
        _context = context;
        _jwt = jwt;
        _hasher = hasher;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username, ct);

        if (user is null || !user.IsActive) return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed) return null;

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwt.GenerateAccessToken(user, roles);
        var refreshToken = _jwt.GenerateRefreshToken();

        return new LoginResponse(
            Token: token,
            RefreshToken: refreshToken,
            ExpiresAt: DateTime.UtcNow.AddMinutes(60),
            Username: user.Username,
            Roles: roles.ToArray()
        );
    }

    public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == request.Username, ct);
        if (exists) throw new InvalidOperationException("Username already exists");

        var user = new User
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        // Assign default role "User"
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User", ct);
        if (defaultRole is not null)
            user.UserRoles.Add(new UserRole { RoleId = defaultRole.Id });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        return new UserDto(user.Id, user.Username, user.FirstName, user.LastName, user.Email, user.IsActive, new[] { "User" });
    }

    public async Task<LoginResponse?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // In production, store refresh tokens in DB and validate here
        await Task.CompletedTask;
        return null; // Simplified
    }

    public async Task<bool> LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return true;
    }
}