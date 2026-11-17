using ShopNet.Front.Services.Contracts;
using ShopNet.Models.Dtos;
using System.Net.Http;
using System.Net.Http.Json;

namespace ShopNet.Front.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly HttpClient _httpClient;
        public PedidoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<PedidoDto>> GetItems()
        {
            try
            {
                var pedidos = await _httpClient.GetFromJsonAsync<List<PedidoDto>>("/api/Pedido");

                return pedidos;

            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<PedidoDto> GetItem(int id)
        {
            try
            {
                var pedido = await _httpClient.GetFromJsonAsync<PedidoDto>($"/api/Pedido/{id}");

                return pedido;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
