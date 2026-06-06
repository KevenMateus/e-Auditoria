using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;
using EAuditoria.Application.Interfaces.Repositories;
using EAuditoria.Application.Interfaces.Services;
using EAuditoria.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EAuditoria.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuarioRepository usuarioRepository, IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var emailLower = request.Email.ToLowerInvariant();

        var usuario = await _usuarioRepository.ObterPorEmailAsync(emailLower);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        if (!usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativo.");

        usuario.RegistrarLogin();
        await _usuarioRepository.SalvarAsync();

        var token = GerarToken(usuario);

        return new AuthResponse
        {
            Token            = token,
            ExpiresInSeconds = 3600 * 8,
            Nome             = usuario.Nome,
            Email            = usuario.Email,
            Perfil           = usuario.Perfil,
        };
    }

    private string GerarToken(Usuario usuario)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key não configurada.");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name,  usuario.Nome),
            new Claim(ClaimTypes.Role,               usuario.Perfil),
            new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"]   ?? "e-auditoria",
            audience:           _configuration["Jwt:Audience"] ?? "e-auditoria-frontend",
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
