using eShop.Application.DTOs.Admin.Category;
using eShop.Application.DTOs.Product;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;

namespace eShop.Application.Services.Admin;

public class ProductAdminService(IUnitOfWork _uow, ILogger<ProductAdminService> _logger) : IProductAdminService
{
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();

    public async Task<ApiResponse<ProductDetailsDTO>> CreateProduct(CreateUpdateProductRequest request)
    {
        try
        {
            var trimmedName = request.Name.Trim();
            var normalized = trimmedName.ToLower();

            var categoryExists = await _categoryRepository.ExistsAsync(c =>
                c.Id == request.CategoryId && !c.IsDeleted);

            if (!categoryExists)
            {
                return new ApiResponse<ProductDetailsDTO>
                {
                    Status = ResponseStatus.NotFound,
                    Message = CategoryConstants.CategoryDoesNotExist
                };
            }

            var hasChildren = await _categoryRepository.ExistsAsync(c =>
                c.ParentCategoryId == request.CategoryId && !c.IsDeleted);

            if (hasChildren)
            {
                return new ApiResponse<ProductDetailsDTO>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = "Products are allowed only on leaf categories"
                };
            }

            var nameTaken = await _productRepository.ExistsAsync(x =>
                x.CategoryId == request.CategoryId &&
                !x.IsDeleted &&
                x.Name.ToLower() == normalized);

            if (nameTaken)
            {
                return new ApiResponse<ProductDetailsDTO>
                {
                    Status = ResponseStatus.Conflict,
                    Message = "Product exist"
                };
            }

            Image? image = null;
            if (!string.IsNullOrWhiteSpace(request.Image))
            {
                var (bytes, type) = ImageParsing.FromBase64(request.Image);
                image = Image.FromBytes(bytes, type);
            }

            var productData = new ProductData(
                name: trimmedName,
                description: request.Description?.Trim(),
                unitPrice: request.Price,
                unitQuantity: request.Quantity,
                categoryId: request.CategoryId,
                image: image
            );

            var product = Product.Create(productData);
            _productRepository.Insert(product);
            await _uow.SaveChangesAsync();

            return new ApiResponse<ProductDetailsDTO>
            {
                Status = ResponseStatus.Created,
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_CREATED,
                Data = new ProductDetailsDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    UnitPrice = product.UnitPrice,
                    UnitQuantity = product.UnitQuantity,
                    Created = product.Created
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductDetailsDTO>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating product");
            throw;
        }
    }

    public ApiResponse<ProductDetailsDTO> DeleteProduct(Guid id)
    {
        var product = _productRepository.Get(x => x.Id == id && !x.IsDeleted)?.FirstOrDefault();
        if (product is null)
            return new ApiResponse<ProductDetailsDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = ProductConstants.ProductDoesNotExist
            };

        product.SoftDelete();
        _uow.SaveChanges();

        return new ApiResponse<ProductDetailsDTO>
        {
            Message = ProductConstants.PRODUCT_SUCCESSFULLY_DELETED,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id)
    {
        var product = await _productRepository
            .GetAsQueryable(x => !x.IsDeleted && x.Id == id)
            .Include(x => x.Category)
                .ThenInclude(c => c.ParentCategory)
            .FirstOrDefaultAsync();

        if (product is null)
        {
            return new ApiResponse<ProductDetailsAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = ProductConstants.ProductDoesNotExist
            };
        }

        // Build category hierarchy
        var categories = new List<CategoryRefDto>();
        var current = product.Category;
        while (current != null)
        {
            categories.Insert(0, new CategoryRefDto
            {
                Id = current.Id,
                Name = current.Name
            });

            current = current.ParentCategory;
        }

