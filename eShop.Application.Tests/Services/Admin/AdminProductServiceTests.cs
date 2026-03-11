using eShop.Application.Constants.Admin;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Services.Admin;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using eShop.Domain.ValueObjects;
using Moq;
using System.Linq.Expressions;

namespace eShop.Application.Tests.Services.Admin;

public class AdminProductServiceTests
{
    private readonly Mock<IEfUnitOfWork> _uowMock = new();
    private readonly Mock<IEfRepository<Category>> _categoryRepoMock = new();
    private readonly Mock<IEfRepository<Product>> _productRepoMock = new();
    private readonly Mock<IOpenAIProductDescriptionGenerator> _openAiMock = new();
    private readonly AdminProductService _sut;

    public AdminProductServiceTests()
    {
        _sut = new AdminProductService(
            _uowMock.Object,
            _categoryRepoMock.Object,
            _productRepoMock.Object,
            _openAiMock.Object);
    }

    // --- DeleteProductAsync ---

    [Fact]
    public async Task DeleteProductAsync_ProductNotFound_ReturnsNotFound()
    {
        _productRepoMock
            .Setup(r => r.GetSingleAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _sut.DeleteProductAsync(Guid.NewGuid());

        Assert.Equal(ResponseStatus.NotFound, result.Status);
        Assert.Equal(AdminProductConstants.ProductDoesNotExist, result.Message);
    }

    [Fact]
    public async Task DeleteProductAsync_ProductExists_ReturnsSuccess()
    {
        var product = CreateTestProduct();

        _productRepoMock
            .Setup(r => r.GetSingleAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _sut.DeleteProductAsync(product.Id);

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal(AdminProductConstants.ProductSuccessfullyDeleted, result.Message);
    }

    // --- CreateProductAsync ---

    [Fact]
    public async Task CreateProductAsync_CategoryNotFound_ReturnsNotFound()
    {
        _categoryRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateProductAdminRequest
        {
            Name = "Laptop",
            CategoryId = Guid.NewGuid(),
            Price = 999m,
            Quantity = 10,
            Description = "A laptop",
            Image = ValidBase64Image()
        };

        var result = await _sut.CreateProductAsync(request);

        Assert.Equal(ResponseStatus.NotFound, result.Status);
        Assert.Equal(AdminCategoryConstants.CategoryDoesNotExist, result.Message);
    }

    [Fact]
    public async Task CreateProductAsync_CategoryIsNotLeaf_ReturnsBadRequest()
    {
        // Category exists
        _categoryRepoMock
            .SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)   // category exists
            .ReturnsAsync(true);  // has children

        var request = new CreateProductAdminRequest
        {
            Name = "Laptop",
            CategoryId = Guid.NewGuid(),
            Price = 999m,
            Quantity = 10,
            Description = "A laptop",
            Image = ValidBase64Image()
        };

        var result = await _sut.CreateProductAsync(request);

        Assert.Equal(ResponseStatus.BadRequest, result.Status);
        Assert.Equal(AdminProductConstants.ProductsAllowedOnlyOnLeafCategories, result.Message);
    }

    [Fact]
    public async Task CreateProductAsync_NameAlreadyTaken_ReturnsConflict()
    {
        _categoryRepoMock
            .SetupSequence(r => r.ExistsAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)   // category exists
            .ReturnsAsync(false); // no children (leaf)

        _productRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);  // name taken

        var request = new CreateProductAdminRequest
        {
            Name = "Laptop",
            CategoryId = Guid.NewGuid(),
            Price = 999m,
            Quantity = 10,
            Description = "A laptop",
            Image = ValidBase64Image()
        };

        var result = await _sut.CreateProductAsync(request);

        Assert.Equal(ResponseStatus.Conflict, result.Status);
        Assert.Equal(AdminProductConstants.ProductExist, result.Message);
    }

    // --- GetProductByIdAsync ---

    [Fact]
    public async Task GetProductByIdAsync_ProductNotFound_ReturnsNotFound()
    {
        _productRepoMock
            .Setup(r => r.GetSingleAsync(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<Expression<Func<Product, Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _sut.GetProductByIdAsync(Guid.NewGuid());

        Assert.Equal(ResponseStatus.NotFound, result.Status);
        Assert.Equal(AdminProductConstants.ProductDoesNotExist, result.Message);
    }

    // --- Helpers ---

    private static Product CreateTestProduct()
    {
        var image = Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");
        return Product.Create("Test Product", "Description", 10m, 5, Guid.NewGuid(), image);
    }

    private static string ValidBase64Image()
    {
        // Minimal valid base64 jpeg prefix for testing
        return "data:image/jpeg;base64," + Convert.ToBase64String(new byte[] { 1, 2, 3 });
    }
}
