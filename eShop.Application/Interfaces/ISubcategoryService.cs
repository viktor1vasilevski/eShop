using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Subcategory;
using eShop.Application.Requests.Subcategory;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface ISubcategoryService
{
    ApiResponse<List<SubcategoryDetailsDTO>> GetSubcategories(SubcategoryRequest request);
    ApiResponse<SubcategoryDetailsDTO> CreateSubcategory(CreateUpdateSubcategoryRequest request);
    ApiResponse<SubcategoryDTO> UpdateSubcategory(Guid id, CreateUpdateSubcategoryRequest request);
    ApiResponse<SubcategoryDetailsDTO> GetSubcategoryById(Guid id);
    ApiResponse<SubcategoryDTO> DeleteSubcategory(Guid id);
    ApiResponse<List<SelectSubcategoryListItemDTO>> GetSubcategoriesWithCategoriesDropdownList();
    ApiResponse<List<SelectSubcategoryListItemDTO>> GetSubcategoriesDropdownList();
}
