using eShop.Application.Interfaces.Customer;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace eShop.Infrastructure.Services;

public class MailKitEmailService(IConfiguration _config) : IEmailService
{

    public async Task SendHtmlAsync(string to, string subject, string html)
    {
        if (string.IsNullOrWhiteSpace(to)) throw new ArgumentException("Recipient is required.", nameof(to));
        if (string.IsNullOrWhiteSpace(subject)) subject = "(no subject)";

        var msg = new MimeMessage();
        var senderName = _config["Email:SenderName"] ?? "No-Reply";
        var senderEmail = _config["Email:SenderEmail"] ?? "no-reply@localhost";

        msg.From.Add(new MailboxAddress(senderName, senderEmail));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;

        msg.Body = new TextPart("html") { Text = html ?? string.Empty };

        using var smtp = new SmtpClient();

        var host = _config["Email:SmtpHost"] ?? "localhost";
        var port = 587;
        if (!string.IsNullOrWhiteSpace(_config["Email:SmtpPort"]) && int.TryParse(_config["Email:SmtpPort"], out var p)) port = p;

        await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls).ConfigureAwait(false);

        var user = _config["Email:SmtpUser"];
        var pass = _config["Email:SmtpPass"];

        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            await smtp.AuthenticateAsync(user, pass).ConfigureAwait(false);
        }

        await smtp.SendAsync(msg).ConfigureAwait(false);
        await smtp.DisconnectAsync(true).ConfigureAwait(false);
    }
}
