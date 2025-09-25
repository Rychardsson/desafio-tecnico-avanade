using AutoMapper;
using EstoqueService.Repositories;
using Microsoft.Extensions.Logging;
using Shared.DTOs;
using Shared.Helpers;
using Shared.Messages;
using Shared.Models;
using Shared.Services;
using Shared.Constants;

namespace EstoqueService.Services
{
    public interface IProdutoService
    {
        Task<ApiResponse<IEnumerable<ProdutoResponseDto>>> GetAllProdutosAsync();
        Task<ApiResponse<ProdutoResponseDto>> GetProdutoByIdAsync(int id);
        Task<ApiResponse<ProdutoResponseDto>> CreateProdutoAsync(ProdutoCreateDto produtoDto);
        Task<ApiResponse<ProdutoResponseDto>> UpdateProdutoAsync(int id, ProdutoUpdateDto produtoDto);
        Task<ApiResponse<bool>> DeleteProdutoAsync(int id);
        Task<ApiResponse<IEnumerable<ProdutoResponseDto>>> GetProdutosComEstoqueAsync();
        Task<ApiResponse<bool>> ValidarEstoqueAsync(int produtoId, int quantidade);
        Task<ApiResponse<bool>> AtualizarEstoqueAsync(int produtoId, int quantidade, string motivo = "Venda");
        Task<ApiResponse<IEnumerable<ProdutoResponseDto>>> SearchProdutosAsync(string searchTerm);
    }
    
    public class ProdutoService : IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IMapper _mapper;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly ILogger<ProdutoService> _logger;
        
        public ProdutoService(
            IProdutoRepository produtoRepository,
            IMapper mapper,
            IRabbitMQService rabbitMQService,
            ILogger<ProdutoService> logger)
        {
            _produtoRepository = produtoRepository;
            _mapper = mapper;
            _rabbitMQService = rabbitMQService;
            _logger = logger;
        }
        
