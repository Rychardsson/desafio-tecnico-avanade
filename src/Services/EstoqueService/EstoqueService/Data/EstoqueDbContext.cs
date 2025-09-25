using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace EstoqueService.Data
{
    public class EstoqueDbContext : DbContext
    {
        public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options)
        {
        }
        
        public DbSet<Produto> Produtos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuração da entidade Produto
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(p => p.Id);
                
                entity.Property(p => p.Nome)
                    .IsRequired()
                    .HasMaxLength(200);
                    
                entity.Property(p => p.Descricao)
                    .HasMaxLength(1000);
                    
                entity.Property(p => p.Preco)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");
                    
                entity.Property(p => p.QuantidadeEstoque)
                    .IsRequired();
                    
                entity.Property(p => p.DataCriacao)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
                    
                entity.Property(p => p.Ativo)
                    .IsRequired()
                    .HasDefaultValue(true);
                    
                entity.HasIndex(p => p.Nome)
                    .HasDatabaseName("IX_Produto_Nome");
            });
        }
    }
}