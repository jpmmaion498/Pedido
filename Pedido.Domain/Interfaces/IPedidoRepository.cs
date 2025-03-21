using Pedido.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pedido.Domain.Interfaces
{
    public interface IPedidoRepository
    {
        Task AdicionarAsync(PedidoEntity pedido);
        Task<PedidoEntity?> ObterPorIdAsync(Guid pedidoId);
        Task<List<PedidoEntity>> ListarTodosAsync();
        Task<bool> ExistePedidoAsync(Guid pedidoId);
    }
}
