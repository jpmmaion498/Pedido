using Xunit;
using Moq;
using FluentAssertions;
using Pedido.Application.Services;
using Pedido.Domain.Entities;
using Pedido.Domain.Interfaces;

namespace Pedido.Tests;

public class PedidoServiceTests
{
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
    private readonly PedidoService _pedidoService;

    public PedidoServiceTests()
    {
        _pedidoRepositoryMock = new Mock<IPedidoRepository>();
        _pedidoService = new PedidoService(_pedidoRepositoryMock.Object);
    }

    [Fact]
    public async Task CriarPedidoAsync_DeveChamarAdicionarAsync_UmaVez()
    {
        // Arrange
        var pedido = new PedidoEntity
        {
            PedidoId = Guid.NewGuid(),
            ClienteId = 123,
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

        // Act
        await _pedidoService.CriarPedidoAsync(pedido);

        // Assert
        _pedidoRepositoryMock.Verify(repo => repo.AdicionarAsync(pedido), Times.Once);
    }

    [Fact]
    public async Task PedidoExisteAsync_DeveRetornarTrue_SePedidoExistir()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        _pedidoRepositoryMock.Setup(r => r.ExistePedidoAsync(pedidoId)).ReturnsAsync(true);

        // Act
        var result = await _pedidoService.PedidoExisteAsync(pedidoId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ObterPedidoPorIdAsync_DeveRetornarPedido_SeEncontrado()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        var pedido = new PedidoEntity { PedidoId = pedidoId, ClienteId = 1 };
        _pedidoRepositoryMock.Setup(r => r.ObterPorIdAsync(pedidoId)).ReturnsAsync(pedido);

        // Act
        var result = await _pedidoService.ObterPedidoPorIdAsync(pedidoId);

        // Assert
        result.Should().NotBeNull();
        result!.PedidoId.Should().Be(pedidoId);
    }

    [Fact]
    public async Task ListarTodosAsync_DeveRetornarListaDePedidos()
    {
        // Arrange
        var pedidos = new List<PedidoEntity>
        {
            new PedidoEntity { PedidoId = Guid.NewGuid(), ClienteId = 1 }
        };
        _pedidoRepositoryMock.Setup(r => r.ListarTodosAsync()).ReturnsAsync(pedidos);

        // Act
        var result = await _pedidoService.ListarTodosAsync();

        // Assert
        result.Should().HaveCount(1);
    }
}
