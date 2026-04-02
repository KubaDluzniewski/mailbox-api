namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string name, string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
