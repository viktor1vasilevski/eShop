using eShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IOrderService _orderService) : BaseController
{
}
