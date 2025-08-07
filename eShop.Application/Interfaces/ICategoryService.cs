using eShop.Application.DTOs.Category;
using eShop.Application.Requests.Category;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface ICategoryService
{
    ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request);
}
