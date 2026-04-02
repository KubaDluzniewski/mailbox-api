using Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests;

public class SmtpEmailServiceTests
{
    [Fact]
    public void Constructor_Throws_WhenHostMissing()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = null,
                ["Smtp:Port"] = "587"
            })
            .Build();

        var logger = new Mock<ILogger<SmtpEmailService>>();

        Assert.Throws<InvalidOperationException>(() => new SmtpEmailService(config, logger.Object));
    }

    [Fact]
    public async Task SendEmailAsync_ThrowsArgumentException_WhenRecipientEmpty()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Smtp:Host"] = "smtp.local",
                ["Smtp:Port"] = "587",
                ["Smtp:FromEmail"] = "no-reply@mailbox.local",
                ["Smtp:FromName"] = "Mailbox"
            })
            .Build();

        var logger = new Mock<ILogger<SmtpEmailService>>();
        var sut = new SmtpEmailService(config, logger.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.SendEmailAsync("Jan", "", "Subject", "Body"));
    }
}
