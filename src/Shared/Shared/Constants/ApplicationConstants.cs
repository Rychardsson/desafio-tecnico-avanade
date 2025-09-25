namespace Shared.Constants
{
    public static class QueueNames
    {
        public const string PEDIDO_CRIADO = "pedido.criado";
        public const string PEDIDO_CONFIRMADO = "pedido.confirmado";
        public const string PEDIDO_CANCELADO = "pedido.cancelado";
        public const string PEDIDO_STATUS_ATUALIZADO = "pedido.status.atualizado";
        public const string ESTOQUE_ATUALIZADO = "estoque.atualizado";
        public const string ESTOQUE_REDUZIDO = "estoque.reduzido";
        public const string ESTOQUE_INSUFICIENTE = "estoque.insuficiente";
        public const string ESTOQUE_VALIDACAO_REQUEST = "estoque.validacao.request";
        public const string ESTOQUE_VALIDACAO_RESPONSE = "estoque.validacao.response";
        public const string PRODUTO_CRIADO = "produto.criado";
        public const string PRODUTO_ATUALIZADO = "produto.atualizado";
    }
    
    public static class ApiRoutes
    {
        public const string ESTOQUE_SERVICE = "api/estoque";
        public const string VENDAS_SERVICE = "api/vendas";
        public const string AUTH_SERVICE = "api/auth";
    }
    
    public static class Roles
    {
        public const string ADMIN = "Admin";
        public const string CLIENTE = "Cliente";
        public const string VENDEDOR = "Vendedor";
    }
}
