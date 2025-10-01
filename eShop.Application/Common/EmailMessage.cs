namespace eShop.Application.Common;

public record EmailMessage(string To, string Subject, string HtmlBody);
