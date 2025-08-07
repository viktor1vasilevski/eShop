using eShop.Application.Constants;
using eShop.Application.DTOs.Product;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Product;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services;

public class ProductService(IUnitOfWork _uow) : IProductService
{
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();
    private readonly IRepositoryBase<Subcategory> _subcategoryRepository = _uow.GetRepository<Subcategory>();

    public ApiResponse<ProductDetailsDTO> CreateProduct(CreateUpdateProductRequest request)
    {
        if (!_subcategoryRepository.Exists(x => x.Id == request.SubcategoryId))
            return new ApiResponse<ProductDetailsDTO>
            {
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
                NotificationType = NotificationType.NotFound
            };

        try
        {
            var productData = new ProductData(
                name: request.Name,
                description: request.Description,
                unitPrice: request.Price,
                unitQuantity: request.Quantity,
                subcategoryId: request.SubcategoryId,
                base64Image: request.Image);

            var product = Product.CreateNew(productData);
            _productRepository.Insert(product);
            _uow.SaveChanges();

            return new ApiResponse<ProductDetailsDTO>
            {
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_CREATED,
                NotificationType = NotificationType.Created,
                Data = new ProductDetailsDTO
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    UnitPrice = product.UnitPrice,
                    UnitQuantity = product.UnitQuantity,
                    SubcategoryId = product.SubcategoryId,
                    Created = product.Created,
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }


    }

    public ApiResponse<ProductDetailsDTO> DeleteProduct(Guid id)
    {
        var product = _productRepository.GetById(id);
        if (product is null)
            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = ProductConstants.PRODUCT_DOESNT_EXIST
            };

        _productRepository.Delete(product);
        _uow.SaveChanges();

        return new ApiResponse<ProductDetailsDTO>
        {
            Message = ProductConstants.PRODUCT_SUCCESSFULLY_DELETED,
            NotificationType = NotificationType.Success
        };
    }

    public ApiResponse<ProductDetailsDTO> GetProductById(Guid id)
    {
        var product = _productRepository.Get(
                filter: x => x.Id == id,
                include: x => x.Include(x => x.Subcategory).ThenInclude(x => x.Category)).FirstOrDefault();

        if (product is null)
            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = ProductConstants.PRODUCT_DOESNT_EXIST
            };

        var productDto = new ProductDetailsDTO()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            UnitQuantity = product.UnitQuantity,
            SubcategoryId = product.SubcategoryId,
            Subcategory = product.Subcategory?.Name,
            Category = product.Subcategory?.Category?.Name,
            LastModified = product.LastModified,
            Created = product.Created,
            Image = product.Image != null ? $"data:{product.ImageType};base64,{Convert.ToBase64String(product.Image)}" : null,
        };


        return new ApiResponse<ProductDetailsDTO>
        {
            NotificationType = NotificationType.Success,
            Data = productDto
        };
    }

    public ApiResponse<List<ProductDetailsDTO>> GetProducts(ProductRequest request)
    {
        var products = _productRepository.GetAsQueryableWhereIf(x =>
                x.WhereIf(!String.IsNullOrEmpty(request.CategoryId.ToString()), x => x.Subcategory.Category.Id == request.CategoryId)
                 .WhereIf(!String.IsNullOrEmpty(request.SubcategoryId.ToString()), x => x.Subcategory.Id == request.SubcategoryId)
                 .WhereIf(!String.IsNullOrEmpty(request.Description), x => x.Description.ToLower().Contains(request.Description.ToLower()))
                 .WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())),
                null,
                x => x.Include(x => x.Subcategory).ThenInclude(sc => sc.Category));

        if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                products = request.SortBy.ToLower() switch
                {
                    "created" => products.OrderBy(x => x.Created),
                    "lastmodified" => products.OrderBy(x => x.LastModified),
                    "unitprice" => products.OrderBy(x => x.UnitPrice),
                    "unitquantity" => products.OrderBy(x => x.UnitQuantity),
                    _ => products.OrderBy(x => x.Created)
                };
            }
            else if (request.SortDirection.ToLower() == "desc")
            {
                products = request.SortBy.ToLower() switch
                {
                    "created" => products.OrderByDescending(x => x.Created),
                    "lastmodified" => products.OrderByDescending(x => x.LastModified),
                    "unitprice" => products.OrderByDescending(x => x.UnitPrice),
                    "unitquantity" => products.OrderByDescending(x => x.UnitQuantity),
                    _ => products.OrderByDescending(x => x.Created)
                };
            }
        }

        var totalCount = products.Count();

        if (request.Skip.HasValue)
            products = products.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            products = products.Take(request.Take.Value);

        var productsDTO = products.Select(x => new ProductDetailsDTO
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            UnitPrice = x.UnitPrice,
            UnitQuantity = x.UnitQuantity,
            Image = x.Image != null ? $"data:{x.ImageType};base64,{Convert.ToBase64String(x.Image)}" : null,
            Category = x.Subcategory.Category.Name,
            Subcategory = x.Subcategory.Name,
            SubcategoryId = x.SubcategoryId,
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<ProductDetailsDTO>>()
        {
            Data = productsDTO,
            TotalCount = totalCount,
            NotificationType = NotificationType.Success
        };
    }

    public ApiResponse<ProductDetailsDTO> UpdateProduct(Guid id, CreateUpdateProductRequest request)
    {
        if (!_subcategoryRepository.Exists(x => x.Id == request.SubcategoryId))
            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
            };

        var product = _productRepository.GetById(id);

        if (product is null)
            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = ProductConstants.PRODUCT_DOESNT_EXIST
            };

        if(product.Name.ToLower() == request.Name.ToLower() && product.Id != id)
            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.Conflict,
                Message = ProductConstants.PRODUCT_EXISTS
            };

        try
        {
            var productData = new ProductData(
                name: request.Name,
                description: request.Description,
                unitPrice: request.Price,
                unitQuantity: request.Quantity,
                subcategoryId: request.SubcategoryId,
                base64Image: request.Image);

            product.Update(productData);
            _productRepository.Update(product);
            _uow.SaveChanges();

            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.Success,
                Message = ProductConstants.PRODUCT_SUCCESSFULLY_UPDATED,
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductDetailsDTO>
            {
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }
    }
}
