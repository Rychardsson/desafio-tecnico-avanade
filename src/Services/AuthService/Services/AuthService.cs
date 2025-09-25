using AutoMapper;
using AuthService.Repositories;
using Shared.DTOs;
using Shared.Models;
using Shared.Services;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IJwtService jwtService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _jwtService = jwtService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<string> AuthenticateAsync(LoginDto loginDto)
    {
        _logger.LogInformation("Tentativa de autenticação para o email: {Email}", loginDto.Email);

        var usuario = await _usuarioRepository.GetByEmailAsync(loginDto.Email);
        if (usuario == null)
        {
            _logger.LogWarning("Usuário não encontrado para o email: {Email}", loginDto.Email);
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Senha, usuario.Senha))
        {
            _logger.LogWarning("Senha incorreta para o usuário: {Email}", loginDto.Email);
            throw new UnauthorizedAccessException("Credenciais inválidas");
        }

        var token = _jwtService.GenerateToken(usuario);
        _logger.LogInformation("Autenticação bem-sucedida para o usuário: {Email}", loginDto.Email);

        return token;
    }

    public async Task<UsuarioDto> RegisterAsync(CreateUsuarioDto createUsuarioDto)
    {
        _logger.LogInformation("Criando novo usuário: {Email}", createUsuarioDto.Email);

        if (await _usuarioRepository.ExistsAsync(createUsuarioDto.Email))
        {
            _logger.LogWarning("Tentativa de criar usuário com email já existente: {Email}", createUsuarioDto.Email);
            throw new InvalidOperationException("Email já está em uso");
        }

        var usuario = new Usuario
        {
            Nome = createUsuarioDto.Nome,
            Email = createUsuarioDto.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword(createUsuarioDto.Senha),
            Role = createUsuarioDto.Role ?? "User",
            CriadoEm = DateTime.UtcNow
        };

        var usuarioCriado = await _usuarioRepository.AddAsync(usuario);
        _logger.LogInformation("Usuário criado com sucesso: {Email}", usuarioCriado.Email);

        return _mapper.Map<UsuarioDto>(usuarioCriado);
    }

    public async Task<UsuarioDto?> GetUserByIdAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto?> GetUserByEmailAsync(string email)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);
        return usuario == null ? null : _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<IEnumerable<UsuarioDto>> GetAllUsersAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
    }

    public async Task<UsuarioDto> UpdateUserAsync(int id, UpdateUsuarioDto updateUsuarioDto)
    {
        _logger.LogInformation("Atualizando usuário: {Id}", id);

        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        // Verificar se o email já está em uso por outro usuário
        if (!string.IsNullOrEmpty(updateUsuarioDto.Email) && updateUsuarioDto.Email != usuario.Email)
        {
            if (await _usuarioRepository.ExistsAsync(updateUsuarioDto.Email))
            {
                throw new InvalidOperationException("Email já está em uso");
            }
            usuario.Email = updateUsuarioDto.Email;
        }

        if (!string.IsNullOrEmpty(updateUsuarioDto.Nome))
            usuario.Nome = updateUsuarioDto.Nome;

        if (!string.IsNullOrEmpty(updateUsuarioDto.Senha))
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(updateUsuarioDto.Senha);

        if (!string.IsNullOrEmpty(updateUsuarioDto.Role))
            usuario.Role = updateUsuarioDto.Role;

        var usuarioAtualizado = await _usuarioRepository.UpdateAsync(usuario);
        _logger.LogInformation("Usuário atualizado com sucesso: {Id}", id);

        return _mapper.Map<UsuarioDto>(usuarioAtualizado);
    }

    public async Task DeleteUserAsync(int id)
    {
        _logger.LogInformation("Deletando usuário: {Id}", id);

        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        await _usuarioRepository.DeleteAsync(id);
        _logger.LogInformation("Usuário deletado com sucesso: {Id}", id);
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var principal = _jwtService.ValidateToken(token);
            return Task.FromResult(principal != null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token inválido: {Token}", token);
            return Task.FromResult(false);
        }
    }
}
