using System.Security.Claims;

namespace eShop.Application.Extensions;

public static class ClaimsPrincipalExtensions
{
    private static readonly string[] _userIdClaimKeys =
    {
        ClaimTypes.NameIdentifier,
        "sub",
        "uid",
        "userId"
    };

    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        foreach (var key in _userIdClaimKeys)
        {
            var claim = user.FindFirst(key);
            if (claim != null && Guid.TryParse(claim.Value, out var id))
                return id;
        }

        return null;
    }
}
