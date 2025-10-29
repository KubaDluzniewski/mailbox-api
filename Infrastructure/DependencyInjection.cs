using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Application.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<MailboxDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();

        // AWS SES client
        var awsSection = configuration.GetSection("AWS");
        var regionName = awsSection["Region"] ?? "eu-central-1";
        var accessKey = awsSection["AccessKey"];
        var secretKey = awsSection["SecretKey"];

        services.AddSingleton<IAmazonSimpleEmailServiceV2>(_ =>
        {
            var region = RegionEndpoint.GetBySystemName(regionName);
            if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
                return new AmazonSimpleEmailServiceV2Client(new BasicAWSCredentials(accessKey, secretKey), region);
            return new AmazonSimpleEmailServiceV2Client(region);
        });

        return services;
    }
}

