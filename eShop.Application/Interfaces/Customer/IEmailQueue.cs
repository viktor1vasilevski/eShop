namespace eShop.Application.Interfaces.Customer;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailMessage message);
    ValueTask<EmailMessage?> DequeueAsync(CancellationToken ct);
}

public record EmailMessage(string To, string Subject, string HtmlBody);
