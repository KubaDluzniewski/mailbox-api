using System.Text;
using Application;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

const string frontendCorsPolicy = "Frontend";


builder.Services.AddApplication()
                .AddInfrastructure(builder.Configuration);


builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// CORS: odczyt originów z konfiguracji (Cors:Origins jako CSV)
var corsOrigins = builder.Configuration.GetValue<string>("Cors:Origins") ?? "http://localhost:5173";
var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"]!;
var issuer = jwt["Issuer"];
var audience = jwt["Audience"];

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// HealthChecks (basic)
builder.Services.AddHealthChecks();

var app = builder.Build();


// Automatyczne migracje (prod-friendly)
if (!args.Contains("seed"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<MailboxDbContext>();
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                Console.WriteLine("--> Wykryto oczekujące migracje. Nakładanie...");
                await context.Database.MigrateAsync();
                Console.WriteLine("--> Migracje zakończone sukcesem.");
            }

            if (!await context.Users.AnyAsync())
            {
                Console.WriteLine("--> Wykryto pustą bazę. Seedowanie...");
                await DatabaseSeeder.SeedAsync(context);
                Console.WriteLine("--> Seedowanie zakończone.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Błąd podczas inicjalizacji bazy: {ex.Message}");
        }
    }
}

// Seed na żądanie: dotnet run -- seed
if (args.Contains("seed"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MailboxDbContext>();
    await DatabaseSeeder.SeedAsync(db);
    Console.WriteLine("Baza została zseedowana.");
    return;
}

app.UseCors(frontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
