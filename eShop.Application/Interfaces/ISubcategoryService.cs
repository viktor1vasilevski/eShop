using eShop.Application.DTOs.Subcategory;
using eShop.Application.Requests.Subcategory;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface ISubcategoryService
{
    ApiResponse<List<SubcategoryDTO>> GetSubcategories(SubcategoryRequest request);
    Task<ApiResponse<SubcategoryDetailsDTO>> GetSubcategoryByIdAsync(Guid id);
    ApiResponse<SubcategoryDetailsDTO> CreateSubcategory(CreateUpdateSubcategoryRequest request);
    ApiResponse<SubcategoryDTO> UpdateSubcategory(Guid id, CreateUpdateSubcategoryRequest request);
    ApiResponse<SubcategoryDTO> DeleteSubcategory(Guid id);
    ApiResponse<List<SelectSubcategoryListItemDTO>> GetSubcategoriesWithCategoriesDropdownList();
    Task<ApiResponse<List<SelectSubcategoryListItemDTO>>> GetSubcategoriesDropdownListAsync();
}
