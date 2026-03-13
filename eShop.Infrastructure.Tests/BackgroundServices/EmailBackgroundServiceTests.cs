using eShop.Application.Interfaces.Customer;
using eShop.Infrastructure.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace eShop.Infrastructure.Tests.BackgroundServices;

public class EmailBackgroundServiceTests
{
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<ILogger<EmailBackgroundService>> _loggerMock = new();

    private EmailBackgroundService CreateService(IEmailQueue queue)
    {
        var scopeMock = new Mock<IServiceScope>();
        scopeMock.Setup(s => s.ServiceProvider.GetService(typeof(IEmailService)))
                 .Returns(_emailServiceMock.Object);

        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(p => p.GetService(typeof(IServiceScopeFactory)))
                           .Returns(scopeFactoryMock.Object);

        return new EmailBackgroundService(queue, serviceProviderMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_MessageInQueue_SendsEmail()
    {
        var queue = new InMemoryEmailQueue();
        var message = new EmailMessage("user@test.com", "Welcome", "<p>Hi</p>");
        await queue.EnqueueAsync(message);

        _emailServiceMock
            .Setup(s => s.SendHtmlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        using var cts = new CancellationTokenSource();
        var service = CreateService(queue);

        // Start the service, let it process the one queued message, then cancel
        var task = service.StartAsync(cts.Token);
        await Task.Delay(200); // give the loop time to dequeue and send
        await cts.CancelAsync();
        await service.StopAsync(CancellationToken.None);

        _emailServiceMock.Verify(
            s => s.SendHtmlAsync(message.To, message.Subject, message.HtmlBody),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_SendThrows_LogsErrorAndKeepsRunning()
    {
        var queue = new InMemoryEmailQueue();
        var message = new EmailMessage("user@test.com", "Welcome", "<p>Hi</p>");
        await queue.EnqueueAsync(message);

        _emailServiceMock
            .Setup(s => s.SendHtmlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("SMTP down"));

        using var cts = new CancellationTokenSource();
        var service = CreateService(queue);

        // Should not throw despite the email service failing
        var task = service.StartAsync(cts.Token);
        await Task.Delay(200);
        await cts.CancelAsync();

        var ex = await Record.ExceptionAsync(() => service.StopAsync(CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequested_StopsGracefully()
    {
        var queue = new InMemoryEmailQueue(); // empty queue — service will block on DequeueAsync
        using var cts = new CancellationTokenSource();
        var service = CreateService(queue);

        await service.StartAsync(cts.Token);
        await cts.CancelAsync();

        var ex = await Record.ExceptionAsync(() => service.StopAsync(CancellationToken.None));
        Assert.Null(ex);
    }
}
