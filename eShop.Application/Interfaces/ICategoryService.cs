using eShop.Application.DTOs.Category;
using eShop.Application.Requests.Category;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface ICategoryService
{
    ApiResponse<List<CategoryDetailsDTO>> GetCategories(CategoryRequest request);
    ApiResponse<CategoryDetailsDTO> CreateCategory(CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDTO> UpdateCategory(Guid id, CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDTO> DeleteCategory(Guid id);
    ApiResponse<CategoryDTO> GetCategoryById(Guid id);

    ApiResponse<List<SelectCategoryListItemDTO>> GetCategoriesDropdownList();
    ApiResponse<List<CategoryWithSubcategoriesDTO>> GetCategoriesWithSubcategoriesForMenu();
}