        var productDto = new ProductDetailsAdminDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            UnitQuantity = product.UnitQuantity,
            LastModified = product.LastModified,
            Created = product.Created,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            Categories = categories
        };

        return new ApiResponse<ProductDetailsAdminDto>
        {
            Status = ResponseStatus.Success,
            Data = productDto
        };
    }

    public ApiResponse<List<ProductAdminDto>> GetProducts(ProductAdminRequest request)
    {
        var allCategories = _categoryRepository.GetAsQueryable(x => x.IsDeleted);

        List<Guid> categoryIds = new();
        if (request.CategoryId is not null && request.CategoryId != Guid.Empty)
        {
            categoryIds = GetAllDescendantCategoryIds(allCategories, request.CategoryId ?? Guid.Empty);
        }

        var query = _productRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(true, x => !x.IsDeleted)
                          .WhereIf(!string.IsNullOrEmpty(request.Name), x=> x.Name.ToLower().Contains(request.Name.ToLower())),
            include: x => x.Include(x => x.Category)).AsNoTracking();

        var totalCount = query.Count();

        var sortedQuery = query;
        if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderBy(x => x.Created),
                    "lastmodified" => sortedQuery.OrderBy(x => x.LastModified),
                    "unitprice" => sortedQuery.OrderBy(x => x.UnitPrice),
                    "unitquantity" => sortedQuery.OrderBy(x => x.UnitQuantity),
                    _ => sortedQuery.OrderBy(x => x.Created)
                };
            }
            else
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderByDescending(x => x.Created),
                    "lastmodified" => sortedQuery.OrderByDescending(x => x.LastModified),
                    "unitprice" => sortedQuery.OrderByDescending(x => x.UnitPrice),
                    "unitquantity" => sortedQuery.OrderByDescending(x => x.UnitQuantity),
                    _ => sortedQuery.OrderByDescending(x => x.Created)
                };
            }
        }

        if (request.Skip.HasValue)
            sortedQuery = sortedQuery.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            sortedQuery = sortedQuery.Take(request.Take.Value);

        var productsDTO = sortedQuery.Select(x => new ProductAdminDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            UnitPrice = x.UnitPrice,
            UnitQuantity = x.UnitQuantity,
            Image = ImageDataUriBuilder.FromImage(x.Image),
            Category = x.Category.Name,
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<ProductAdminDto>>()
        {
            Data = productsDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public ApiResponse<ProductDetailsDTO> UpdateProduct(Guid id, CreateUpdateProductRequest request)
    {
        var product = _productRepository.Get(x => x.Id == id && !x.IsDeleted)?.FirstOrDefault();
        if (product is null)
            return new ApiResponse<ProductDetailsDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = ProductConstants.ProductDoesNotExist
            };

        //if (!_subcategoryRepository.Exists(x => x.Id == request.SubcategoryId))
        //    return new ApiResponse<ProductDetailsDTO>
        //    {
        //        Status = ResponseStatus.NotFound,
        //        Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
        //    };


        if (product.Name.ToLower() == request.Name.ToLower() && product.Id != id)
            return new ApiResponse<ProductDetailsDTO>
            {
                Status = ResponseStatus.Conflict,
                Message = ProductConstants.PRODUCT_EXISTS
            };

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var productData = new ProductData(
                name: request.Name,
                description: request.Description,
                unitPrice: request.Price,
                unitQuantity: request.Quantity,
                categoryId: request.CategoryId,
                image: image);

            product.Update(productData);
            _productRepository.Update(product);
            _uow.SaveChanges();

            return new ApiResponse<ProductDetailsDTO>
            {
                Status = ResponseStatus.Success,
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_UPDATED,
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductDetailsDTO>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }



    #region private methods

    private List<Guid> GetAllDescendantCategoryIds(IQueryable<Category> allCategories, Guid parentId)
    {
        var result = new List<Guid> { parentId };

        var children = allCategories.Where(c => c.ParentCategoryId == parentId);
        foreach (var child in children)
        {
            result.AddRange(GetAllDescendantCategoryIds(allCategories, child.Id));
        }

        return result;
    }

    #endregion
}
