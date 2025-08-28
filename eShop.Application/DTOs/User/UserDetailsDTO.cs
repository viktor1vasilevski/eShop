using eShop.Application.DTOs.Order;

namespace eShop.Application.DTOs.User;

public class UserDetailsDTO : UserDTO
{
    public List<OrderDetailsDTO> Orders { get; set; }
}
