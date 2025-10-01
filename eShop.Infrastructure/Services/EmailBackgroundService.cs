using eShop.Application.Common;
using eShop.Application.Interfaces.Customer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShop.Infrastructure.Services;

public class EmailBackgroundService : BackgroundService
{
    private readonly IEmailQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailBackgroundService> _logger;

    public EmailBackgroundService(
        IEmailQueue queue,
        IServiceProvider serviceProvider,
        ILogger<EmailBackgroundService> logger)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailBackgroundService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            EmailMessage? message = null;
            try
            {
                message = await _queue.DequeueAsync(stoppingToken);
                if (message is null) continue;

                _logger.LogInformation("Dequeued email for {To} (subject: {Subject})", message.To, message.Subject);

                // Create a scope so we can resolve scoped services safely
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await emailService.SendHtmlAsync(message.To, message.Subject, message.HtmlBody);

                _logger.LogInformation("Email sent to {To}", message.To);
            }
            catch (OperationCanceledException)
            {
                // Host is shutting down
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", message?.To);
                // For learning: log and continue. In prod, consider persistence/retries.
            }
        }

        _logger.LogInformation("EmailBackgroundService stopping.");
    }
}
