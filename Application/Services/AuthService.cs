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

    public AuthService(IUserService userService, IMapper mapper, IConfiguration configuration, IRepository<UserCredential> userCredentialRepository)
    {
        _userService = userService;
        _mapper = mapper;
        _userCredentialRepository = userCredentialRepository;
        _jwtKey = configuration["Jwt:Key"];
        _jwtIssuer = configuration["Jwt:Issuer"];
        _jwtAudience = configuration["Jwt:Audience"];
    }
    
    public async Task<string?> LoginAsync(string email, string password)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
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
        return token;
    }

    public Task LogoutAsync() => Task.CompletedTask;

    public Task<UserDto?> GetCurrentUserAsync()
    {
        throw new NotImplementedException();
    }
    
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
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