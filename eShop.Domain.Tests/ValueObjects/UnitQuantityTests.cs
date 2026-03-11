using eShop.Domain.Exceptions;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Tests.ValueObjects;

public class UnitQuantityTests
{
    [Fact]
    public void Create_ValidQuantity_ReturnsUnitQuantity()
    {
        var quantity = UnitQuantity.Create(10);
        Assert.Equal(10, quantity.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ZeroOrNegative_ThrowsDomainValidationException(int value)
    {
        Assert.Throws<DomainValidationException>(() => UnitQuantity.Create(value));
    }

    [Fact]
    public void Subtract_ValidAmount_ReturnsReducedQuantity()
    {
        var quantity = UnitQuantity.Create(10);
        var result = quantity.Subtract(3);
        Assert.Equal(7, result.Value);
    }

    [Fact]
    public void Subtract_EntireQuantity_ReturnsZeroIsNotAllowed()
    {
        var quantity = UnitQuantity.Create(5);
        Assert.Throws<DomainValidationException>(() => quantity.Subtract(5));
    }

    [Fact]
    public void Subtract_MoreThanAvailable_ThrowsDomainValidationException()
    {
        var quantity = UnitQuantity.Create(5);
        Assert.Throws<DomainValidationException>(() => quantity.Subtract(10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Subtract_ZeroOrNegativeAmount_ThrowsDomainValidationException(int amount)
    {
        var quantity = UnitQuantity.Create(10);
        Assert.Throws<DomainValidationException>(() => quantity.Subtract(amount));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = UnitQuantity.Create(10);
        var b = UnitQuantity.Create(10);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = UnitQuantity.Create(10);
        var b = UnitQuantity.Create(20);
        Assert.NotEqual(a, b);
    }
}
