using Pedido.Api.Models.Requests;
using System.Text.Json.Serialization;

namespace Pedido.Api.Models.Responses
{
    public class PedidoResponse
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("pedidoId")]
        public Guid PedidoId { get; set; }

        [JsonPropertyName("clienteId")]
        public int ClienteId { get; set; }

        [JsonPropertyName("imposto")]
        public decimal Imposto { get; set; }

        [JsonPropertyName("itens")]
        public List<ItemPedidoDto> Itens { get; set; } = new();
    }

}
