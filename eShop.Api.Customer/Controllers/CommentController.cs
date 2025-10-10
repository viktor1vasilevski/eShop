using eShop.Application.Interfaces.Customer;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController(ICommentCustomerService _commentCustomerService) : BaseController
{
}
