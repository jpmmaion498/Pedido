using Microsoft.EntityFrameworkCore;
using Pedido.Domain.Entities;
using Pedido.Infrastructure.Context;
using Pedido.Infrastructure.Repositories;
using Xunit;
using FluentAssertions;

namespace Pedido.Tests;

public class PedidoRepositoryTests
{
    private readonly PedidoDbContext _context;
    private readonly PedidoRepository _repository;

    public PedidoRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PedidoDbContext>()
            .UseInMemoryDatabase(databaseName: "PedidoDb_Test")
            .Options;

        _context = new PedidoDbContext(options);
        _repository = new PedidoRepository(_context);
    }

    [Fact]
    public async Task AdicionarAsync_DevePersistirPedido()
    {
        var pedido = GerarPedido();

        await _repository.AdicionarAsync(pedido);

        var salvo = await _context.Pedidos.FindAsync(pedido.PedidoId);
        salvo.Should().NotBeNull();
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarPedidoComItens()
    {
        var pedido = GerarPedido();
        await _repository.AdicionarAsync(pedido);

        var retorno = await _repository.ObterPorIdAsync(pedido.PedidoId);

        retorno.Should().NotBeNull();
        retorno!.Itens.Should().HaveCount(1);
    }

    [Fact]
    public async Task PedidoExisteAsync_DeveRetornarTrueSeExistir()
    {
        var pedido = GerarPedido();
        await _repository.AdicionarAsync(pedido);

        var existe = await _repository.ExistePedidoAsync(pedido.PedidoId);

        existe.Should().BeTrue();
    }

    [Fact]
    public async Task ListarTodosAsync_DeveRetornarPedidos()
    {
        var pedido = GerarPedido();
        await _repository.AdicionarAsync(pedido);

        var pedidos = await _repository.ListarTodosAsync();

        pedidos.Should().NotBeEmpty();
    }

    private PedidoEntity GerarPedido()
    {
        return new PedidoEntity
        {
            PedidoId = Guid.NewGuid(),
            ClienteId = 1,
            Itens = new List<ItemPedidoEntity>
            {
                new ItemPedidoEntity
                {
                    ProdutoId = 1,
                    Quantidade = 2,
                    Valor = 10.0m
                }
            }
        };
    }
}
