using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pedido.Api.Models.Requests;
using Pedido.Api.Models.Responses;
using Pedido.Domain.Entities;
using Pedido.Domain.Interfaces;
using Pedido.Domain.Settings;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace Pedido.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidoController : ControllerBase
{
    private readonly IPedidoService _pedidoService;
    private readonly SqsService _sqsService;
    private readonly FeatureFlags _features;

    public PedidoController(IPedidoService pedidoService, SqsService sqsService, IOptions<FeatureFlags> features)
    {
        _pedidoService = pedidoService;
        _sqsService = sqsService;
        _features = features.Value;
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo pedido", Description = "Criação de pedido. Não permite duplicidade.")]
    [ProducesResponseType(typeof(object), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CriarPedido([FromBody] CreatePedidoRequest request)
    {
        if (request is null || request.Itens is null || !request.Itens.Any())
        {
            Log.Warning("Requisição de pedido inválida: estrutura nula ou sem itens");
            return BadRequest("Pedido inválido ou sem itens.");
        }

        if (await _pedidoService.PedidoExisteAsync(request.PedidoId))
        {
            Log.Warning("Tentativa de criação de pedido duplicado: {PedidoId}", request.PedidoId);
            return Conflict("Já existe um pedido com esse ID.");
        }

        var pedido = new PedidoEntity
        {
            PedidoId = request.PedidoId,
            ClienteId = request.ClienteId,
            Itens = request.Itens.Select(i => new ItemPedidoEntity
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade,
                Valor = i.Valor
            }).ToList()
        };

        Log.Information("Pedido {PedidoId} recebido. Enviando para a fila.", request.PedidoId);

        await _sqsService.EnviarParaFilaAsync(pedido);

        Log.Information("Pedido {PedidoId} enviado para a fila com sucesso", pedido.PedidoId);

        return Accepted(new { id = pedido.PedidoId });
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obter pedido por ID", Description = "Retorna um pedido completo com cálculo de imposto.")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPedidoPorId(Guid id)
    {
        var pedido = await _pedidoService.ObterPedidoPorIdAsync(id);
        if (pedido is null)
        {
            Log.Warning("Pedido {PedidoId} não encontrado", id);
            return NotFound();
        }

        Log.Information("Pedido {PedidoId} localizado com sucesso", id);

        var total = pedido.Itens.Sum(i => i.Valor * i.Quantidade);
        var imposto = _features.UsarNovaRegraImposto ? total * 0.2m : total * 0.3m;

        var response = new PedidoResponse
        {
            Id = pedido.PedidoId,
            PedidoId = pedido.PedidoId,
            ClienteId = pedido.ClienteId,
            Imposto = imposto,
            Itens = pedido.Itens.Select(i => new ItemPedidoDto
            {
                ProdutoId = i.ProdutoId,
                Quantidade = i.Quantidade,
                Valor = i.Valor
            }).ToList()
        };

        return Ok(response);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Listar todos os pedidos", Description = "Retorna todos os pedidos cadastrados com cálculo de imposto.")]
    [ProducesResponseType(typeof(List<PedidoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodos()
    {
        var pedidos = await _pedidoService.ListarTodosAsync();

        Log.Information("Listagem de todos os pedidos solicitada. Total encontrados: {Quantidade}", pedidos.Count);

        var responses = pedidos.Select(p =>
        {
            var total = p.Itens.Sum(i => i.Valor * i.Quantidade);
            var imposto = _features.UsarNovaRegraImposto ? total * 0.2m : total * 0.3m;

            return new PedidoResponse
            {
                Id = p.PedidoId,
                PedidoId = p.PedidoId,
                ClienteId = p.ClienteId,
                Imposto = imposto,
                Itens = p.Itens.Select(i => new ItemPedidoDto
                {
                    ProdutoId = i.ProdutoId,
                    Quantidade = i.Quantidade,
                    Valor = i.Valor
                }).ToList()
            };
        });

        return Ok(responses);
    }
}
