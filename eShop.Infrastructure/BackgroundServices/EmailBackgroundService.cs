using eShop.Application.Interfaces.Customer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace eShop.Infrastructure.BackgroundServices;

public class EmailBackgroundService(IEmailQueue queue, IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger) : BackgroundService
{
    private readonly IEmailQueue _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ILogger<EmailBackgroundService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


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

                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await emailService.SendHtmlAsync(message.To, message.Subject, message.HtmlBody);

                _logger.LogInformation("Email sent to {To}", message.To);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", message?.To);
            }
        }

        _logger.LogInformation("EmailBackgroundService stopping.");
    }
}
