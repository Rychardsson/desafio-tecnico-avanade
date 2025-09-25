using System.Text;
using System.Text.Json;
using Shared.DTOs;
using Shared.Helpers;

namespace VendasService.Services
{
    public interface IEstoqueServiceClient
    {
        Task<bool> ValidarEstoqueAsync(int produtoId, int quantidade);
        Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade, string motivo = "Venda");
        Task<ProdutoResponseDto?> GetProdutoAsync(int produtoId);
        Task<IEnumerable<ProdutoResponseDto>> GetProdutosComEstoqueAsync();
    }
    
    public class EstoqueServiceClient : IEstoqueServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstoqueServiceClient> _logger;
        private readonly string _baseUrl;
        
        public EstoqueServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<EstoqueServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["Services:EstoqueService:BaseUrl"] ?? "https://localhost:5001";
            
            // Configurar timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }
        
        public async Task<bool> ValidarEstoqueAsync(int produtoId, int quantidade)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/produtos/{produtoId}/validar-estoque/{quantidade}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<bool>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return apiResponse?.Success == true && apiResponse.Data;
                }
                
                _logger.LogWarning("Falha ao validar estoque para produto {ProdutoId}. Status: {StatusCode}", 
                    produtoId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar estoque para produto {ProdutoId}", produtoId);
                return false;
            }
        }
        
        public async Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade, string motivo = "Venda")
        {
            try
            {
                var requestData = new { Quantidade = quantidade, Motivo = motivo };
                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/produtos/{produtoId}/atualizar-estoque", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<bool>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return apiResponse?.Success == true && apiResponse.Data;
                }
                
                _logger.LogWarning("Falha ao atualizar estoque para produto {ProdutoId}. Status: {StatusCode}", 
                    produtoId, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar estoque para produto {ProdutoId}", produtoId);
                return false;
            }
        }
        
        public async Task<ProdutoResponseDto?> GetProdutoAsync(int produtoId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/produtos/{produtoId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ProdutoResponseDto>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return apiResponse?.Data;
                }
                
                _logger.LogWarning("Produto {ProdutoId} não encontrado no serviço de estoque", produtoId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produto {ProdutoId} no serviço de estoque", produtoId);
                return null;
            }
        }
        
        public async Task<IEnumerable<ProdutoResponseDto>> GetProdutosComEstoqueAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/produtos/com-estoque");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<ProdutoResponseDto>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    return apiResponse?.Data ?? new List<ProdutoResponseDto>();
                }
                
                _logger.LogWarning("Falha ao buscar produtos com estoque. Status: {StatusCode}", response.StatusCode);
                return new List<ProdutoResponseDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar produtos com estoque");
                return new List<ProdutoResponseDto>();
            }
        }
    }
}