using eShop.Application.Constants.Admin;
using eShop.Application.Enums;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Services.Admin;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using eShop.Domain.ValueObjects;
using Moq;
using System.Linq.Expressions;

namespace eShop.Application.Tests.Services.Admin;

public class AdminCategoryServiceTests
{
    private readonly Mock<IEfUnitOfWork> _uowMock = new();
    private readonly Mock<IEfRepository<Category>> _categoryRepoMock = new();
    private readonly Mock<IEfRepository<Product>> _productRepoMock = new();
    private readonly AdminCategoryService _sut;

    public AdminCategoryServiceTests()
    {
        _sut = new AdminCategoryService(_uowMock.Object, _categoryRepoMock.Object, _productRepoMock.Object);
    }

    // --- CreateCategoryAsync ---

    [Fact]
    public async Task CreateCategoryAsync_NameAlreadyExists_ReturnsConflict()
    {
        _categoryRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateCategoryAdminRequest
        {
            Name = "Electronics",
            Image = ValidBase64Image()
        };

        var result = await _sut.CreateCategoryAsync(request);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(AdminCategoryConstants.CategoryExist, result.Message);
    }

    // --- UpdateCategoryAsync ---

    [Fact]
    public async Task UpdateCategoryAsync_CategoryNotFound_ReturnsNotFound()
    {
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _sut.UpdateCategoryAsync(Guid.NewGuid(), new UpdateCategoryAdminRequest { Name = "Test" });

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal(AdminCategoryConstants.CategoryDoesNotExist, result.Message);
    }

    [Fact]
    public async Task UpdateCategoryAsync_CategoryCannotBeOwnParent_ReturnsBadRequest()
    {
        var id = Guid.NewGuid();
        var category = CreateTestCategory();

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new UpdateCategoryAdminRequest
        {
            Name = "Electronics",
            ParentCategoryId = id  // same as the id being updated
        };

        var result = await _sut.UpdateCategoryAsync(id, request);

        Assert.Equal(ResultStatus.BadRequest, result.Status);
        Assert.Equal(AdminCategoryConstants.CategoryCannotBeOwnParent, result.Message);
    }

    // --- DeleteCategoryAsync ---

    [Fact]
    public async Task DeleteCategoryAsync_CategoryNotFound_ReturnsNotFound()
    {
        _categoryRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.DeleteCategoryAsync(Guid.NewGuid());

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal(AdminCategoryConstants.CategoryDoesNotExist, result.Message);
    }

    [Fact]
    public async Task DeleteCategoryAsync_CategoryHasProducts_ReturnsConflict()
    {
        var categoryId = Guid.NewGuid();

        _categoryRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Return category nodes (no children)
        _categoryRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Category>, IQueryable<Category>>>(),
                It.IsAny<Expression<Func<Category, Category.CategoryNode>>>(),
                It.IsAny<Func<IQueryable<Category>, IQueryable<Category>>>(),
                It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Category.CategoryNode> { new(categoryId, null) }, 1));

        // Category has products
        _productRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<Expression<Func<Product, Guid>>>(),
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Guid> { Guid.NewGuid() }, 1));

        var result = await _sut.DeleteCategoryAsync(categoryId);

        Assert.Equal(ResultStatus.Conflict, result.Status);
    }

    // --- GetCategoryByIdAsync ---

    [Fact]
    public async Task GetCategoryByIdAsync_CategoryNotFound_ReturnsNotFound()
    {
        _categoryRepoMock
            .Setup(r => r.GetSingleAsync(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<Expression<Func<Category, eShop.Application.Responses.Admin.Category.CategoryDetailsAdminDto>>>(),
                It.IsAny<Func<IQueryable<Category>, IQueryable<Category>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((eShop.Application.Responses.Admin.Category.CategoryDetailsAdminDto?)null);

        var result = await _sut.GetCategoryByIdAsync(Guid.NewGuid());

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal(AdminCategoryConstants.CategoryDoesNotExist, result.Message);
    }

    // --- Helpers ---

    private static Category CreateTestCategory()
    {
        var image = Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");
        return Category.Create("Electronics", image, null);
    }

    private static string ValidBase64Image() =>
        "data:image/jpeg;base64," + Convert.ToBase64String(new byte[] { 1, 2, 3 });
}
