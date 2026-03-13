using eShop.Application.Interfaces.Customer;
using eShop.Infrastructure.BackgroundServices;

namespace eShop.Infrastructure.Tests.BackgroundServices;

public class InMemoryEmailQueueTests
{
    private readonly InMemoryEmailQueue _sut = new();

    [Fact]
    public async Task EnqueueAsync_SingleMessage_DequeueReturnsTheSameMessage()
    {
        var message = new EmailMessage("to@test.com", "Subject", "<p>Hello</p>");

        await _sut.EnqueueAsync(message);
        var dequeued = await _sut.DequeueAsync(CancellationToken.None);

        Assert.NotNull(dequeued);
        Assert.Equal(message.To, dequeued.To);
        Assert.Equal(message.Subject, dequeued.Subject);
        Assert.Equal(message.HtmlBody, dequeued.HtmlBody);
    }

    [Fact]
    public async Task EnqueueAsync_MultipleMessages_DequeueReturnsInFifoOrder()
    {
        var first  = new EmailMessage("a@test.com", "First",  "<p>1</p>");
        var second = new EmailMessage("b@test.com", "Second", "<p>2</p>");
        var third  = new EmailMessage("c@test.com", "Third",  "<p>3</p>");

        await _sut.EnqueueAsync(first);
        await _sut.EnqueueAsync(second);
        await _sut.EnqueueAsync(third);

        Assert.Equal("First",  (await _sut.DequeueAsync(CancellationToken.None))!.Subject);
        Assert.Equal("Second", (await _sut.DequeueAsync(CancellationToken.None))!.Subject);
        Assert.Equal("Third",  (await _sut.DequeueAsync(CancellationToken.None))!.Subject);
    }

    [Fact]
    public async Task DequeueAsync_WhenCancelled_ReturnsNull()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Queue is empty — dequeue will block until a message or cancellation
        var result = await _sut.DequeueAsync(cts.Token);

        Assert.Null(result);
    }

    [Fact]
    public async Task EnqueueAsync_ConcurrentProducers_AllMessagesAreDequeued()
    {
        const int count = 20;

        var enqueueTask = Parallel.ForEachAsync(
            Enumerable.Range(0, count),
            async (i, _) => await _sut.EnqueueAsync(
                new EmailMessage($"{i}@test.com", $"Subject {i}", $"<p>{i}</p>")));

        await enqueueTask;

        var received = new List<EmailMessage>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        for (int i = 0; i < count; i++)
        {
            var msg = await _sut.DequeueAsync(cts.Token);
            if (msg is not null)
                received.Add(msg);
        }

        Assert.Equal(count, received.Count);
    }
}
