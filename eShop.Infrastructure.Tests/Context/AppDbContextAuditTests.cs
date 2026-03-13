using eShop.Domain.Enums;
using eShop.Domain.Models;
using eShop.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace eShop.Infrastructure.Tests.Context;

public class AppDbContextAuditTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();

    public AppDbContextAuditTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options, _httpContextAccessorMock.Object);
    }

    public void Dispose() => _context.Dispose();

    // ── Created / CreatedBy ──────────────────────────────────────────────────

    [Fact]
    public async Task SaveChangesAsync_NewAuditableEntity_SetsCreated()
    {
        SetCurrentUser("testuser");
        var user = CreateUser("alice");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        Assert.NotEqual(default, user.Created);
    }

    [Fact]
    public async Task SaveChangesAsync_NewAuditableEntity_SetsCreatedByToCurrentUser()
    {
        SetCurrentUser("admin");
        var user = CreateUser("alice");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        Assert.Equal("admin", user.CreatedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_NoHttpContext_SetsCreatedByToSystem()
    {
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(value: null!);
        var user = CreateUser("alice");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        Assert.Equal("System", user.CreatedBy);
    }

    // ── LastModified / LastModifiedBy ─────────────────────────────────────────

    [Fact]
    public async Task SaveChangesAsync_ModifiedAuditableEntity_SetsLastModified()
    {
        SetCurrentUser("admin");
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var originalCreated = user.Created;
        await Task.Delay(10); // ensure time difference

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        Assert.NotNull(user.LastModified);
        Assert.True(user.LastModified >= originalCreated);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedAuditableEntity_SetsLastModifiedBy()
    {
        SetCurrentUser("admin");
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        SetCurrentUser("editor");
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        Assert.Equal("editor", user.LastModifiedBy);
    }

    [Fact]
    public async Task SaveChangesAsync_ModifiedAuditableEntity_DoesNotChangeCreated()
    {
        SetCurrentUser("admin");
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var originalCreated = user.Created;
        var originalCreatedBy = user.CreatedBy;

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        Assert.Equal(originalCreated, user.Created);
        Assert.Equal(originalCreatedBy, user.CreatedBy);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetCurrentUser(string username)
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }, "Test");
        var principal = new ClaimsPrincipal(identity);
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.User).Returns(principal);
        _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(httpContextMock.Object);
    }

    private static User CreateUser(string username) =>
        User.CreateNew(new UserData("First", "Last", username, $"{username}@test.com", "hash", "salt", Role.Customer));
}
