using ShopNet.Front.Services.Contracts;
using ShopNet.Models.Dtos;
using System.Net.Http.Json;

namespace ShopNet.Front.Services
{
    public class ProductoService : IProductoService
    {
        private readonly HttpClient _httpClient;
        public ProductoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ProductoDto>> GetItems()
        {
            try
            {
                var productos = await _httpClient.GetFromJsonAsync<List<ProductoDto>>("/api/Producto");

                return productos;

            }
            catch (Exception)
            {
                return null;
            }
        }
        public async Task<ProductoDto> GetItem(int id)
        {
            try
            {
                var producto = await _httpClient.GetFromJsonAsync<ProductoDto>($"/api/Producto/{id}");

                return producto;

            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<ProductoDto> CreateItem(ProductoDto producto)
        {
            try
            {
                await _httpClient.PostAsJsonAsync($"/api/Producto", producto);

                return producto;

            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
