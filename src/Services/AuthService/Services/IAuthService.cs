using Shared.DTOs;

namespace AuthService.Services;

public interface IAuthService
{
    Task<string> AuthenticateAsync(LoginDto loginDto);
    Task<UsuarioDto> RegisterAsync(CreateUsuarioDto createUsuarioDto);
    Task<UsuarioDto?> GetUserByIdAsync(int id);
    Task<UsuarioDto?> GetUserByEmailAsync(string email);
    Task<IEnumerable<UsuarioDto>> GetAllUsersAsync();
    Task<UsuarioDto> UpdateUserAsync(int id, UpdateUsuarioDto updateUsuarioDto);
    Task DeleteUserAsync(int id);
    Task<bool> ValidateTokenAsync(string token);
}
