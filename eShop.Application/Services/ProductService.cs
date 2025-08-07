using eShop.Application.Interfaces;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;

namespace eShop.Application.Services;

public class ProductService(IUnitOfWork _uow) : IProductService
{
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();
}
