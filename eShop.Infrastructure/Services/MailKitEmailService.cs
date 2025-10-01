using eShop.Application.Interfaces.Customer;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;


namespace eShop.Infrastructure.Services;

public class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public MailKitEmailService(IConfiguration config) => _config = config ?? throw new ArgumentNullException(nameof(config));

    public async Task SendHtmlAsync(string to, string subject, string html)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_config["Email:SenderName"], _config["Email:SenderEmail"]));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        msg.Body = new TextPart("html")
        {
            Text = html
        };

        using var smtp = new SmtpClient();

        var host = _config["Email:SmtpHost"];
        var portStr = _config["Email:SmtpPort"];
        var port = 587;
        if (!string.IsNullOrWhiteSpace(portStr) && int.TryParse(portStr, out var p)) port = p;

        // Connect. Use StartTls by default; if you use local dev SMTP without TLS set SecureSocketOptions.None
        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

        var user = _config["Email:SmtpUser"];
        var pass = _config["Email:SmtpPass"];

        // Only authenticate if credentials are provided
        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            await smtp.AuthenticateAsync(user, pass);
        }

        await smtp.SendAsync(msg);
        await smtp.DisconnectAsync(true);
    }

    private string BuildOrderHtml(string orderId, string productName, decimal total)
    {
        // Minimal HTML; later you'll replace it with the full BuildOrderHtml that uses order items.
        return $@"
            <h2>Thanks for your order!</h2>
            <p><b>Order ID:</b> {orderId}</p>
            <p><b>Product:</b> {System.Net.WebUtility.HtmlEncode(productName)}</p>
            <p><b>Total:</b> {total:C}</p>
            <p>We’ll notify you when it ships.</p>";
    }

    public async Task SendOrderConfirmationAsync(string to, string orderId, string productName, decimal total)
    {
        var subject = $"Order Confirmation #{orderId}";
        var html = BuildOrderHtml(orderId, productName, total);
        await SendHtmlAsync(to, subject, html);
    }
}
