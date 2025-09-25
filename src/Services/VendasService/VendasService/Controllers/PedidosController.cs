using VendasService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.DTOs;
using Shared.Models;
using System.Security.Claims;

namespace VendasService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PedidosController : ControllerBase
    {
        private readonly IPedidoService _pedidoService;
        private readonly ILogger<PedidosController> _logger;
        
        public PedidosController(IPedidoService pedidoService, ILogger<PedidosController> logger)
        {
            _pedidoService = pedidoService;
            _logger = logger;
        }
        
        /// <summary>
        /// Obtém todos os pedidos (apenas para Admin e Vendedor)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> GetPedidos()
        {
            var resultado = await _pedidoService.GetAllPedidosAsync();
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém pedidos do cliente logado
        /// </summary>
        [HttpGet("meus-pedidos")]
        public async Task<IActionResult> GetMeusPedidos()
        {
            var clienteId = GetCurrentUserId();
            if (string.IsNullOrEmpty(clienteId))
                return Unauthorized("Cliente não identificado");
                
            var resultado = await _pedidoService.GetPedidosByClienteAsync(clienteId);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém pedidos por cliente (apenas para Admin e Vendedor)
        /// </summary>
        [HttpGet("cliente/{clienteId}")]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> GetPedidosByCliente(string clienteId)
        {
            var resultado = await _pedidoService.GetPedidosByClienteAsync(clienteId);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém um pedido por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPedido(int id)
        {
            var resultado = await _pedidoService.GetPedidoByIdAsync(id);
            
            if (!resultado.IsSuccess)
                return NotFound(resultado);
            
            // Verificar se o usuário pode acessar este pedido
            if (!CanAccessPedido(resultado.Data?.ClienteId))
                return Forbid("Acesso negado a este pedido");
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Cria um novo pedido
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePedido([FromBody] PedidoCreateDto pedidoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var clienteId = GetCurrentUserId();
            if (string.IsNullOrEmpty(clienteId))
                return Unauthorized("Cliente não identificado");
                
            var resultado = await _pedidoService.CreatePedidoAsync(pedidoDto, clienteId);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return CreatedAtAction(
                nameof(GetPedido), 
                new { id = resultado.Data?.Id }, 
                resultado);
        }
        
        /// <summary>
        /// Atualiza o status de um pedido
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> UpdateStatusPedido(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var resultado = await _pedidoService.UpdateStatusPedidoAsync(id, dto.Status, dto.Motivo);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Cancela um pedido
        /// </summary>
        [HttpPost("{id}/cancelar")]
        public async Task<IActionResult> CancelarPedido(int id, [FromBody] CancelarPedidoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            // Verificar se o usuário pode cancelar este pedido
            var pedidoResult = await _pedidoService.GetPedidoByIdAsync(id);
            if (!pedidoResult.IsSuccess)
                return NotFound(pedidoResult);
                
            if (!CanAccessPedido(pedidoResult.Data?.ClienteId))
                return Forbid("Acesso negado para cancelar este pedido");
                
            var resultado = await _pedidoService.CancelarPedidoAsync(id, dto.Motivo);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém pedidos por status
        /// </summary>
        [HttpGet("status/{status}")]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> GetPedidosByStatus(StatusPedido status)
        {
            var resultado = await _pedidoService.GetPedidosByStatusAsync(status);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém total de vendas por período
        /// </summary>
        [HttpGet("relatorios/vendas")]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> GetTotalVendasPorPeriodo([FromQuery] DateTime dataInicio, [FromQuery] DateTime dataFim)
        {
            if (dataInicio > dataFim)
                return BadRequest("Data início deve ser menor que data fim");
                
            var resultado = await _pedidoService.GetTotalVendasPorPeriodoAsync(dataInicio, dataFim);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém pedidos recentes
        /// </summary>
        [HttpGet("recentes")]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> GetPedidosRecentes([FromQuery] int limit = 10)
        {
            if (limit <= 0 || limit > 100)
                return BadRequest("Limite deve estar entre 1 e 100");
                
            var resultado = await _pedidoService.GetPedidosRecentesAsync(limit);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Endpoint para health check
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(new { Status = "Healthy", Service = "VendasService", Timestamp = DateTime.UtcNow });
        }
        
        private string GetCurrentUserId()
        {
            return User.FindFirst("userId")?.Value ?? 
                   User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                   string.Empty;
        }
        
        private bool CanAccessPedido(string? pedidoClienteId)
        {
            var currentUserId = GetCurrentUserId();
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            
            // Admin e Vendedor podem acessar qualquer pedido
            if (userRoles.Contains(Roles.ADMIN) || userRoles.Contains(Roles.VENDEDOR))
                return true;
                
            // Cliente só pode acessar seus próprios pedidos
            return currentUserId == pedidoClienteId;
        }
    }
    
    public class UpdateStatusDto
    {
        public StatusPedido Status { get; set; }
        public string? Motivo { get; set; }
    }
    
    public class CancelarPedidoDto
    {
        public string Motivo { get; set; } = "Cancelado pelo cliente";
    }
}
