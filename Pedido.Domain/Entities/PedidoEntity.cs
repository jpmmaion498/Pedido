using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pedido.Domain.Entities
{
    public class PedidoEntity
    {
        public PedidoEntity()
        {
            CreatedAt = DateTime.UtcNow;
            Itens = new List<ItemPedidoEntity>();
        }
        [Key]
        public Guid PedidoId { get; set; }
        public int ClienteId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<ItemPedidoEntity> Itens { get; set; }

    }
}
