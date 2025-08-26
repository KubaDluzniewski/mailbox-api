namespace Application.Interfaces;

public interface ISesEmailService
{
    Task SendEmailAsync(string name, string from, string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}