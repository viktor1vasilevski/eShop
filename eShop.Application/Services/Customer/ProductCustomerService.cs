using eShop.Application.DTOs.Customer.Category;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Helpers.Admin;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses;
using eShop.Application.Responses.Customer.Product;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eShop.Application.Services.Customer;

public class ProductCustomerService(IUnitOfWork _uow, ILogger<ProductCustomerService> _logger) : IProductCustomerService
{
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();
    private readonly IRepositoryBase<Order> _orderRepository = _uow.GetRepository<Order>();


    private List<Guid> GetAllCategoryIds(Guid categoryId)
    {
        var allCategories = _categoryRepository
            .GetAsQueryable()
            .Select(c => new CategoryNode
            {
                Id = c.Id,
                ParentCategoryId = c.ParentCategoryId
            })
            .ToList();

        var ids = new List<Guid>();
        CollectCategoryIds(categoryId, allCategories, ids);
        return ids;
    }

    private void CollectCategoryIds(Guid categoryId, List<CategoryNode> allCategories, List<Guid> ids)
    {
        ids.Add(categoryId);

        var children = allCategories
            .Where(c => c.ParentCategoryId == categoryId)
            .ToList();

        foreach (var child in children)
        {
            CollectCategoryIds(child.Id, allCategories, ids);
        }
    }


    public ApiResponse<List<ProductCustomerDto>> GetProducts(ProductCustomerRequest request)
    {
        var categoryIds = GetAllCategoryIds(request.CategoryId);

        var query = _productRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(true, x => !x.IsDeleted && categoryIds.Contains(x.CategoryId)),
            include: x => x.Include(x => x.Category))
            .AsNoTracking();

        var totalCount = query.Count();

        var sortedQuery = query;

        if (request.Skip.HasValue)
            sortedQuery = sortedQuery.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            sortedQuery = sortedQuery.Take(request.Take.Value);

        var productsDTO = sortedQuery.Select(x => new ProductCustomerDto
        {
            Id = x.Id,
            Name = x.Name,
            Price = x.UnitPrice,
            Image = ImageDataUriBuilder.FromImage(x.Image),
            Category = x.Category.Name,
        }).ToList();

        return new ApiResponse<List<ProductCustomerDto>>()
        {
            Data = productsDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<ProductDetailsCustomerDto>> GetProductById(Guid id, Guid? userId = null)
    {
        var product = await _productRepository.GetAsQueryable(
                filter: x => x.Id == id && !x.IsDeleted,
                include: q => q.Include(x => x.Category))
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return new ApiResponse<ProductDetailsCustomerDto>
            {
                Status = ResponseStatus.NotFound,
                Message = "Product not found"
            };
        }

        bool canComment = false;

        if (userId.HasValue)
        {
            canComment = _orderRepository.GetAsQueryable(
                    filter: o => o.UserId == userId.Value &&
                                 o.OrderItems.Any(oi => oi.ProductId == id))
                .Any();
        }

        var productDto = new ProductDetailsCustomerDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.UnitPrice,
            UnitQuantity = product.UnitQuantity,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            Category = product.Category?.Name,
            CanComment = canComment
        };

        return new ApiResponse<ProductDetailsCustomerDto>
        {
            Data = productDto,
            Status = ResponseStatus.Success
        };
    }


}
