using eShop.Application.Common;
using eShop.Application.Interfaces.Customer;
using System.Threading.Channels;

namespace eShop.Infrastructure.Services;

public class InMemoryEmailQueue : IEmailQueue
{
    private readonly Channel<EmailMessage> _channel;

    public InMemoryEmailQueue()
    {
        _channel = Channel.CreateUnbounded<EmailMessage>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

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

    public ValueTask EnqueueAsync(EmailMessage message) =>
        _channel.Writer.WriteAsync(message);
}
