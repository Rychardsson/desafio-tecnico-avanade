using AutoMapper;
using VendasService.Repositories;
using VendasService.Services;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using Shared.Helpers;
using Shared.Messages;
using Shared.Models;
using Shared.Services;
using Shared.Constants;
using System.Security.Claims;

namespace VendasService.Services
{
    public interface IPedidoService
    {
        Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetAllPedidosAsync();
        Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetPedidosByClienteAsync(string clienteId);
        Task<ApiResponse<PedidoResponseDto>> GetPedidoByIdAsync(int id);
        Task<ApiResponse<PedidoResponseDto>> CreatePedidoAsync(PedidoCreateDto pedidoDto, string clienteId);
        Task<ApiResponse<PedidoResponseDto>> UpdateStatusPedidoAsync(int id, StatusPedido novoStatus, string? motivo = null);
        Task<ApiResponse<bool>> CancelarPedidoAsync(int id, string motivo);
        Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetPedidosByStatusAsync(StatusPedido status);
        Task<ApiResponse<decimal>> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetPedidosRecentesAsync(int limit = 10);
    }
    
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IEstoqueServiceClient _estoqueServiceClient;
        private readonly IMapper _mapper;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<PedidoService> _logger;
        
        public PedidoService(
            IPedidoRepository pedidoRepository,
            IEstoqueServiceClient estoqueServiceClient,
            IMapper mapper,
            IRabbitMQService rabbitMQService,
            ILogger<PedidoService> logger)
        {
            _pedidoRepository = pedidoRepository;
            _estoqueServiceClient = estoqueServiceClient;
            _mapper = mapper;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }
        
