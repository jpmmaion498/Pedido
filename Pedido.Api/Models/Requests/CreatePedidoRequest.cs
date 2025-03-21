using System.Text.Json.Serialization;

namespace Pedido.Api.Models.Requests
{
    public class CreatePedidoRequest
    {
        [JsonPropertyName("pedidoId")]
        public Guid PedidoId { get; set; }

        [JsonPropertyName("clienteId")]
        public int ClienteId { get; set; }

        [JsonPropertyName("itens")]
        public List<ItemPedidoDto> Itens { get; set; } = new();
    }

    public class ItemPedidoDto
    {
        [JsonPropertyName("produtoId")]
        public int ProdutoId { get; set; }

        [JsonPropertyName("quantidade")]
        public int Quantidade { get; set; }

        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }
    }
}
