using eShop.Application.Interfaces.Customer;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class CommentCustomerService(IUnitOfWork _uow) : ICommentCustomerService
{
    private readonly IEfRepository<Comment> _commentRepository = _uow.GetEfRepository<Comment>();
}