        public async Task<ApiResponse<IEnumerable<ProdutoResponseDto>>> GetAllProdutosAsync()
        {
            try
            {
                var produtos = await _produtoRepository.GetAllAsync();
                var produtosDto = _mapper.Map<IEnumerable<ProdutoResponseDto>>(produtos);
                
                return ApiResponse<IEnumerable<ProdutoResponseDto>>.SuccessResult(produtosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar todos os produtos");
                return ApiResponse<IEnumerable<ProdutoResponseDto>>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<ProdutoResponseDto>> GetProdutoByIdAsync(int id)
        {
            try
            {
                var produto = await _produtoRepository.GetByIdAsync(id);
                if (produto == null)
                {
                    return ApiResponse<ProdutoResponseDto>.FailureResult("Produto não encontrado");
                }
                
                var produtoDto = _mapper.Map<ProdutoResponseDto>(produto);
                return ApiResponse<ProdutoResponseDto>.SuccessResult(produtoDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produto com ID {ProdutoId}", id);
                return ApiResponse<ProdutoResponseDto>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<ProdutoResponseDto>> CreateProdutoAsync(ProdutoCreateDto produtoDto)
        {
            try
            {
                var produto = _mapper.Map<Produto>(produtoDto);
                var produtoCriado = await _produtoRepository.CreateAsync(produto);
                
                // Publicar mensagem de produto criado
                var mensagem = new EstoqueAtualizadoMessage
                {
                    ProdutoId = produtoCriado.Id,
                    QuantidadeAnterior = 0,
                    QuantidadeAtual = produtoCriado.QuantidadeEstoque,
                    Motivo = "Produto criado",
                    DataAtualizacao = produtoCriado.DataCriacao
                };
                
                await _rabbitMQService.PublishMessageAsync(QueueNames.PRODUTO_CRIADO, mensagem);
                
                var produtoResponseDto = _mapper.Map<ProdutoResponseDto>(produtoCriado);
                return ApiResponse<ProdutoResponseDto>.SuccessResult(
                    produtoResponseDto, 
                    "Produto criado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar produto");
                return ApiResponse<ProdutoResponseDto>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<ProdutoResponseDto>> UpdateProdutoAsync(int id, ProdutoUpdateDto produtoDto)
        {
            try
            {
                var produtoExistente = await _produtoRepository.GetByIdAsync(id);
                if (produtoExistente == null)
                {
                    return ApiResponse<ProdutoResponseDto>.FailureResult("Produto não encontrado");
                }
                
                var quantidadeAnterior = produtoExistente.QuantidadeEstoque;
                
                // Aplicar atualizações apenas nos campos fornecidos
                if (!string.IsNullOrEmpty(produtoDto.Nome))
                    produtoExistente.Nome = produtoDto.Nome;
                    
                if (!string.IsNullOrEmpty(produtoDto.Descricao))
                    produtoExistente.Descricao = produtoDto.Descricao;
                    
                if (produtoDto.Preco.HasValue)
                    produtoExistente.Preco = produtoDto.Preco.Value;
                    
                if (produtoDto.QuantidadeEstoque.HasValue)
                    produtoExistente.QuantidadeEstoque = produtoDto.QuantidadeEstoque.Value;
                    
                if (produtoDto.Ativo.HasValue)
                    produtoExistente.Ativo = produtoDto.Ativo.Value;
                
                var produtoAtualizado = await _produtoRepository.UpdateAsync(produtoExistente);
                
                // Se a quantidade em estoque mudou, publicar mensagem
                if (quantidadeAnterior != produtoAtualizado.QuantidadeEstoque)
                {
                    var mensagem = new EstoqueAtualizadoMessage
                    {
                        ProdutoId = produtoAtualizado.Id,
                        QuantidadeAnterior = quantidadeAnterior,
                        QuantidadeAtual = produtoAtualizado.QuantidadeEstoque,
                        Motivo = "Produto atualizado",
                        DataAtualizacao = produtoAtualizado.DataAtualizacao ?? DateTime.UtcNow
                    };
                    
                    await _rabbitMQService.PublishMessageAsync(QueueNames.PRODUTO_ATUALIZADO, mensagem);
                }
                
                var produtoResponseDto = _mapper.Map<ProdutoResponseDto>(produtoAtualizado);
                return ApiResponse<ProdutoResponseDto>.SuccessResult(
                    produtoResponseDto, 
                    "Produto atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar produto com ID {ProdutoId}", id);
                return ApiResponse<ProdutoResponseDto>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<bool>> DeleteProdutoAsync(int id)
        {
            try
            {
                var sucesso = await _produtoRepository.DeleteAsync(id);
                if (!sucesso)
                {
                    return ApiResponse<bool>.FailureResult("Produto não encontrado");
                }
                
                return ApiResponse<bool>.SuccessResult(true, "Produto excluído com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir produto com ID {ProdutoId}", id);
                return ApiResponse<bool>.FailureResult("Erro interno do servidor", ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<ProdutoResponseDto>>> GetProdutosComEstoqueAsync()
        {
            try
            {
                var produtos = await _produtoRepository.GetProdutosComEstoqueAsync();
                var produtosDto = _mapper.Map<IEnumerable<ProdutoResponseDto>>(produtos);
                
                return ApiResponse<IEnumerable<ProdutoResponseDto>>.SuccessResult(produtosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos com estoque");
                return ApiResponse<IEnumerable<ProdutoResponseDto>>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
        
        public async Task<ApiResponse<bool>> ValidarEstoqueAsync(int produtoId, int quantidade)
        {
            try
            {
                var temEstoque = await _produtoRepository.TemEstoqueSuficienteAsync(produtoId, quantidade);
                return ApiResponse<bool>.SuccessResult(temEstoque);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar estoque do produto {ProdutoId}", produtoId);
                return ApiResponse<bool>.FailureResult("Erro interno do servidor", ex.Message);
            }
        }
        
        public async Task<ApiResponse<bool>> AtualizarEstoqueAsync(int produtoId, int quantidade, string motivo = "Venda")
        {
            try
            {
                var produto = await _produtoRepository.GetByIdAsync(produtoId);
                if (produto == null)
                {
                    return ApiResponse<bool>.FailureResult("Produto não encontrado");
                }
                
                var quantidadeAnterior = produto.QuantidadeEstoque;
                
                var sucesso = await _produtoRepository.AtualizarEstoqueAsync(produtoId, quantidade);
                if (!sucesso)
                {
                    // Publicar mensagem de estoque insuficiente
                    var mensagemEstoqueInsuficiente = new EstoqueInsuficienteMessage
                    {
                        ProdutoId = produtoId,
                        NomeProduto = produto.Nome,
                        QuantidadeSolicitada = quantidade,
                        QuantidadeDisponivel = produto.QuantidadeEstoque
                    };
                    
                    await _rabbitMQService.PublishMessageAsync(QueueNames.ESTOQUE_INSUFICIENTE, mensagemEstoqueInsuficiente);
                    
                    return ApiResponse<bool>.FailureResult("Estoque insuficiente");
                }
                
                // Publicar mensagem de estoque atualizado
                var mensagem = new EstoqueAtualizadoMessage
                {
                    ProdutoId = produtoId,
                    QuantidadeAnterior = quantidadeAnterior,
                    QuantidadeAtual = quantidadeAnterior - quantidade,
                    Motivo = motivo,
                    DataAtualizacao = DateTime.UtcNow
                };
                
                await _rabbitMQService.PublishMessageAsync(QueueNames.ESTOQUE_ATUALIZADO, mensagem);
                
                return ApiResponse<bool>.SuccessResult(true, "Estoque atualizado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar estoque do produto {ProdutoId}", produtoId);
                return ApiResponse<bool>.FailureResult("Erro interno do servidor", ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<ProdutoResponseDto>>> SearchProdutosAsync(string searchTerm)
        {
            try
            {
                var produtos = await _produtoRepository.SearchAsync(searchTerm);
                var produtosDto = _mapper.Map<IEnumerable<ProdutoResponseDto>>(produtos);
                
                return ApiResponse<IEnumerable<ProdutoResponseDto>>.SuccessResult(produtosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao pesquisar produtos com termo: {SearchTerm}", searchTerm);
                return ApiResponse<IEnumerable<ProdutoResponseDto>>.FailureResult(
                    "Erro interno do servidor", 
                    ex.Message);
            }
        }
    }
}