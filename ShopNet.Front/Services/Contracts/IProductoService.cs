using ShopNet.Models.Dtos;

namespace ShopNet.Front.Services.Contracts
{
    public interface IProductoService
    {
        Task<List<ProductoDto>> GetItems();
        Task<ProductoDto> GetItem(int id);
        Task<ProductoDto> CreateItem(ProductoDto producto);
    }
}
