using Pedido.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pedido.Domain.Interfaces
{
    public interface IPedidoService
    {
        Task CriarPedidoAsync(PedidoEntity pedido);
        Task<PedidoEntity?> ObterPedidoPorIdAsync(Guid pedidoId);
        Task<List<PedidoEntity>> ListarTodosAsync();
        Task<bool> PedidoExisteAsync(Guid pedidoId);
    }
}
