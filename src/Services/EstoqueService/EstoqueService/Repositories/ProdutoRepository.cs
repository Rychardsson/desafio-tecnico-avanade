using EstoqueService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace EstoqueService.Repositories
{
    public interface IProdutoRepository
    {
        Task<IEnumerable<Produto>> GetAllAsync();
        Task<Produto?> GetByIdAsync(int id);
        Task<Produto> CreateAsync(Produto produto);
        Task<Produto> UpdateAsync(Produto produto);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Produto>> GetProdutosComEstoqueAsync();
        Task<bool> TemEstoqueSuficienteAsync(int produtoId, int quantidade);
        Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade);
        Task<IEnumerable<Produto>> SearchAsync(string searchTerm);
    }
    
    public class ProdutoRepository : IProdutoRepository
    {
        private readonly EstoqueDbContext _context;
        
        public ProdutoRepository(EstoqueDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Produto>> GetAllAsync()
        {
            return await _context.Produtos
                .Where(p => p.Ativo)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
        
        public async Task<Produto?> GetByIdAsync(int id)
        {
            return await _context.Produtos
                .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);
        }
        
        public async Task<Produto> CreateAsync(Produto produto)
        {
            produto.DataCriacao = DateTime.UtcNow;
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return produto;
        }
        
        public async Task<Produto> UpdateAsync(Produto produto)
        {
            produto.DataAtualizacao = DateTime.UtcNow;
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync();
            return produto;
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            var produto = await GetByIdAsync(id);
            if (produto == null)
                return false;
                
            produto.Ativo = false;
            produto.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Produtos
                .AnyAsync(p => p.Id == id && p.Ativo);
        }
        
        public async Task<IEnumerable<Produto>> GetProdutosComEstoqueAsync()
        {
            return await _context.Produtos
                .Where(p => p.Ativo && p.QuantidadeEstoque > 0)
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
        
        public async Task<bool> TemEstoqueSuficienteAsync(int produtoId, int quantidade)
        {
            var produto = await GetByIdAsync(produtoId);
            return produto != null && produto.QuantidadeEstoque >= quantidade;
        }
        
        public async Task<bool> AtualizarEstoqueAsync(int produtoId, int quantidade)
        {
            var produto = await GetByIdAsync(produtoId);
            if (produto == null || produto.QuantidadeEstoque < quantidade)
                return false;
                
            produto.QuantidadeEstoque -= quantidade;
            produto.DataAtualizacao = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<IEnumerable<Produto>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();
                
            searchTerm = searchTerm.ToLower();
            return await _context.Produtos
                .Where(p => p.Ativo && 
                           (p.Nome.ToLower().Contains(searchTerm) || 
                            p.Descricao.ToLower().Contains(searchTerm)))
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
    }
}
