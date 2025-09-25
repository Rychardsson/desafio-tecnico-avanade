using EstoqueService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Constants;
using Shared.DTOs;
using System.Security.Claims;

namespace EstoqueService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtoService;
        private readonly ILogger<ProdutosController> _logger;
        
        public ProdutosController(IProdutoService produtoService, ILogger<ProdutosController> logger)
        {
            _produtoService = produtoService;
            _logger = logger;
        }
        
        /// <summary>
        /// Obtém todos os produtos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProdutos()
        {
            var resultado = await _produtoService.GetAllProdutosAsync();
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém um produto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduto(int id)
        {
            var resultado = await _produtoService.GetProdutoByIdAsync(id);
            
            if (!resultado.IsSuccess)
                return NotFound(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Cria um novo produto
        /// </summary>
        [HttpPost]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> CreateProduto([FromBody] ProdutoCreateDto produtoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var resultado = await _produtoService.CreateProdutoAsync(produtoDto);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return CreatedAtAction(
                nameof(GetProduto), 
                new { id = resultado.Data?.Id }, 
                resultado);
        }
        
        /// <summary>
        /// Atualiza um produto existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> UpdateProduto(int id, [FromBody] ProdutoUpdateDto produtoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            var resultado = await _produtoService.UpdateProdutoAsync(id, produtoDto);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Exclui um produto (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.ADMIN)]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var resultado = await _produtoService.DeleteProdutoAsync(id);
            
            if (!resultado.IsSuccess)
                return NotFound(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Obtém produtos com estoque disponível
        /// </summary>
        [HttpGet("com-estoque")]
        public async Task<IActionResult> GetProdutosComEstoque()
        {
            var resultado = await _produtoService.GetProdutosComEstoqueAsync();
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Valida se há estoque suficiente para um produto
        /// </summary>
        [HttpGet("{id}/validar-estoque/{quantidade}")]
        public async Task<IActionResult> ValidarEstoque(int id, int quantidade)
        {
            if (quantidade <= 0)
                return BadRequest("Quantidade deve ser maior que zero");
                
            var resultado = await _produtoService.ValidarEstoqueAsync(id, quantidade);
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Atualiza o estoque de um produto (reduz)
        /// </summary>
        [HttpPost("{id}/atualizar-estoque")]
        [Authorize(Roles = $"{Roles.ADMIN},{Roles.VENDEDOR}")]
        public async Task<IActionResult> AtualizarEstoque(int id, [FromBody] AtualizarEstoqueDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
                
            if (dto.Quantidade <= 0)
                return BadRequest("Quantidade deve ser maior que zero");
                
            var resultado = await _produtoService.AtualizarEstoqueAsync(id, dto.Quantidade, dto.Motivo ?? "Atualização manual");
            
            if (!resultado.IsSuccess)
                return BadRequest(resultado);
                
            return Ok(resultado);
        }
        
        /// <summary>
        /// Pesquisa produtos por nome ou descrição
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProdutos([FromQuery] string searchTerm)
        {
            var resultado = await _produtoService.SearchProdutosAsync(searchTerm);
            
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
            return Ok(new { Status = "Healthy", Service = "EstoqueService", Timestamp = DateTime.UtcNow });
        }
    }
    
    public class AtualizarEstoqueDto
    {
        public int Quantidade { get; set; }
        public string? Motivo { get; set; }
    }
}
