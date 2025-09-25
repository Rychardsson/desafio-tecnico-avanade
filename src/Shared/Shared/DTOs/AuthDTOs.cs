namespace Shared.DTOs;

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class CreateUsuarioDto
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string? Role { get; set; }
}

public class UpdateUsuarioDto
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Senha { get; set; }
    public string? Role { get; set; }
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}

public class TokenResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UsuarioDto Usuario { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}