using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace VendasService.Data
{
    public class VendasDbContext : DbContext
    {
        public VendasDbContext(DbContextOptions<VendasDbContext> options) : base(options)
        {
        }
        
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuração da entidade Pedido
            modelBuilder.Entity<Pedido>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.Property(p => p.ClienteId)
                    .IsRequired()
                    .HasMaxLength(100);
                    
                entity.Property(p => p.ValorTotal)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                    
                entity.Property(p => p.Status)
                    .IsRequired()
                    .HasConversion<int>();
                    
                entity.Property(p => p.DataCriacao)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
                    
                entity.Property(p => p.Observacoes)
                    .HasMaxLength(500);
                    
                entity.HasIndex(p => p.ClienteId)
                    .HasDatabaseName("IX_Pedido_ClienteId");
                    
                entity.HasIndex(p => p.Status)
                    .HasDatabaseName("IX_Pedido_Status");
                    
                entity.HasIndex(p => p.DataCriacao)
                    .HasDatabaseName("IX_Pedido_DataCriacao");
            });
            
            // Configuração da entidade ItemPedido
            modelBuilder.Entity<ItemPedido>(entity =>
            {
                entity.HasKey(i => i.Id);
                
                entity.Property(i => i.NomeProduto)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(i => i.PrecoUnitario)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                    
                entity.Property(i => i.Quantidade)
                    .IsRequired();
                    
                // Relacionamento com Pedido
                entity.HasOne<Pedido>()
                    .WithMany(p => p.Itens)
                    .HasForeignKey(i => i.PedidoId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasIndex(i => i.PedidoId)
                    .HasDatabaseName("IX_ItemPedido_PedidoId");
                    
                entity.HasIndex(i => i.ProdutoId)
                    .HasDatabaseName("IX_ItemPedido_ProdutoId");
            });
        }
    }
}
