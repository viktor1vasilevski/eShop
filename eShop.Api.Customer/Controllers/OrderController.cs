using eShop.Application.Interfaces.Customer;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IOrderCustomerService _orderCustomerService) : BaseController
{
}
