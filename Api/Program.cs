using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "Frontend";

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<MailboxDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

// --- AWS SES konfiguracja ---
var awsSection = builder.Configuration.GetSection("AWS");
var regionName = awsSection["Region"] ?? "eu-central-1";
var accessKey = awsSection["AccessKey"];
var secretKey = awsSection["SecretKey"];

builder.Services.AddSingleton<IAmazonSimpleEmailServiceV2>(_ =>
{
    var region = RegionEndpoint.GetBySystemName(regionName);

    if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
    {
        // DEV: jawne klucze (User Secrets / zmienne środowiskowe). Nie commitować.
        return new AmazonSimpleEmailServiceV2Client(new BasicAWSCredentials(accessKey, secretKey), region);
    }

    // Fallback: domyślny łańcuch (ENV, profil ~/.aws/credentials, IMDS)
    return new AmazonSimpleEmailServiceV2Client(region);
});

builder.Services.AddScoped<ISesEmailService, SesEmailService>();
// --- koniec AWS SES ---

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!))
    };
});

var app = builder.Build();

if (args.Contains("seed"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MailboxDbContext>();
    await DatabaseSeeder.SeedAsync(db);
    Console.WriteLine("Baza została zseedowana.");
    return;
}

if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var ses = scope.ServiceProvider.GetRequiredService<IAmazonSimpleEmailServiceV2>();
        // Lekki test (opcjonalny): await ses.ListEmailIdentitiesAsync(new());
    }
    catch (Exception ex)
    {
        Console.WriteLine("AWS SES init error: " + ex.Message);
    }

    var testResult = System.Diagnostics.Process.Start("dotnet", "test ../Application.Tests");
    testResult?.WaitForExit();
    Console.WriteLine("Testy zostały uruchomione.");
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();