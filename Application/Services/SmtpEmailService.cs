using Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Application.Services;

public class SmtpEmailService : IEmailService
{
    private readonly ILogger<SmtpEmailService> _logger;
    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _logger = logger;
        var smtp = configuration.GetSection("Smtp");
        _host      = smtp["Host"]      ?? throw new InvalidOperationException("Smtp:Host is not configured.");
        _port      = int.TryParse(smtp["Port"], out var p) ? p : 587;
        _username  = smtp["Username"]  ?? string.Empty;
        _password  = smtp["Password"]  ?? string.Empty;
        _fromEmail = smtp["FromEmail"] ?? "no-reply@mailbox.local";
        _fromName  = smtp["FromName"]  ?? "Mailbox";
    }

    public async Task SendEmailAsync(
        string name,
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Adres odbiorcy jest pusty.", nameof(to));

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress(name, to));
        message.Subject = subject ?? string.Empty;

        var bodyBuilder = new BodyBuilder { HtmlBody = string.IsNullOrWhiteSpace(htmlBody) ? "(brak treści)" : htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            var secureOption = string.IsNullOrWhiteSpace(_username)
                ? SecureSocketOptions.None
                : SecureSocketOptions.StartTls;
            await client.ConnectAsync(_host, _port, secureOption, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_username))
                await client.AuthenticateAsync(_username, _password, cancellationToken);

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("SMTP: Wysłano e-mail do {To}, temat: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP: Błąd podczas wysyłania e-maila do {To}", to);
            throw;
        }
    }
}