        public async Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetAllPedidosAsync()
        {
            try
            {
                var pedidos = await _pedidoRepository.GetAllAsync();
                var pedidosDto = _mapper.Map<IEnumerable<PedidoResponseDto>>(pedidos);
                
                return ApiResponse<IEnumerable<PedidoResponseDto>>.SuccessResult(pedidosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os pedidos");
                return ApiResponse<IEnumerable<PedidoResponseDto>>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetPedidosByClienteAsync(string clienteId)
        {
            try
            {
                var pedidos = await _pedidoRepository.GetByClienteIdAsync(clienteId);
                var pedidosDto = _mapper.Map<IEnumerable<PedidoResponseDto>>(pedidos);
                
                return ApiResponse<IEnumerable<PedidoResponseDto>>.SuccessResult(pedidosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos do cliente {ClienteId}", clienteId);
                return ApiResponse<IEnumerable<PedidoResponseDto>>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<PedidoResponseDto>> GetPedidoByIdAsync(int id)
        {
            try
            {
                var pedido = await _pedidoRepository.GetByIdAsync(id);
                if (pedido == null)
                {
                    return ApiResponse<PedidoResponseDto>.FailureResult("Pedido não encontrado");
                }
                
                var pedidoDto = _mapper.Map<PedidoResponseDto>(pedido);
                return ApiResponse<PedidoResponseDto>.SuccessResult(pedidoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedido com ID {PedidoId}", id);
                return ApiResponse<PedidoResponseDto>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<PedidoResponseDto>> CreatePedidoAsync(PedidoCreateDto pedidoDto, string clienteId)
        {
            try
            {
                if (!pedidoDto.Itens.Any())
                {
                    return ApiResponse<PedidoResponseDto>.FailureResult("O pedido deve conter pelo menos um item");
                }
                
                var pedido = new Pedido
                {
                    ClienteId = clienteId,
                    Observacoes = pedidoDto.Observacoes,
                    Status = StatusPedido.Pendente,
                    Itens = new List<ItemPedido>()
                };
                
                decimal valorTotal = 0;
                var itensComErro = new List<string>();
                
                // Validar e processar cada item do pedido
                foreach (var itemDto in pedidoDto.Itens)
                {
                    // Buscar informações do produto no serviço de estoque
                    var produto = await _estoqueServiceClient.GetProdutoAsync(itemDto.ProdutoId);
                    if (produto == null)
                    {
                        itensComErro.Add($"Produto ID {itemDto.ProdutoId} não encontrado");
                        continue;
                    }
                    
                    // Validar estoque disponível
                    var temEstoque = await _estoqueServiceClient.ValidarEstoqueAsync(itemDto.ProdutoId, itemDto.Quantidade);
                    if (!temEstoque)
                    {
                        itensComErro.Add($"Estoque insuficiente para o produto {produto.Nome}");
                        continue;
                    }
                    
                    var itemPedido = new ItemPedido
                    {
                        ProdutoId = itemDto.ProdutoId,
                        NomeProduto = produto.Nome,
                        Quantidade = itemDto.Quantidade,
                        PrecoUnitario = produto.Preco
                    };
                    
                    pedido.Itens.Add(itemPedido);
                    valorTotal += itemPedido.SubTotal;
                }
                
                if (itensComErro.Any())
                {
                    return ApiResponse<PedidoResponseDto>.FailureResult(
                        "Erro ao processar itens do pedido", 
                        itensComErro);
                }
                
                if (!pedido.Itens.Any())
                {
                    return ApiResponse<PedidoResponseDto>.FailureResult("Nenhum item válido encontrado no pedido");
                }
                
                pedido.ValorTotal = valorTotal;
                
                // Atualizar estoque para cada item
                foreach (var item in pedido.Itens)
                {
                    var estoqueAtualizado = await _estoqueServiceClient.AtualizarEstoqueAsync(
                        item.ProdutoId, 
                        item.Quantidade, 
                        $"Venda - Pedido #{pedido.Id}");
                        
                    if (!estoqueAtualizado)
                    {
                        _logger.LogWarning("Falha ao atualizar estoque para produto {ProdutoId} na quantidade {Quantidade}", 
                            item.ProdutoId, item.Quantidade);
                    }
                }
                
                // Salvar o pedido
                var pedidoCriado = await _pedidoRepository.CreateAsync(pedido);
                
                // Atualizar status para confirmado
                pedidoCriado.Status = StatusPedido.Confirmado;
                pedidoCriado.DataAtualizacao = DateTime.UtcNow;
                await _pedidoRepository.UpdateAsync(pedidoCriado);
                
                // Publicar mensagem de pedido criado
                var mensagem = new PedidoCriadoMessage
                {
                    PedidoId = pedidoCriado.Id,
                    ClienteId = pedidoCriado.ClienteId,
                    ValorTotal = pedidoCriado.ValorTotal,
                    DataCriacao = pedidoCriado.DataCriacao,
                    Itens = pedidoCriado.Itens.Select(i => new ItemPedidoMessage
                    {
                        ProdutoId = i.ProdutoId,
                        Quantidade = i.Quantidade,
                        PrecoUnitario = i.PrecoUnitario
                    }).ToList()
                };
                
                await _rabbitMQService.PublishMessageAsync(QueueNames.PEDIDO_CRIADO, mensagem);
                
                var pedidoResponseDto = _mapper.Map<PedidoResponseDto>(pedidoCriado);
                return ApiResponse<PedidoResponseDto>.SuccessResult(
                    pedidoResponseDto, 
                    "Pedido criado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pedido para cliente {ClienteId}", clienteId);
                return ApiResponse<PedidoResponseDto>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<PedidoResponseDto>> UpdateStatusPedidoAsync(int id, StatusPedido novoStatus, string? motivo = null)
        {
            try
            {
                var pedido = await _pedidoRepository.GetByIdAsync(id);
                if (pedido == null)
                {
                    return ApiResponse<PedidoResponseDto>.FailureResult("Pedido não encontrado");
                }
                
                var statusAnterior = pedido.Status;
                pedido.Status = novoStatus;
                
                var pedidoAtualizado = await _pedidoRepository.UpdateAsync(pedido);
                
                // Publicar mensagem de status atualizado
                var mensagem = new PedidoStatusAtualizadoMessage
                {
                    PedidoId = pedidoAtualizado.Id,
                    StatusAnterior = statusAnterior.ToString(),
                    StatusAtual = novoStatus.ToString(),
                    DataAtualizacao = pedidoAtualizado.DataAtualizacao ?? DateTime.UtcNow,
                    Observacoes = motivo
                };
                
                await _rabbitMQService.PublishMessageAsync(QueueNames.PEDIDO_STATUS_ATUALIZADO, mensagem);
                
                var pedidoResponseDto = _mapper.Map<PedidoResponseDto>(pedidoAtualizado);
                return ApiResponse<PedidoResponseDto>.SuccessResult(
                    pedidoResponseDto, 
                    "Status do pedido atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido {PedidoId}", id);
                return ApiResponse<PedidoResponseDto>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<bool>> CancelarPedidoAsync(int id, string motivo)
        {
            try
            {
                var resultado = await UpdateStatusPedidoAsync(id, StatusPedido.Cancelado, motivo);
                
                if (resultado.IsSuccess)
                {
                    return ApiResponse<bool>.SuccessResult(true, "Pedido cancelado com sucesso");
                }
                
                return ApiResponse<bool>.FailureResult(resultado.Message, resultado.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pedido {PedidoId}", id);
                return ApiResponse<bool>.FailureResult("Erro interno do servidor", ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetPedidosByStatusAsync(StatusPedido status)
        {
            try
            {
                var pedidos = await _pedidoRepository.GetPedidosByStatusAsync(status);
                var pedidosDto = _mapper.Map<IEnumerable<PedidoResponseDto>>(pedidos);
                
                return ApiResponse<IEnumerable<PedidoResponseDto>>.SuccessResult(pedidosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos com status {Status}", status);
                return ApiResponse<IEnumerable<PedidoResponseDto>>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<decimal>> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            try
            {
                var total = await _pedidoRepository.GetTotalVendasPorPeriodoAsync(dataInicio, dataFim);
                return ApiResponse<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular total de vendas no período {DataInicio} - {DataFim}", 
                    dataInicio, dataFim);
                return ApiResponse<decimal>.FailureResult("Erro interno do servidor", ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<PedidoResponseDto>>> GetPedidosRecentesAsync(int limit = 10)
        {
            try
            {
                var pedidos = await _pedidoRepository.GetPedidosRecentesAsync(limit);
                var pedidosDto = _mapper.Map<IEnumerable<PedidoResponseDto>>(pedidos);
                
                return ApiResponse<IEnumerable<PedidoResponseDto>>.SuccessResult(pedidosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedidos recentes");
                return ApiResponse<IEnumerable<PedidoResponseDto>>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
    }
}
