using eShop.Domain.Exceptions;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Tests.ValueObjects;

public class ProductNameTests
{
    [Fact]
    public void Create_ValidName_ReturnsProductName()
    {
        var name = ProductName.Create("Laptop");
        Assert.Equal("Laptop", name.Value);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var name = ProductName.Create("  Laptop  ");
        Assert.Equal("Laptop", name.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ThrowsDomainValidationException(string value)
    {
        Assert.Throws<DomainValidationException>(() => ProductName.Create(value));
    }

    [Fact]
    public void Create_NameExceeds50Characters_ThrowsDomainValidationException()
    {
        var longName = new string('a', 51);
        Assert.Throws<DomainValidationException>(() => ProductName.Create(longName));
    }

    [Fact]
    public void Create_NameExactly50Characters_Succeeds()
    {
        var name = ProductName.Create(new string('a', 50));
        Assert.Equal(50, name.Value.Length);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = ProductName.Create("Laptop");
        var b = ProductName.Create("Laptop");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentValues_AreNotEqual()
    {
        var a = ProductName.Create("Laptop");
        var b = ProductName.Create("Phone");
        Assert.NotEqual(a, b);
    }
}
