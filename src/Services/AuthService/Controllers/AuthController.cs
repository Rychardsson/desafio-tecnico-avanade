using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AuthService.Services;
using Shared.DTOs;
using Shared.Helpers;
using Shared.Constants;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Realizar login do usuário
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<TokenResponseDto>>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            var token = await _authService.AuthenticateAsync(loginDto);
            var user = await _authService.GetUserByEmailAsync(loginDto.Email);

            var response = new TokenResponseDto
            {
                Token = token,
                Usuario = user!,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            return Ok(ApiResponse<TokenResponseDto>.Success(response, "Login realizado com sucesso"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Tentativa de login inválida para {Email}", loginDto.Email);
            return Unauthorized(ApiResponse<TokenResponseDto>.Error("Credenciais inválidas"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login para {Email}", loginDto.Email);
            return StatusCode(500, ApiResponse<TokenResponseDto>.Error("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Registrar novo usuário
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> Register([FromBody] CreateUsuarioDto createUsuarioDto)
    {
        try
        {
            var usuario = await _authService.RegisterAsync(createUsuarioDto);
            return CreatedAtAction(nameof(GetUser), new { id = usuario.Id }, 
                ApiResponse<UsuarioDto>.Success(usuario, "Usuário criado com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Tentativa de registro com email já existente: {Email}", createUsuarioDto.Email);
            return BadRequest(ApiResponse<UsuarioDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário {Email}", createUsuarioDto.Email);
            return StatusCode(500, ApiResponse<UsuarioDto>.Error("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Validar token JWT
    /// </summary>
    [HttpPost("validate-token")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateToken([FromBody] string token)
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync(token);
            return Ok(ApiResponse<bool>.Success(isValid, isValid ? "Token válido" : "Token inválido"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar token");
            return StatusCode(500, ApiResponse<bool>.Error("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Obter usuário por ID (requer autenticação)
    /// </summary>
    [HttpGet("users/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetUser(int id)
    {
        try
        {
            var usuario = await _authService.GetUserByIdAsync(id);
            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioDto>.Error("Usuário não encontrado"));
            }

            return Ok(ApiResponse<UsuarioDto>.Success(usuario));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuário {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioDto>.Error("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Listar todos os usuários (requer permissão de Admin)
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = Roles.ADMIN)]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDto>>>> GetAllUsers()
    {
        try
        {
            var usuarios = await _authService.GetAllUsersAsync();
            return Ok(ApiResponse<IEnumerable<UsuarioDto>>.Success(usuarios));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return StatusCode(500, ApiResponse<IEnumerable<UsuarioDto>>.Error("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Atualizar usuário (requer autenticação)
    /// </summary>
    [HttpPut("users/{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UsuarioDto>>> UpdateUser(int id, [FromBody] UpdateUsuarioDto updateUsuarioDto)
    {
        try
        {
            var usuario = await _authService.UpdateUserAsync(id, updateUsuarioDto);
            return Ok(ApiResponse<UsuarioDto>.Success(usuario, "Usuário atualizado com sucesso"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<UsuarioDto>.Error("Usuário não encontrado"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<UsuarioDto>.Error(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioDto>.Error("Erro interno do servidor"));
        }
    }

    /// <summary>
    /// Deletar usuário (requer permissão de Admin)
    /// </summary>
    [HttpDelete("users/{id}")]
    [Authorize(Roles = Roles.ADMIN)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteUser(int id)
    {
        try
        {
            await _authService.DeleteUserAsync(id);
            return Ok(ApiResponse<object?>.Success(null, "Usuário deletado com sucesso"));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse<object>.Error("Usuário não encontrado"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar usuário {Id}", id);
            return StatusCode(500, ApiResponse<object>.Error("Erro interno do servidor"));
        }
    }
}