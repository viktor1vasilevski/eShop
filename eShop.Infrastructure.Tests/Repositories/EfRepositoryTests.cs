using eShop.Domain.Enums;
using eShop.Domain.Models;
using eShop.Infrastructure.Context;
using eShop.Infrastructure.Repositories.EntityFramework;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace eShop.Infrastructure.Tests.Repositories;

public class EfRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly EfRepository<User> _sut;

    public EfRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _context = new AppDbContext(options, httpContextAccessorMock.Object);
        _sut = new EfRepository<User>(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingEntity_ReturnsEntity()
    {
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(user.Id);

        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ── FindAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task FindAsync_WithoutPredicate_ReturnsAllEntities()
    {
        await _context.Users.AddRangeAsync(CreateUser("alice"), CreateUser("bob"), CreateUser("carol"));
        await _context.SaveChangesAsync();

        var result = await _sut.FindAsync();

        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task FindAsync_WithPredicate_ReturnsOnlyMatchingEntities()
    {
        await _context.Users.AddRangeAsync(
            CreateUser("alice", Role.Admin),
            CreateUser("bob",   Role.Customer),
            CreateUser("carol", Role.Customer));
        await _context.SaveChangesAsync();

        var result = await _sut.FindAsync(u => u.Role == Role.Customer);

        Assert.Equal(2, result.Count());
        Assert.All(result, u => Assert.Equal(Role.Customer, u.Role));
    }

    // ── FirstOrDefaultAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task FirstOrDefaultAsync_MatchingEntity_ReturnsEntity()
    {
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.FirstOrDefaultAsync(u => u.Username.Value == "alice");

        Assert.NotNull(result);
        Assert.Equal("alice", result.Username.Value);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_NoMatch_ReturnsNull()
    {
        var result = await _sut.FirstOrDefaultAsync(u => u.Username.Value == "nobody");

        Assert.Null(result);
    }

    // ── AddAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NewEntity_IsPersistedAfterSave()
    {
        var user = CreateUser("alice");

        await _sut.AddAsync(user);
        await _context.SaveChangesAsync();

        var persisted = await _context.Users.FindAsync(user.Id);
        Assert.NotNull(persisted);
        Assert.Equal("alice", persisted.Username.Value);
    }

    // ── ExistsAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ExistsAsync_MatchingPredicate_ReturnsTrue()
    {
        await _context.Users.AddAsync(CreateUser("alice"));
        await _context.SaveChangesAsync();

        var exists = await _sut.ExistsAsync(u => u.Username.Value == "alice");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_NonMatchingPredicate_ReturnsFalse()
    {
        var exists = await _sut.ExistsAsync(u => u.Username.Value == "nobody");

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithoutPredicate_ReturnsTrueWhenAnyEntityExists()
    {
        await _context.Users.AddAsync(CreateUser("alice"));
        await _context.SaveChangesAsync();

        var exists = await _sut.ExistsAsync();

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_WithoutPredicate_ReturnsFalseWhenTableIsEmpty()
    {
        var exists = await _sut.ExistsAsync();

        Assert.False(exists);
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_DetachedEntity_AttachesAndMarksModified()
    {
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Fetch detached, verify state is modified after Update()
        var detached = await _context.Users.AsNoTracking().FirstAsync(u => u.Id == user.Id);
        _sut.Update(detached);

        Assert.Equal(EntityState.Modified, _context.Entry(detached).State);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingEntity_IsRemovedAfterSave()
    {
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        _sut.Delete(user);
        await _context.SaveChangesAsync();

        var deleted = await _context.Users.FindAsync(user.Id);
        Assert.Null(deleted);
    }

    // ── QueryAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task QueryAsync_WithPagination_ReturnsCorrectPage()
    {
        await _context.Users.AddRangeAsync(
            CreateUser("a"), CreateUser("b"), CreateUser("c"),
            CreateUser("d"), CreateUser("e"));
        await _context.SaveChangesAsync();

        var (items, totalCount) = await _sut.QueryAsync<User>(skip: 2, take: 2);

        Assert.Equal(5, totalCount);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task QueryAsync_WithFilter_ReturnsTotalCountBeforePaging()
    {
        await _context.Users.AddRangeAsync(
            CreateUser("a", Role.Admin),
            CreateUser("b", Role.Customer),
            CreateUser("c", Role.Customer),
            CreateUser("d", Role.Customer));
        await _context.SaveChangesAsync();

        var (items, totalCount) = await _sut.QueryAsync<User>(
            queryBuilder: q => q.Where(u => u.Role == Role.Customer),
            take: 1);

        Assert.Equal(3, totalCount);
        Assert.Single(items);
    }

    // ── GetSingleAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetSingleAsync_MatchingEntity_ReturnsEntity()
    {
        var user = CreateUser("alice");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetSingleAsync<User>(u => u.Username.Value == "alice");

        Assert.NotNull(result);
        Assert.Equal("alice", result.Username.Value);
    }

    [Fact]
    public async Task GetSingleAsync_NoMatch_ReturnsNull()
    {
        var result = await _sut.GetSingleAsync<User>(u => u.Username.Value == "nobody");

        Assert.Null(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static User CreateUser(string username, Role role = Role.Customer) =>
        User.CreateNew(new UserData("First", "Last", username, $"{username}@test.com", "hash", "salt", role));
}
