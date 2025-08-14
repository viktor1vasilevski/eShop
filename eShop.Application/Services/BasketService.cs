using eShop.Application.Constants;
using eShop.Application.DTOs.Basket;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Basket;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services
{
    public class BasketService(IUnitOfWork _uow) : IBasketService
    {
        private readonly IRepositoryBase<Basket> _basketRepository = _uow.GetRepository<Basket>();
        private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();
        private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();



        public async Task<ApiResponse<BasketDTO>> GetBasketByUserIdAsync(Guid userId)
        {
            var baskets = await _basketRepository.GetAsync(
                filter: b => b.UserId == userId,
                include: b => b.Include(x => x.Items).ThenInclude(i => i.Product));

            var basket = baskets.FirstOrDefault();

            if (basket is null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = BasketConstants.BASKET_NOT_FOUND_FOR_USER
                };

            var basketDto = new BasketDTO
            {
                Items = basket.Items.Select(i => new BasketItemDTO
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    Quantity = i.Quantity,
                    Price = i.Product?.UnitPrice ?? 0,
                    UnitQuantity = i.Product?.UnitQuantity ?? 0,
                    Image = ImageHelper.BuildImageDataUrl(i.Product?.Image, i.Product?.ImageType)
                }).ToList()
            };

            return new ApiResponse<BasketDTO>
            {
                NotificationType = NotificationType.Success,
                Data = basketDto
            };
        }

        public async Task<ApiResponse<BasketDTO>> MergeItemsAsync(Guid userId, List<BasketRequest> request)
        {
            // Check if user exists
            var userExists = await _userRepository.ExistsAsync(x => x.Id == userId);
            if (!userExists)
                return new ApiResponse<BasketDTO>
                {
                    Message = UserConstants.USER_NOT_FOUND,
                    NotificationType = NotificationType.NotFound
                };

            // Load basket including items (tracked)
            var basket = _basketRepository.Get(
                filter: x => x.UserId == userId,
                include: x => x.Include(b => b.Items)).FirstOrDefault();

            if (basket == null)
            {
                basket = Basket.CreateNew(userId);
                await _basketRepository.InsertAsync(basket);
            }

            // Load all products needed for validation
            var productIds = request.Select(r => r.ProductId).Distinct().ToList();
            var products = await _productRepository.GetAsync(x => productIds.Contains(x.Id));
            var productsDict = products.ToDictionary(p => p.Id);

            // Merge items
            foreach (var reqItem in request)
            {
                if (!productsDict.TryGetValue(reqItem.ProductId, out var product))
                    continue; // skip invalid products

                // Use Basket method to ensure EF tracking is correct
                basket.AddItem(product, reqItem.Quantity);
            }

            // Save everything at once
            await _uow.SaveChangesAsync();

            return new ApiResponse<BasketDTO>
            {
                Data = null,
                Message = BasketConstants.BASKET_MERGED,
                NotificationType = NotificationType.Success
            };
        }


        public async Task<ApiResponse<BasketDTO>> UpdateItemQuantityAsync(Guid userId, Guid productId, int quantityToAdd)
        {
            // Load basket and items directly for user
            var basket = (await _basketRepository.GetAsync(
                filter: b => b.UserId == userId,
                include: b => b.Include(b => b.Items)))
                .FirstOrDefault();

            if (basket == null)
            {
                // Create new basket if none exists
                basket = Basket.CreateNew(userId);
                await _basketRepository.InsertAsync(basket);
            }

            // Load product to validate
            var product = _productRepository.GetById(productId);
            if (product == null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = "Product not found"
                };

            // Try to find existing item in basket
            var basketItem = basket.Items.FirstOrDefault(i => i.ProductId == productId);

            if (basketItem != null)
            {
                basket.UpdateItemQuantity(productId, basketItem.Quantity + quantityToAdd);
            }
            else
            {
                basket.AddItem(product, quantityToAdd);
            }

            await _uow.SaveChangesAsync();

            return new ApiResponse<BasketDTO>
            {
                Data = null,
                NotificationType = NotificationType.Success,
                Message = "Basket updated successfully"
            };
        }


        public async Task<ApiResponse<BasketDTO>> ClearBasketItemsForUserAsync(Guid userId)
        {
            var userQuery = await _userRepository.GetAsync(
                filter: x => x.Id == userId,
                include: x => x.Include(x => x.Basket).ThenInclude(b => b.Items));

            var user = userQuery.FirstOrDefault();
            if (user is null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = UserConstants.USER_NOT_FOUND,
                };

            user?.ClearBasket();

            await _uow.SaveChangesAsync();

            return new ApiResponse<BasketDTO>
            {
                NotificationType = NotificationType.Success
            };
        }


        public async Task<ApiResponse<BasketDTO>> RemoveItemAsync(Guid userId, Guid productId)
        {
            var userQuery = await _userRepository.GetAsync(
                filter: x => x.Id == userId,
                include: x => x.Include(x => x.Basket).ThenInclude(b => b.Items));

            var user = userQuery.FirstOrDefault();
            if (user is null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = UserConstants.USER_NOT_FOUND,
                };

            var basket = user.Basket;
            if (basket == null)
            {
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = "Basket not found",
                };
            }

            var itemToRemove = basket.Items.FirstOrDefault(x => x.ProductId == productId);
            if (itemToRemove == null)
            {
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = "Item not found in basket",
                };
            }

            basket.RemoveItem(productId);

            await _uow.SaveChangesAsync();

            return new ApiResponse<BasketDTO>
            {
                NotificationType = NotificationType.Success
            };
        }

    }
}
