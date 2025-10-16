using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services.Customer;

public class ProductCustomerService(IUnitOfWork _uow, IRepository<Product> _productRepository, IRepository<Category> _categoryRepository) : IProductCustomerService
{

    public async Task<ApiResponse<List<ProductCustomerDto>>> GetProductsAsync(ProductCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var categoryIds = await GetAllCategoryIdsAsync(request.CategoryId, cancellationToken);

        var (products, totalCount) = await _productRepository.QueryAsync(
            queryBuilder: q => q.Where(x => !x.IsDeleted && categoryIds.Contains(x.CategoryId)),
            selector: x => new ProductCustomerDto
            {
                Id = x.Id,
                Name = x.Name,
                Price = x.UnitPrice,
                Image = ImageDataUriBuilder.FromImage(x.Image),
                Category = x.Category.Name
            },
            includeBuilder: q => q.Include(x => x.Category),
            skip: request.Skip,
            take: request.Take,
            cancellationToken: cancellationToken
        );

        return new ApiResponse<List<ProductCustomerDto>>
        {
            Data = products.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public class CustomNode
    {
        public Guid Id { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }

    private async Task<List<Guid>> GetAllCategoryIdsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var allCategories = (await _categoryRepository.QueryAsync(
            queryBuilder: q => q,
            selector: c => new CustomNode { Id = c.Id, ParentCategoryId = c.ParentCategoryId },
            cancellationToken: cancellationToken
        )).Items.ToList();

        var ids = new List<Guid>();
        CollectCategoryIds(categoryId, allCategories, ids);
        return ids;
    }

    private void CollectCategoryIds(Guid categoryId, List<CustomNode> allCategories, List<Guid> ids)
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
}
