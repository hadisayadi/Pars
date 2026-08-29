namespace Pars.Application.Auth.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt, string Username, string[] Roles);
public record RegisterRequest(string Username, string Password, string? FirstName, string? LastName, string? Email);
public record UserDto(Guid Id, string Username, string? FirstName, string? LastName, string? Email, bool IsActive, string[] Roles);