using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pedido.Domain.Entities
{
    public class ItemPedidoEntity
    {
        [Key]
        public Guid ItemPedidoId { get; set; } // ✅ Definir como chave primária        
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }
        public Guid PedidoId { get; set; }

        [ForeignKey("PedidoId")]
        public PedidoEntity? Pedido { get; set; } // Relacionamento com Pedido

    }
}
