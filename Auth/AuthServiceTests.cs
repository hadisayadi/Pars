using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Pars.Application.Auth.DTOs;
using Pars.Domain.Entities.Auth;
using Pars.Infrastructure.Auth;
using Pars.Infrastructure.Persistence;
using Xunit;

namespace Pars.Tests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IJwtTokenService> _jwtMock = new();
    private readonly Mock<IPasswordHasher<User>> _hasherMock = new();

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ParsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new ParsDbContext(options);

        var user = new User { Id = Guid.NewGuid(), Username = "admin", IsActive = true };
        user.PasswordHash = "hashed";
        context.Users.Add(user);
        context.Roles.Add(new Role { Id = 1, Name = "Admin" });
        context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = 1 });
        await context.SaveChangesAsync();

        _hasherMock.Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), "hashed", "password123"))
                   .Returns(PasswordVerificationResult.Success);
        _jwtMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
                .Returns("fake-jwt-token");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("fake-refresh-token");

        var service = new AuthService(context, _jwtMock.Object, _hasherMock.Object);

        // Act
        var result = await service.LoginAsync(new LoginRequest("admin", "password123"));

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("fake-jwt-token");
        result.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldReturnNull()
    {
        var options = new DbContextOptionsBuilder<ParsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var context = new ParsDbContext(options);

        var user = new User { Id = Guid.NewGuid(), Username = "admin", IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _hasherMock.Setup(h => h.VerifyHashedPassword(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
                   .Returns(PasswordVerificationResult.Failed);

        var service = new AuthService(context, _jwtMock.Object, _hasherMock.Object);
        var result = await service.LoginAsync(new LoginRequest("admin", "wrong"));

        result.Should().BeNull();
    }
}