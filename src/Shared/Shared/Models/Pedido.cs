using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        
        [Required]
        public string ClienteId { get; set; } = string.Empty;
        
        [Required]
        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
        
        public decimal ValorTotal { get; set; }
        
        public StatusPedido Status { get; set; } = StatusPedido.Pendente;
        
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }
        
        [MaxLength(500)]
        public string? Observacoes { get; set; }
    }
    
    public class ItemPedido
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string NomeProduto { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
        public int Quantidade { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O preço unitário deve ser maior que zero")]
        public decimal PrecoUnitario { get; set; }
        
        public decimal SubTotal => Quantidade * PrecoUnitario;
    }
    
    public enum StatusPedido
    {
        Pendente = 0,
        Confirmado = 1,
        Processando = 2,
        Enviado = 3,
        Entregue = 4,
        Cancelado = 5
    }
}