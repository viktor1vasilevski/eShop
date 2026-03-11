using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Models;

namespace eShop.Domain.Tests.Models;

public class CommentTests
{
    [Fact]
    public void Create_ValidData_SetsPropertiesCorrectly()
    {
        var productId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = Comment.Create("Great product!", Rating.Excellent, productId, userId);

        Assert.Equal("Great product!", comment.Text.Value);
        Assert.Equal(Rating.Excellent, comment.Rating);
        Assert.Equal(productId, comment.ProductId);
        Assert.Equal(userId, comment.UserId);
        Assert.NotEqual(Guid.Empty, comment.Id);
    }

    [Fact]
    public void Create_EmptyProductId_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            Comment.Create("text", Rating.Good, Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Create_EmptyUserId_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            Comment.Create("text", Rating.Good, Guid.NewGuid(), Guid.Empty));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyCommentText_ThrowsDomainValidationException(string text)
    {
        Assert.Throws<DomainValidationException>(() =>
            Comment.Create(text, Rating.Good, Guid.NewGuid(), Guid.NewGuid()));
    }
}
