namespace Shared.Messages
{
    public class EstoqueAtualizadoMessage
    {
        public int ProdutoId { get; set; }
        public int QuantidadeAnterior { get; set; }
        public int QuantidadeAtual { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public string UsuarioId { get; set; } = string.Empty;
    }
    
    public class PedidoCriadoMessage
    {
        public int PedidoId { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public List<ItemPedidoMessage> Itens { get; set; } = new List<ItemPedidoMessage>();
        public decimal ValorTotal { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
    
    public class ItemPedidoMessage
    {
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
    
    public class PedidoStatusAtualizadoMessage
    {
        public int PedidoId { get; set; }
        public string StatusAnterior { get; set; } = string.Empty;
        public string StatusAtual { get; set; } = string.Empty;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public string? Motivo { get; set; }
    }
    
    public class EstoqueInsuficienteMessage
    {
        public int PedidoId { get; set; }
        public int ProdutoId { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public int QuantidadeSolicitada { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public DateTime DataOcorrencia { get; set; } = DateTime.UtcNow;
    }
}