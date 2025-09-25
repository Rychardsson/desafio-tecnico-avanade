using VendasService.Data;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace VendasService.Repositories
{
    public interface IPedidoRepository
    {
        Task<IEnumerable<Pedido>> GetAllAsync();
        Task<IEnumerable<Pedido>> GetByClienteIdAsync(string clienteId);
        Task<Pedido?> GetByIdAsync(int id);
        Task<Pedido> CreateAsync(Pedido pedido);
        Task<Pedido> UpdateAsync(Pedido pedido);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Pedido>> GetPedidosByStatusAsync(StatusPedido status);
        Task<IEnumerable<Pedido>> GetPedidosByPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<decimal> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<Pedido>> GetPedidosRecentesAsync(int limit = 10);
    }
    
    public class PedidoRepository : IPedidoRepository
    {
        private readonly VendasDbContext _context;
        
        public PedidoRepository(VendasDbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<Pedido>> GetAllAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Pedido>> GetByClienteIdAsync(string clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }
        
        public async Task<Pedido?> GetByIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        
        public async Task<Pedido> CreateAsync(Pedido pedido)
        {
            pedido.DataCriacao = DateTime.UtcNow;
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            
            // Recarregar o pedido com os itens para garantir que os IDs estão corretos
            return await GetByIdAsync(pedido.Id) ?? pedido;
        }
        
        public async Task<Pedido> UpdateAsync(Pedido pedido)
        {
            pedido.DataAtualizacao = DateTime.UtcNow;
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }
        
        public async Task<bool> DeleteAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
                return false;
                
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Pedidos.AnyAsync(p => p.Id == id);
        }
        
        public async Task<IEnumerable<Pedido>> GetPedidosByStatusAsync(StatusPedido status)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Pedido>> GetPedidosByPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .Where(p => p.DataCriacao >= dataInicio && p.DataCriacao <= dataFim)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();
        }
        
        public async Task<decimal> GetTotalVendasPorPeriodoAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _context.Pedidos
                .Where(p => p.DataCriacao >= dataInicio && 
                           p.DataCriacao <= dataFim && 
                           p.Status != StatusPedido.Cancelado)
                .SumAsync(p => p.ValorTotal);
        }
        
        public async Task<IEnumerable<Pedido>> GetPedidosRecentesAsync(int limit = 10)
        {
            return await _context.Pedidos
                .Include(p => p.Itens)
                .OrderByDescending(p => p.DataCriacao)
                .Take(limit)
                .ToListAsync();
        }
    }
}