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
            Message = BasketCustomerConstants.BasketUpdated,
            Status = ResponseStatus.Success
        };
    }


    public async Task<ApiResponse<BasketDTO>> GetBasketByUserIdAsync(Guid userId)
    {
        var baskets = await _basketRepository.GetAsync(
            filter: b => b.UserId == userId,
            include: b => b.Include(x => x.BasketItems).ThenInclude(i => i.Product));

        var basket = baskets.FirstOrDefault();

        if (basket is null)
            return new ApiResponse<BasketDTO>
            {
                Status = ResponseStatus.Success,
                //Message = BasketCustomerConstants.BasketNotFoundForUser
                Data = new BasketDTO { Items = new List<BasketItemDTO>() }
            };

        var basketDto = new BasketDTO
        {
            Items = basket.BasketItems.Select(i => new BasketItemDTO
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                Quantity = i.Quantity,
                Price = i.Product?.UnitPrice ?? 0,
                UnitQuantity = i.Product?.UnitQuantity ?? 0,
                Image = ImageDataUriBuilder.FromImage(i.Product?.Image)
            }).ToList()
        };

        return new ApiResponse<BasketDTO>
        {
            Status = ResponseStatus.Success,
            Data = basketDto
        };
    }

    public async Task<ApiResponse<BasketDTO>> ClearBasketItemsForUserAsync(Guid userId)
    {
        var userQuery = await _userRepository.GetAsync(
            filter: x => x.Id == userId,
            include: x => x.Include(x => x.Basket).ThenInclude(b => b.BasketItems));

        var user = userQuery.FirstOrDefault();
        if (user is null)
            return new ApiResponse<BasketDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = UserCustomerConstants.UserNotFound,
            };

        user?.ClearBasket();

        await _uow.SaveChangesAsync();

        return new ApiResponse<BasketDTO>
        {
            Status = ResponseStatus.Success,
            Message = BasketCustomerConstants.BasketCleared
        };
    }

    public async Task<ApiResponse<BasketDTO>> RemoveItemAsync(Guid userId, Guid productId)
    {
        var userQuery = await _userRepository.GetAsync(
            filter: x => x.Id == userId,
            include: x => x.Include(x => x.Basket).ThenInclude(b => b.BasketItems));

        var user = userQuery.FirstOrDefault();
        if (user is null)
            return new ApiResponse<BasketDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = UserCustomerConstants.UserNotFound,
            };

        var basket = user.Basket;
        if (basket == null)
        {
            return new ApiResponse<BasketDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = BasketCustomerConstants.BasketNotFoundForUser,
            };
        }

        var itemToRemove = basket.BasketItems.FirstOrDefault(x => x.ProductId == productId);
        if (itemToRemove == null)
        {
            return new ApiResponse<BasketDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = BasketItemCustomerConstants.BasketItemNotFound,
            };
        }

        basket.RemoveItem(productId);

        await _uow.SaveChangesAsync();

        return new ApiResponse<BasketDTO>
        {
            Status = ResponseStatus.Success,
            Message = BasketItemCustomerConstants.BasketItemRemoved,
        };
    }
}
