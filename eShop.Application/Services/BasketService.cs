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
        private readonly IRepositoryBase<BasketItem> _basketItemRepository = _uow.GetRepository<BasketItem>();


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

        public async Task<ApiResponse<BasketDTO>> Merge(Guid userId, List<BasketRequest> request)
        {
            var userExists = await _userRepository.ExistsAsync(x => x.Id == userId);
            if (!userExists)
                return new ApiResponse<BasketDTO>
                {
                    Message = UserConstants.USER_NOT_FOUND,
                    NotificationType = NotificationType.NotFound
                };

            // Load basket and include items
            var basket = _basketRepository.Get(
                filter: x => x.UserId == userId,
                include: x => x.Include(b => b.Items)).FirstOrDefault();

            if (basket == null)
            {
                basket = Basket.CreateNew(userId);
                await _basketRepository.InsertAsync(basket);
            }

            // Load products needed for validation, stock limits, etc.
            var productIds = request.Select(r => r.ProductId).Distinct().ToList();
            var products = await _productRepository.GetAsync(x => productIds.Contains(x.Id));
            var productsDict = products.ToDictionary(p => p.Id, p => p);

            // Merge basket items manually here:
            foreach (var reqItem in request)
            {
                if (!productsDict.TryGetValue(reqItem.ProductId, out var product))
                    continue; // skip invalid product

                // Check if basket already has this product
                var basketItem = basket.Items.FirstOrDefault(i => i.ProductId == reqItem.ProductId);

                if (basketItem != null)
                {
                    // Add quantities, but do not exceed product's available unit quantity
                    int newQuantity = basketItem.Quantity + reqItem.Quantity;
                    basketItem.Quantity = Math.Min(newQuantity, product.UnitQuantity);
                }
                else
                {
                    // Add new basket item (make sure to not exceed stock limit)
                    var quantityToAdd = Math.Min(reqItem.Quantity, product.UnitQuantity);

                    basket.Items.Add(new BasketItem
                    {
                        ProductId = reqItem.ProductId,
                        Quantity = quantityToAdd,
                        // fill other needed properties if applicable, e.g. price, name
                    });
                }
            }

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
            var user = (await _userRepository.GetAsync(
                filter: x => x.Id == userId,
                include: x => x.Include(x => x.Basket).ThenInclude(x => x.Items))).FirstOrDefault();

            if (user == null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = UserConstants.USER_NOT_FOUND
                };

            var product = _productRepository.GetById(productId);
            if (product == null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = "Product not found"
                };

            if (user.Basket == null)
            {
                user.Basket = Basket.CreateNew(userId);
            }

            user.Basket.AddOrUpdateItem(product, quantityToAdd);

            await _uow.SaveChangesAsync();

            return new ApiResponse<BasketDTO>
            {
                Data = null,
                NotificationType = NotificationType.Success,
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

            user?.Basket?.Items.Clear();
            await _uow.SaveChangesAsync();

            return new ApiResponse<BasketDTO>
            {
                NotificationType = NotificationType.Success
            };
        }
    }
}
