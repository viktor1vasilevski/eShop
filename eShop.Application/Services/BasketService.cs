using eShop.Application.Constants;
using eShop.Application.DTOs.Basket;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Basket;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces;
using eShop.Domain.Models;
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
                    ImageDataUrl = ImageHelper.BuildImageDataUrl(i.Product?.Image, i.Product?.ImageType)
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

            var basket = _basketRepository.Get(
                filter: x => x.UserId == userId, 
                include: x => x.Include(b => b.Items)).FirstOrDefault();

            if (basket is null)
            {
                basket = Basket.CreateNew(userId);
                await _basketRepository.InsertAsync(basket);
            }

            var productIds = request.Select(r => r.ProductId).Distinct().ToList();
            var products = await _productRepository.GetAsync(x => productIds.Contains(x.Id));
            var productsDict = products.ToDictionary(p => p.Id, p => p);

            var mergeItems = request
                .Where(r => productsDict.ContainsKey(r.ProductId))
                .Select(r => new BasketMergeItem(productsDict[r.ProductId], r.Quantity))
                .ToList();

            basket.MergeItems(mergeItems);

            await _uow.SaveChangesAsync();

            return new ApiResponse<BasketDTO>
            {
                Message = BasketConstants.BASKET_MERGED,
                NotificationType = NotificationType.Success
            };
        }


        public async Task<ApiResponse<BasketDTO>> UpdateItemQuantityAsync(Guid userId, Guid productId, int newQuantity)
        {
            var userQuery = await _userRepository.GetAsync(
                filter: x => x.Id == userId,
                include: x => x.Include(x => x.Basket).ThenInclude(b => b.Items).ThenInclude(i => i.Product));

            var user = userQuery.FirstOrDefault();
            if (user is null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = UserConstants.USER_NOT_FOUND,
                };

            var product = _productRepository.GetById(productId);
            if (product is null)
                return new ApiResponse<BasketDTO>
                {
                    NotificationType = NotificationType.NotFound,
                    Message = "Product not found"
                };

            if (user.Basket is null)
            {
                var basket = Basket.CreateNew(userId);
                basket.AddOrUpdateItem(product, newQuantity);

                await _basketRepository.InsertAsync(basket);
                // Insert all new items in the basket (only one in this case)
                foreach (var item in basket.Items)
                {
                    await _basketItemRepository.InsertAsync(item);
                }
            }
            else
            {
                // Update or add item
                var basket = user.Basket;

                // Check if item exists
                var existingItem = basket.Items.FirstOrDefault(i => i.ProductId == productId);
                basket.AddOrUpdateItem(product, newQuantity);

                if (existingItem == null)
                {
                    // New item inserted
                    var newItem = basket.Items.FirstOrDefault(i => i.ProductId == productId);
                    if (newItem != null)
                        await _basketItemRepository.InsertAsync(newItem);
                }
                else
                {
                    // Existing item updated
                    await _basketItemRepository.UpdateAsync(existingItem);
                }

                await _basketRepository.UpdateAsync(basket);
            }

            await _uow.SaveChangesAsync();

            // Build basket DTO for response
            var basketDto = new BasketDTO
            {
                Items = user.Basket?.Items.Select(i => new BasketItemDTO
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name,
                    Quantity = i.Quantity,
                    Price = i.Product?.UnitPrice ?? 0,
                    UnitQuantity = i.Product?.UnitQuantity ?? 0,
                    ImageDataUrl = ImageHelper.BuildImageDataUrl(i.Product?.Image, i.Product?.ImageType)
                }).ToList() ?? new List<BasketItemDTO>()
            };

            return new ApiResponse<BasketDTO>
            {
                NotificationType = NotificationType.Success,
                Data = basketDto
            };
        }


    }
}
