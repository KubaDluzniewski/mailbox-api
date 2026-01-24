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
    private readonly string? _registrationSecret;
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
        _registrationSecret = configuration["Registration:Secret"];
    }



    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userService.GetByEmailWithRolesAsync(email);
        if (user == null) return null;

        var credential = await _userService.GetCredentialByUserIdAsync(user.Id);
        if (credential == null) return null;

        var passwordValid = BCrypt.Net.BCrypt.Verify(password, credential.PasswordHash);
        if (!passwordValid) return null;

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
        await _emailService.SendEmailAsync("Mailbox", email, subject, htmlBody);
        return true;
    }

    public async Task<bool> InitiateEmailChangeAsync(int userId, string newEmail)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null) return false;

        if (user.Email == newEmail)
        {
            if (!user.IsActive)
            {
                return await ActivateAsync(newEmail);
            }
            throw new Exception("This is already your email address.");
        }

        var existingUser = await _userService.GetByEmailAsync(newEmail);
        if (existingUser != null)
        {
            throw new Exception("Email is already taken.");
        }

        var oldTokens = await _userActivationTokenRepository.GetByUserIdAsync(userId, "email_change");
        foreach (var t in oldTokens)
            _userActivationTokenRepository.Remove(t);

        var token = Guid.NewGuid().ToString();
        var tokenEntity = new UserActivationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Type = "email_change",
            NewEmail = newEmail
        };
        await _userActivationTokenRepository.AddAsync(tokenEntity);
        await _userActivationTokenRepository.SaveChangesAsync();

        var confirmationLink = $"http://localhost:5173/confirm?token={token}&email={newEmail}";
        var subject = "Potwierdź zmianę adresu email";
        var htmlBody = $"<p>Kliknij link, aby potwierdzić zmianę adresu email: <a href='{confirmationLink}'>Zmień email</a></p>";

        await _emailService.SendEmailAsync("Mailbox", newEmail, subject, htmlBody);
        return true;
    }

    public async Task<string?> ConfirmAsync(string email, string token)
    {
        var tokenEntity = await _userActivationTokenRepository.GetByTokenAsync(token);
        if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow)
            return null;

        var user = await _userService.GetByIdAsync(tokenEntity.UserId);
        if (user == null) return null;

        string confirmationType = tokenEntity.Type;

        if (confirmationType == "activation")
        {
            if (user.Email != email || user.IsActive) return null;
            user.IsActive = true;
            _userRepository.Update(user);
        }
        else if (confirmationType == "email_change")
        {
            if (tokenEntity.NewEmail != email) return null;
            user.Email = tokenEntity.NewEmail;
            _userRepository.Update(user);
        }
        else
        {
            return null;
        }

        _userActivationTokenRepository.Remove(tokenEntity);
        await _userRepository.SaveChangesAsync();
        await _userActivationTokenRepository.SaveChangesAsync();
        return confirmationType;
    }

    public async Task<string?> ForgotPasswordAsync(string email)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            Console.WriteLine($"[ForgotPassword] User not found for email: {email}");
            return null;
        }

        var oldTokens = await _userActivationTokenRepository.GetByUserIdAsync(user.Id, "password_reset");
        foreach (var t in oldTokens)
            _userActivationTokenRepository.Remove(t);

        var token = Guid.NewGuid().ToString();
        var tokenEntity = new UserActivationToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Type = "password_reset"
        };
        await _userActivationTokenRepository.AddAsync(tokenEntity);
        await _userActivationTokenRepository.SaveChangesAsync();

        var encodedEmail = System.Net.WebUtility.UrlEncode(email);
        var encodedToken = System.Net.WebUtility.UrlEncode(token);

        var resetLink = $"http://localhost:5173/reset-password?token={encodedToken}&email={encodedEmail}";
        var subject = "Reset hasła";
        var htmlBody = $"<p>Kliknij link, aby zresetować hasło: <a href='{resetLink}'>Zresetuj hasło</a></p>";

        Console.WriteLine($"[ForgotPassword] Generated link: {resetLink}");

        try
        {
            await _emailService.SendEmailAsync("Mailbox", email, subject, htmlBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ForgotPassword] Email send failed (expected in dev without creds): {ex.Message}");
        }

        return resetLink;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        Console.WriteLine($"[ResetPassword] Attempting reset for email: {email}, token: {token}");
        var tokenEntity = await _userActivationTokenRepository.GetByTokenAsync(token);

        if (tokenEntity == null) {
            Console.WriteLine($"[ResetPassword] Token not found: {token}");
            return false;
        }

        if (tokenEntity.ExpiresAt < DateTime.UtcNow) {
            Console.WriteLine($"[ResetPassword] Token expired. Expires: {tokenEntity.ExpiresAt}, Now: {DateTime.UtcNow}");
            return false;
        }

        if (tokenEntity.Type != "password_reset") {
             Console.WriteLine($"[ResetPassword] Invalid token type: {tokenEntity.Type}");
             return false;
        }

        var user = await _userService.GetByIdAsync(tokenEntity.UserId);
        if (user == null) {
            Console.WriteLine($"[ResetPassword] User not found for ID: {tokenEntity.UserId}");
            return false;
        }

        if (user.Email != email) {
            Console.WriteLine($"[ResetPassword] Email mismatch. TokenUser: {user.Email}, Request: {email}");
            return false;
        }

        var result = await _userService.SetPasswordAsync(user.Id, newPassword);
        if (!result) {
            Console.WriteLine($"[ResetPassword] SetPasswordAsync failed.");
            return false;
        }

        _userActivationTokenRepository.Remove(tokenEntity);
        await _userActivationTokenRepository.SaveChangesAsync();
        Console.WriteLine($"[ResetPassword] Password reset successful for user: {email}");
        return true;
    }

    public async Task<bool> RegisterAsync(string name, string surname, string email, string password, string registrationCode)
    {
        if (string.IsNullOrWhiteSpace(_registrationSecret))
        {
             Console.WriteLine("Registration secret is not configured on server.");
             return false;
        }

        if (registrationCode != _registrationSecret) return false;

        var roles = new List<UserRole> { UserRole.STUDENT };
        var createdUser = await _userService.CreateUserAsync(name, surname, email, password, roles, isActive: false);

        if (createdUser != null)
        {
            await ActivateAsync(email);
            return true;
        }

        return false;
    }

    private string GenerateJwtToken(User user)
    {
        var roles = user.Roles?.Select(r => r.Role.ToString()).ToList() ?? new List<string>();
        var rolesString = string.Join(",", roles);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("email", user.Email ?? string.Empty),
            new Claim("name", user.Name ?? string.Empty),
            new Claim("surname", user.Surname ?? string.Empty),
            new Claim("isActive", user.IsActive.ToString()),
            new Claim("roles", rolesString)
        };

        // Add individual role claims for ASP.NET authorization
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

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
