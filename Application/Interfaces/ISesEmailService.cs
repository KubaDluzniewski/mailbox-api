namespace Application.Interfaces;

public interface ISesEmailService
{
    Task SendEmailAsync(string name, string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
