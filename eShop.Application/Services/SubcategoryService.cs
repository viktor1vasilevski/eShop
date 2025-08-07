using eShop.Application.Interfaces;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;

namespace eShop.Application.Services;

public class SubcategoryService(IUnitOfWork _uow) : ISubcategoryService
{
    private readonly IRepositoryBase<Subcategory> _subcategoryRepository = _uow.GetRepository<Subcategory>();
}
