using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pars.Application.Personals.DTOs;
using Pars.Domain.Entities;
using Pars.Infrastructure.Persistence;
using Pars.Infrastructure.Services;
using Xunit;

namespace Pars.Tests.Services;

public class PersonalServiceTests
{
    private async Task<ParsDbContext> CreateDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ParsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ParsDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    [Fact]
    public async Task CreateAsync_ShouldAddNewPersonal()
    {
        // Arrange
        await using var context = await CreateDbContextAsync();
        var service = new PersonalService(context);
        var dto = new CreatePersonalDto(
            Id: "1001",
            FirstName: "علی",
            LastName: "محمدی",
            FatherName: "حسین",
            CodeMelli: "0012345678",
            TelMob: "09121234567",
            Email: "ali@test.com",
            Company: "شرکت نفت",
            Pos: "کارشناس"
        );

        // Act
        var id = await service.CreateAsync(dto);

        // Assert
        id.Should().Be("1001");
        var created = await context.Personals.FindAsync("1001");
        created.Should().NotBeNull();
        created!.FirstName.Should().Be("علی");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnPersonal()
    {
        // Arrange
        await using var context = await CreateDbContextAsync();
        context.Personals.Add(new Personal { Id = "1002", FirstName = "رضا", LastName = "کریمی" });
        await context.SaveChangesAsync();

        var service = new PersonalService(context);

        // Act
        var result = await service.GetByIdAsync("1002");

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("رضا");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        await using var context = await CreateDbContextAsync();
        var service = new PersonalService(context);

        var result = await service.GetByIdAsync("9999");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_WithKeyword_ShouldFilterResults()
    {
        // Arrange
        await using var context = await CreateDbContextAsync();
        context.Personals.AddRange(
            new Personal { Id = "1", FirstName = "علی", LastName = "محمدی" },
            new Personal { Id = "2", FirstName = "رضا", LastName = "کریمی" },
            new Personal { Id = "3", FirstName = "علی", LastName = "رضایی" }
        );
        await context.SaveChangesAsync();

        var service = new PersonalService(context);

        // Act
        var results = await service.SearchAsync("علی");

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(p => p.FirstName == "علی");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemovePersonal()
    {
        await using var context = await CreateDbContextAsync();
        context.Personals.Add(new Personal { Id = "1003", FirstName = "حسین" });
        await context.SaveChangesAsync();

        var service = new PersonalService(context);
        await service.DeleteAsync("1003");

        var deleted = await context.Personals.FindAsync("1003");
        deleted.Should().BeNull();
    }
}