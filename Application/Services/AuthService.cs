using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using BCrypt.Net;
using Core.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
        // TODO: Dodanie hasowanego hasła
        var user = await _userCredentialRepository.FindSingleAsync(x => x.Email == email && x.Password == password);
        
        if (user == null)
        {
            return null;
        }
        var token = GenerateJwtToken(user.Id.ToString());
        return token;
    }

    public Task LogoutAsync()
    {
        // Brak logiki wylogowania dla JWT
        return Task.CompletedTask;
    }

    public Task<UserDto?> GetCurrentUserAsync()
    {
        // TODO: Implementacja pobierania użytkownika z tokena
        throw new NotImplementedException();
    }
    
    [Authorize]
    private string GenerateJwtToken(string userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}