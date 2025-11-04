using eShop.Application.Constants.Customer;
using eShop.Application.DTOs.Customer.BasketItem;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Basket;
using eShop.Application.Responses.Customer.Basket;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services.Customer;

public class BasketCustomerService(IEfUnitOfWork _uow, IEfRepository<Basket> _basketRepository, 
    IEfRepository<Product> _productRepository, IEfRepository<User> _userRepository) : IBasketCustomerService
{

    public async Task<ApiResponse<BasketCustomerResponse>> ClearBasketItemsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetSingleAsync(
            filter: u => u.Id == userId,
            includeBuilder: q => q.Include(u => u.Basket)
                                  .ThenInclude(b => b.BasketItems),
            selector: x => x,
            asNoTracking: false,
            cancellationToken: cancellationToken
        );

        if (user is null)
        {
            return new ApiResponse<BasketCustomerResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = CustomerAuthConstants.UserNotFound
            };
        }

        user.ClearBasket();

        await _uow.SaveChangesAsync(cancellationToken);

        return new ApiResponse<BasketCustomerResponse>
        {
            Status = ResponseStatus.Success,
            Message = CustomerBasketConstants.BasketCleared
        };
    }

    public async Task<ApiResponse<BasketCustomerResponse>> GetBasketByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var basketDto = await _basketRepository.GetSingleAsync(
            filter: b => b.UserId == userId,
            selector: b => new BasketCustomerResponse
            {
                Items = b.BasketItems.Select(i => new BasketItemCustomerDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    Price = i.Product.UnitPrice,
                    UnitQuantity = i.Product.UnitQuantity,
                    Image = ImageDataUriBuilder.FromImage(i.Product.Image)
                }).ToList()
            },
            includeBuilder: q => q.Include(b => b.BasketItems)
                                  .ThenInclude(i => i.Product),
            cancellationToken: cancellationToken
        );

        basketDto ??= new BasketCustomerResponse { Items = new List<BasketItemCustomerDto>() };

        return new ApiResponse<BasketCustomerResponse>
        {
            Status = ResponseStatus.Success,
            Data = basketDto
        };
    }

    public async Task<ApiResponse<BasketCustomerResponse>> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetSingleAsync(
            filter: u => u.Id == userId,
            includeBuilder: q => q.Include(u => u.Basket)
                                  .ThenInclude(b => b.BasketItems),
            selector: u => u,
            cancellationToken: cancellationToken,
            asNoTracking: false
        );

        if (user is null)
        {
            return new ApiResponse<BasketCustomerResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = CustomerAuthConstants.UserNotFound
            };
        }

        var basket = user.Basket;
        if (basket is null)
        {
            return new ApiResponse<BasketCustomerResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = CustomerBasketConstants.BasketNotFoundForUser
            };
        }

        var itemToRemove = basket.BasketItems.FirstOrDefault(x => x.ProductId == productId);
        if (itemToRemove is null)
        {
            return new ApiResponse<BasketCustomerResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = CustomerBasketItemConstants.BasketItemNotFound
            };
        }

        basket.RemoveItem(productId);

        await _uow.SaveChangesAsync(cancellationToken);

        return new ApiResponse<BasketCustomerResponse>
        {
            Status = ResponseStatus.Success,
            Message = CustomerBasketItemConstants.BasketItemRemoved
        };
    }

    public async Task<ApiResponse<BasketCustomerResponse>> UpdateUserBasketAsync(Guid userId, UpdateBasketCustomerRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _userRepository.ExistsAsync(u => u.Id == userId, cancellationToken))
        {
            return new ApiResponse<BasketCustomerResponse>
            {
                Message = CustomerAuthConstants.UserNotFound,
                Status = ResponseStatus.NotFound
            };
        }

        var basket = await _basketRepository.GetSingleAsync(
            filter: b => b.UserId == userId,
            includeBuilder: q => q.Include(b => b.BasketItems),
            selector: q => q,
            asNoTracking: false,
            cancellationToken: cancellationToken
        );

        if (basket is null)
        {
            basket = Basket.CreateNew(userId);
            await _basketRepository.AddAsync(basket, cancellationToken);
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        var (products, _) = await _productRepository.QueryAsync(
            queryBuilder: q => q.Where(p => productIds.Contains(p.Id)),
            selector: p => p,
            asNoTracking: false,
            cancellationToken: cancellationToken
        );

        var productsDict = products.ToDictionary(p => p.Id);

        foreach (var reqItem in request.Items)
        {
            if (!productsDict.TryGetValue(reqItem.ProductId, out var product))
                continue;

            basket.AddOrUpdateItem(product, reqItem.Quantity);
        }

        await _uow.SaveChangesAsync(cancellationToken);

        return new ApiResponse<BasketCustomerResponse>
        {
            Message = CustomerBasketConstants.BasketUpdated,
            Status = ResponseStatus.Success
        };
    }


}
