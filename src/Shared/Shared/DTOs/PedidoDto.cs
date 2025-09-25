using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace Shared.DTOs
{
    public class PedidoCreateDto
    {
        [Required]
        public List<ItemPedidoDto> Itens { get; set; } = new List<ItemPedidoDto>();
        
        [MaxLength(500)]
        public string? Observacoes { get; set; }
    }
    
    public class ItemPedidoDto
    {
        [Required]
        public int ProdutoId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }
    }
    
    public class PedidoResponseDto
    {
        public int Id { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public List<ItemPedidoResponseDto> Itens { get; set; } = new List<ItemPedidoResponseDto>();
        public decimal ValorTotal { get; set; }
        public StatusPedido Status { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public string? Observacoes { get; set; }
    }
    
    public class ItemPedidoResponseDto
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}