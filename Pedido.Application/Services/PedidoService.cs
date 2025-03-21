using Pedido.Domain.Entities;
using Pedido.Domain.Interfaces;
using Serilog;

namespace Pedido.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;

    public PedidoService(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task CriarPedidoAsync(PedidoEntity pedido)
    {
        try
        {
            await _pedidoRepository.AdicionarAsync(pedido);
            Log.Information("Pedido {PedidoId} persistido com sucesso", pedido.PedidoId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao persistir o pedido {PedidoId}", pedido.PedidoId);
            throw;
        }
    }

    public async Task<PedidoEntity?> ObterPedidoPorIdAsync(Guid pedidoId)
    {
        try
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(pedidoId);
            Log.Information("Consulta ao pedido {PedidoId}: {Resultado}", pedidoId, pedido is not null ? "encontrado" : "não encontrado");
            return pedido;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao consultar o pedido {PedidoId}", pedidoId);
            throw;
        }
    }

    public async Task<List<PedidoEntity>> ListarTodosAsync()
    {
        try
        {
            var pedidos = await _pedidoRepository.ListarTodosAsync();
            Log.Information("Consulta de listagem de pedidos retornou {Quantidade} registros", pedidos.Count);
            return pedidos;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao listar todos os pedidos");
            throw;
        }
    }

    public async Task<bool> PedidoExisteAsync(Guid pedidoId)
    {
        try
        {
            var existe = await _pedidoRepository.ExistePedidoAsync(pedidoId);
            Log.Information("Verificação de duplicidade para pedido {PedidoId}: {Existe}", pedidoId, existe);
            return existe;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao verificar duplicidade do pedido {PedidoId}", pedidoId);
            throw;
        }
    }
}
