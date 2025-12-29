using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Core.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly string? _jwtKey;
    private readonly string? _jwtIssuer;
    private readonly string? _jwtAudience;
    private readonly IRepository<UserCredential> _userCredentialRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISesEmailService _emailService;
    private readonly IUserActivationTokenRepository _userActivationTokenRepository;

    public AuthService(
        IUserService userService,
        IMapper mapper,
        IConfiguration configuration,
        IRepository<UserCredential> userCredentialRepository,
        IUserRepository userRepository,
        ISesEmailService emailService,
        IUserActivationTokenRepository userActivationTokenRepository)
    {
        _userService = userService;
        _mapper = mapper;
        _userCredentialRepository = userCredentialRepository;
        _userRepository = userRepository;
        _emailService = emailService;
        _userActivationTokenRepository = userActivationTokenRepository;
        _jwtKey = configuration["Jwt:Key"];
        _jwtIssuer = configuration["Jwt:Issuer"];
        _jwtAudience = configuration["Jwt:Audience"];
    }
    
    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            return null;
        }
        var credential = await _userService.GetCredentialByUserIdAsync(user.Id);
        if (credential == null)
        {
            return null;
        }
        var passwordValid = BCrypt.Net.BCrypt.Verify(password, credential.PasswordHash);
        if (!passwordValid)
        {
            return null;
        }
        var token = GenerateJwtToken(user);
        Console.WriteLine($"Generated token for user: {user.Id}, {user.Name}, {user.Surname}, {user.Email}, {user.IsActive}");
        return token;
    }

    public Task LogoutAsync() => Task.CompletedTask;

    public Task<UserDto?> GetCurrentUserAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsActiveAsync(string email)
    {
        var user = await _userService.GetByEmailAsync(email);
        return user?.IsActive ?? false;
    }

    public async Task<bool> ActivateAsync(string email)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null || user.IsActive) return false;

        // Usuń stare tokeny aktywacyjne
        var oldTokens = await _userActivationTokenRepository.GetByUserIdAsync(user.Id, "activation");
        foreach (var t in oldTokens)
            _userActivationTokenRepository.Remove(t);

        var activationToken = Guid.NewGuid().ToString();
        var expires = DateTime.UtcNow.AddHours(24);
        var tokenEntity = new UserActivationToken
        {
            UserId = user.Id,
            Token = activationToken,
            ExpiresAt = expires,
            Type = "activation"
        };
        await _userActivationTokenRepository.AddAsync(tokenEntity);
        await _userActivationTokenRepository.SaveChangesAsync();

        var confirmationLink = $"http://localhost:5173/confirm?token={activationToken}&email={email}";

        var subject = "Potwierdzenie konta";
        var htmlBody = $"<p>Kliknij link, aby aktywować konto: <a href='{confirmationLink}'>Aktywuj konto</a></p>";

        await _emailService.SendEmailAsync("Mailbox", "kubagierki123@gmail.com", email, subject, htmlBody);

        return true;
    }

    public async Task<bool> ConfirmAsync(string email, string token)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null || user.IsActive) return false;

        var tokenEntity = await _userActivationTokenRepository.GetByTokenAsync(token, "activation");
        if (tokenEntity == null || tokenEntity.UserId != user.Id || tokenEntity.ExpiresAt < DateTime.UtcNow)
            return false;

        user.IsActive = true;
        _userRepository.Update(user);
        _userActivationTokenRepository.Remove(tokenEntity);
        await _userRepository.SaveChangesAsync();
        await _userActivationTokenRepository.SaveChangesAsync();
        return true;
    }
    
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("email", user.Email ?? string.Empty),
            new Claim("name", user.Name ?? string.Empty),
            new Claim("surname", user.Surname ?? string.Empty),
            new Claim("isActive", user.IsActive.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}