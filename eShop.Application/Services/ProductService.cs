using eShop.Application.DTOs.Product;
using eShop.Application.Requests.Product;

namespace eShop.Application.Services;

public class ProductService(IUnitOfWork _uow) : IProductService
{
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();
    private readonly IRepositoryBase<Order> _orderRepository = _uow.GetRepository<Order>();


    public ApiResponse<List<ProductDto>> GetProducts(ProductRequest request)
    {
        var allCategories = _categoryRepository.Get();

        List<Guid> categoryIds = new();
        if (request.CategoryId is not null && request.CategoryId != Guid.Empty)
        {
            categoryIds = GetAllDescendantCategoryIds(allCategories, request.CategoryId ?? Guid.Empty);
        }

        var query = _productRepository.GetAsQueryableWhereIf(
            filter: x => x
                .WhereIf(!string.IsNullOrEmpty(request.Description), x => x.Description.ToLower().Contains(request.Description.ToLower()))
                .WhereIf(request.CategoryId is not null && request.CategoryId != Guid.Empty, x => categoryIds.Contains(x.CategoryId))
                .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())),
            include: x => x.Include(x => x.Category)
        );

        var totalCount = query.Count();

        // Sorting
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

        var productsDTO = sortedQuery.AsNoTracking().Select(x => new ProductDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            UnitPrice = x.UnitPrice,
            UnitQuantity = x.UnitQuantity,
            Image = ImageDataUriBuilder.FromImage(x.Image),
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<ProductDto>>()
        {
            Data = productsDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }


    public ApiResponse<ProductDetailsDTO> GetProductById(Guid id, Guid? userId = null)
    {
        //var product = _productRepository.Get(
        //    filter: x => x.Id == id && !x.IsDeleted,
        //    include: x => x.Include(c => c.Comments).Include(s => s.Subcategory).ThenInclude(c => c.Category)).FirstOrDefault();

        //if (product is null)
        //    return new ApiResponse<ProductDetailsDTO>
        //    {
        //        Status = ResponseStatus.NotFound,
        //        Message = ProductConstants.ProductDoesNotExist
        //    };

        //bool canComment = false;

        //if (userId.HasValue)
        //{
        //    canComment = _orderRepository.Exists(o =>
        //        o.UserId == userId.Value &&
        //        o.OrderItems.Any(oi => oi.ProductId == id));
        //}

        //var productDto = new ProductDetailsDTO()
        //{
        //    Id = product.Id,
        //    Name = product.Name,
        //    Description = product.Description,
        //    UnitPrice = product.UnitPrice,
        //    UnitQuantity = product.UnitQuantity,
        //    SubcategoryId = product.SubcategoryId,
        //    Subcategory = product.Subcategory?.Name,
        //    Category = product.Subcategory?.Category?.Name,
        //    LastModified = product.LastModified,
        //    Created = product.Created,
        //    CanComment = canComment,
        //    Image = ImageDataUriBuilder.FromImage(product.Image),
        //    Comments = product.Comments?
        //    .OrderByDescending(x => x.Created)
        //    .Select(x => new CommentDTO
        //    {
        //        CommentText = x.CommentText,
        //        CreatedBy = x.CreatedBy,
        //        Created = x.Created,
        //        Rating = x.Rating,
        //    }).ToList()

        //};

        return new ApiResponse<ProductDetailsDTO>
        {
            Status = ResponseStatus.Success,
            //Data = productDto
        };
    }

    public ApiResponse<ProductDetailsDTO> CreateProduct(CreateUpdateProductRequest request)
    {
        //if (!_subcategoryRepository.Exists(x => x.Id == request.SubcategoryId))
        //    return new ApiResponse<ProductDetailsDTO>
        //    {
        //        Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
        //        Status = ResponseStatus.NotFound
        //    };

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

            var product = Product.Create(productData);
            _productRepository.Insert(product);
            _uow.SaveChanges();

            return new ApiResponse<ProductDetailsDTO>
            {
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_CREATED,
                Status = ResponseStatus.Created,
                Data = new ProductDetailsDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    UnitPrice = product.UnitPrice,
                    UnitQuantity = product.UnitQuantity,
                    Created = product.Created,
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

    public async Task<ApiResponse<ProductDetailsDTO>> GetProductByIdAsync(Guid id)
    {
        //var product = (await _productRepository.GetAsync(
        //    filter: x => !x.IsDeleted && x.Id == id,
        //    include: x => x.Include(s => s.Subcategory).ThenInclude(c => c.Category))).FirstOrDefault();

        //if (product is null)
        //    return new ApiResponse<ProductDetailsDTO>
        //    {
        //        Status = ResponseStatus.NotFound,
        //        Message = ProductConstants.ProductDoesNotExist
        //    };

        //var productDto = new ProductDetailsDTO()
        //{
        //    Id = product.Id,
        //    Name = product.Name,
        //    Description = product.Description,
        //    UnitPrice = product.UnitPrice,
        //    UnitQuantity = product.UnitQuantity,
        //    Subcategory = product.Subcategory?.Name,
        //    SubcategoryId = product.SubcategoryId,
        //    Category = product.Subcategory?.Category?.Name,
        //    CategoryId = product.Subcategory.Category.Id,
        //    LastModified = product.LastModified,
        //    Created = product.Created,
        //    Image = ImageDataUriBuilder.FromImage(product.Image),
        //    Comments = product.Comments?
        //    .OrderByDescending(x => x.Created)
        //    .Select(x => new CommentDTO
        //    {
        //        CommentText = x.CommentText,
        //        CreatedBy = x.CreatedBy,
        //        Created = x.Created,
        //        Rating = x.Rating,
        //    }).ToList()

        //};

        return new ApiResponse<ProductDetailsDTO>
        {
            Status = ResponseStatus.Success,
            //Data = productDto
        };
    }


    private List<Guid> GetAllDescendantCategoryIds(IEnumerable<Category> allCategories, Guid parentId)
    {
        var result = new List<Guid> { parentId };

        var children = allCategories.Where(c => c.ParentCategoryId == parentId).ToList();
        foreach (var child in children)
        {
            result.AddRange(GetAllDescendantCategoryIds(allCategories, child.Id));
        }

        return result;
    }
}
