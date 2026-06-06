using EAuditoria.Application.DTOs.Request;
using EAuditoria.Application.DTOs.Response;

namespace EAuditoria.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
