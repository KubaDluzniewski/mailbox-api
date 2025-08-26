using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class SesEmailService : ISesEmailService
{
    private readonly IAmazonSimpleEmailServiceV2 _ses;
    private readonly ILogger<SesEmailService> _logger;

    public SesEmailService(IAmazonSimpleEmailServiceV2 ses, ILogger<SesEmailService> logger)
    {
        _ses = ses;
        _logger = logger;
    }

    public async Task SendEmailAsync(string name, string from, string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(from))
            throw new ArgumentException("From address empty", nameof(from));
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("To address empty", nameof(to));

        var request = new SendEmailRequest
        {
            FromEmailAddress = $"{name} <{from}>",
            Destination = new Destination { ToAddresses = new List<string> { to } },
            Content = new EmailContent
            {
                Simple = new Message
                {
                    Subject = new Content { Charset = "UTF-8", Data = subject ?? string.Empty },
                    Body = new Body
                    {
                        Html = new Content { Charset = "UTF-8", Data = string.IsNullOrWhiteSpace(htmlBody) ? "(brak treści)" : htmlBody }
                    }
                }
            }
        };

        try
        {
            var resp = await _ses.SendEmailAsync(request, cancellationToken);
            _logger.LogInformation("SES SendEmail status {Status} messageId {MessageId}", resp.HttpStatusCode, resp?.MessageId);
        }
        catch (MessageRejectedException ex)
        {
            _logger.LogError(ex, "SES odrzucił wiadomość (MessageRejected) do {To}", to);
            throw;
        }
        catch (BadRequestException ex)
        {
            _logger.LogError(ex, "SES BadRequest przy wysyłce do {To}", to);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ogólny błąd SES przy wysyłce do {To}", to);
            throw;
        }
    }
}