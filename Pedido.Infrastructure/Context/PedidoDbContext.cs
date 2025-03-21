using Microsoft.EntityFrameworkCore;
using Pedido.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pedido.Infrastructure.Context
{
    public class PedidoDbContext : DbContext
    {
        public PedidoDbContext(DbContextOptions<PedidoDbContext> options) : base(options) { }

        public DbSet<PedidoEntity> Pedidos { get; set; }
        public DbSet<ItemPedidoEntity> ItensPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 🔹 Definir chave primária para `ItemPedidoEntity`
            modelBuilder.UseSerialColumns();

            modelBuilder.Entity<PedidoEntity>()
              .HasMany(p => p.Itens)
              .WithOne(i => i.Pedido)
              .HasForeignKey(i => i.PedidoId)
              .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
