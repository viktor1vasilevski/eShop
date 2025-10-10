using eShop.Application.Interfaces.Customer;
using System.Threading.Channels;

namespace eShop.Infrastructure.BackgroundServices;

public class InMemoryEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });

    public ValueTask EnqueueAsync(EmailMessage message) =>
        _channel.Writer.WriteAsync(message);

    public async ValueTask<EmailMessage?> DequeueAsync(CancellationToken ct)
    {
        try
        {
            var msg = await _channel.Reader.ReadAsync(ct);
            return msg;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }
}
