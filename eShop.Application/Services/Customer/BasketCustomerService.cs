using eShop.Application.Constants.Customer;
using eShop.Application.DTOs.Basket;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Basket;

namespace eShop.Application.Services.Customer;

public class BasketCustomerService(IUnitOfWork _uow, ILogger<BasketCustomerService> _logger) : IBasketCustomerService
{
    private readonly IRepositoryBase<Basket> _basketRepository = _uow.GetRepository<Basket>();
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();


    public async Task<ApiResponse<BasketDTO>> UpdateUserBasketAsync(Guid userId, UpdateBasketCustomerRequest request)
    {
        if (!await _userRepository.ExistsAsync(x => x.Id == userId))
            return new ApiResponse<BasketDTO>
            {
                Message = UserCustomerConstants.UserNotFound,
                Status = ResponseStatus.NotFound
            };

        var basket = _basketRepository.Get(
            filter: x => x.UserId == userId,
            include: x => x.Include(b => b.BasketItems)).FirstOrDefault();

        if (basket == null)
        {
            basket = Basket.CreateNew(userId);
            await _basketRepository.InsertAsync(basket);
        }

        var productIds = request.Items.Select(r => r.ProductId).Distinct().ToList();
        var products = await _productRepository.GetAsync(x => productIds.Contains(x.Id));
        var productsDict = products.ToDictionary(p => p.Id);


        foreach (var reqItem in request.Items)
        {
            if (!productsDict.TryGetValue(reqItem.ProductId, out var product))
                continue;

            basket.AddOrUpdateItem(product, reqItem.Quantity);
        }

        await _uow.SaveChangesAsync();

        return new ApiResponse<BasketDTO>
        {
            Data = null,
            Message = BasketConstants.BASKET_MERGED,
            Status = ResponseStatus.Success
        };
    }
}
