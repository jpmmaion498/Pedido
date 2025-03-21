using Microsoft.EntityFrameworkCore;
using Pedido.Domain.Entities;
using Pedido.Domain.Interfaces;
using Pedido.Infrastructure.Context;
using Serilog;

namespace Pedido.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly PedidoDbContext _context;

    public PedidoRepository(PedidoDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(PedidoEntity pedido)
    {
        try
        {
            await _context.Pedidos.AddAsync(pedido);
            await _context.SaveChangesAsync();
            Log.Information("Pedido {PedidoId} inserido no banco de dados", pedido.PedidoId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao salvar pedido {PedidoId} no banco", pedido.PedidoId);
            throw;
        }
    }

    public async Task<PedidoEntity?> ObterPorIdAsync(Guid pedidoId)
    {
        try
        {
            var pedido = await _context.Pedidos
                .Include(p => p.Itens)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            Log.Information("Pedido {PedidoId} consultado no banco: {Status}", pedidoId, pedido is not null ? "encontrado" : "não encontrado");

            return pedido;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao consultar pedido {PedidoId}", pedidoId);
            throw;
        }
    }

    public async Task<List<PedidoEntity>> ListarTodosAsync()
    {
        try
        {
            var pedidos = await _context.Pedidos
                .Include(p => p.Itens)
                .ToListAsync();

            Log.Information("Listagem de pedidos realizada: {Quantidade} encontrados", pedidos.Count);

            return pedidos;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao listar pedidos");
            throw;
        }
    }

    public async Task<bool> ExistePedidoAsync(Guid pedidoId)
    {
        try
        {
            var existe = await _context.Pedidos.AnyAsync(p => p.PedidoId == pedidoId);
            Log.Information("Verificação de existência no banco para pedido {PedidoId}: {Resultado}", pedidoId, existe);
            return existe;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao verificar existência do pedido {PedidoId}", pedidoId);
            throw;
        }
    }
}
