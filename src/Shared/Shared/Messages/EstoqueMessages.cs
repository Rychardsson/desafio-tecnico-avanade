namespace Shared.Messages
{
    public class EstoqueValidacaoRequest
    {
        public int PedidoId { get; set; }
        public List<ItemValidacao> Itens { get; set; } = new List<ItemValidacao>();
        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
    }
    
    public class ItemValidacao
    {
        public int ProdutoId { get; set; }
        public int QuantidadeSolicitada { get; set; }
    }
    
    public class EstoqueValidacaoResponse
    {
        public int PedidoId { get; set; }
        public bool EstoqueDisponivel { get; set; }
        public List<ItemValidacaoResult> ResultadoItens { get; set; } = new List<ItemValidacaoResult>();
        public string? MensagemErro { get; set; }
        public DateTime DataResposta { get; set; } = DateTime.UtcNow;
    }
    
    public class ItemValidacaoResult
    {
        public int ProdutoId { get; set; }
        public bool Disponivel { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public int QuantidadeSolicitada { get; set; }
    }
    
    public class EstoqueReduzidoMessage
    {
        public int PedidoId { get; set; }
        public List<ItemEstoqueReduzido> Itens { get; set; } = new List<ItemEstoqueReduzido>();
        public DateTime DataReducao { get; set; } = DateTime.UtcNow;
        public string Motivo { get; set; } = "Venda confirmada";
    }
    
    public class ItemEstoqueReduzido
    {
        public int ProdutoId { get; set; }
        public int QuantidadeReduzida { get; set; }
    }
    
    public class PedidoCriadoMessage
    {
        public int PedidoId { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public List<ItemPedidoMessage> Itens { get; set; } = new List<ItemPedidoMessage>();
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }
    
    public class ItemPedidoMessage
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }
    
    public class PedidoConfirmadoMessage
    {
        public int PedidoId { get; set; }
        public string ClienteId { get; set; } = string.Empty;
        public DateTime DataConfirmacao { get; set; } = DateTime.UtcNow;
        public List<ItemEstoqueReduzido> ItensParaReduzir { get; set; } = new List<ItemEstoqueReduzido>();
    }
    
    public class EstoqueAtualizadoMessage
    {
        public int ProdutoId { get; set; }
        public int QuantidadeAnterior { get; set; }
        public int QuantidadeAtual { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public string UsuarioId { get; set; } = string.Empty;
    }
    
    public class EstoqueInsuficienteMessage
    {
        public int ProdutoId { get; set; }
        public int QuantidadeSolicitada { get; set; }
        public int QuantidadeDisponivel { get; set; }
        public DateTime DataOcorrencia { get; set; } = DateTime.UtcNow;
        public string? PedidoId { get; set; }
    }
    
    public class PedidoStatusAtualizadoMessage
    {
        public int PedidoId { get; set; }
        public string StatusAnterior { get; set; } = string.Empty;
        public string StatusAtual { get; set; } = string.Empty;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
        public string UsuarioId { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
    }
}
