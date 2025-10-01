using eShop.Application.Common;

namespace eShop.Application.Interfaces.Customer;

public interface IEmailQueue
{
    ValueTask EnqueueAsync(EmailMessage message);
    ValueTask<EmailMessage?> DequeueAsync(CancellationToken ct);
}
