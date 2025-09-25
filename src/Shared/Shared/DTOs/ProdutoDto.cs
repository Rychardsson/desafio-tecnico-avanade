using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs
{
    public class ProdutoCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string Nome { get; set; } = string.Empty;
        
        [MaxLength(1000)]
        public string Descricao { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal Preco { get; set; }
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int QuantidadeEstoque { get; set; }
    }
    
    public class ProdutoUpdateDto
    {
        [MaxLength(200)]
        public string? Nome { get; set; }
        
        [MaxLength(1000)]
        public string? Descricao { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "O preço deve ser maior que zero")]
        public decimal? Preco { get; set; }
        
        [Range(0, int.MaxValue, ErrorMessage = "A quantidade deve ser maior ou igual a zero")]
        public int? QuantidadeEstoque { get; set; }
        
        public bool? Ativo { get; set; }
    }
    
    public class ProdutoResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int QuantidadeEstoque { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? DataAtualizacao { get; set; }
        public bool Ativo { get; set; }
    }
}
