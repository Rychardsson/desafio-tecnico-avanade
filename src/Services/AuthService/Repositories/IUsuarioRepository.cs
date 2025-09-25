using Shared.Models;

namespace AuthService.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task<Usuario?> GetByIdAsync(int id);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<Usuario> AddAsync(Usuario usuario);
    Task<Usuario> UpdateAsync(Usuario usuario);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(string email);
}
