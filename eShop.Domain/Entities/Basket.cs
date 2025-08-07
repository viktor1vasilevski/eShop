using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;

namespace eShop.Domain.Entities;

public class Basket : AuditableBaseEntity
{
    public Guid UserId { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<BasketItem> Items { get; set; } = [];


    public static Basket CreateNew(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        return new Basket
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };
    }
}
