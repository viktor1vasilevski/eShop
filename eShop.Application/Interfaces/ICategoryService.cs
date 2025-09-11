using eShop.Application.DTOs.Category;
using eShop.Application.Requests.Category;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface ICategoryService
{
    ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request);
    ApiResponse<CategoryDetailsDTO> CreateCategory(CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDTO> UpdateCategory(Guid id, CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDTO> DeleteCategory(Guid id);
    Task<ApiResponse<CategoryDetailsDTO>> GetCategoryByIdAsync(Guid id);

    Task<ApiResponse<List<SelectCategoryListItemDto>>> GetCategoriesDropdownListAsync();
    ApiResponse<List<CategoryWithSubcategoriesDTO>> GetCategoriesWithSubcategoriesForMenu();


    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();
}
