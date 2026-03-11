using eShop.Domain.Exceptions;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Tests.ValueObjects;

public class UnitPriceTests
{
    [Fact]
    public void Create_ValidPrice_ReturnsUnitPrice()
    {
        var price = UnitPrice.Create(9.99m);
        Assert.Equal(9.99m, price.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_ZeroOrNegative_ThrowsDomainValidationException(decimal value)
    {
        Assert.Throws<DomainValidationException>(() => UnitPrice.Create(value));
    }

    [Fact]
    public void Multiply_ReturnsCorrectTotal()
    {
        var price = UnitPrice.Create(10m);
        Assert.Equal(30m, price.Multiply(3));
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = UnitPrice.Create(9.99m);
        var b = UnitPrice.Create(9.99m);
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = UnitPrice.Create(9.99m);
        var b = UnitPrice.Create(19.99m);
        Assert.NotEqual(a, b);
    }
}
