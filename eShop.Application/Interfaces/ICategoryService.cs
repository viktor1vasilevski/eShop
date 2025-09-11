using eShop.Application.DTOs.Category;
using eShop.Application.Requests.Category;

namespace eShop.Application.Interfaces;

public interface ICategoryService
{
    ApiResponse<List<CategoryDto>> GetCategories(CategoryRequest request);
    ApiResponse<CategoryDto> CreateCategory(CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDto> UpdateCategory(Guid id, CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDto> DeleteCategory(Guid id);
    Task<ApiResponse<CategoryDetailsDto>> GetCategoryByIdAsync(Guid id);

    Task<ApiResponse<List<SelectCategoryListItemDto>>> GetCategoriesDropdownListAsync();
    ApiResponse<List<CategoryWithSubcategoriesDTO>> GetCategoriesWithSubcategoriesForMenu();


    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();
}
