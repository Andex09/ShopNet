using ShopNet.WebApi.Models;

namespace ShopNet.WebApi.Repositories.Contracts
{
    public interface IProductoRepository
    {
        Task<List<Producto>> GetItems();
        Task<Producto> GetItem(int id);
        Task DeleteItem(Producto producto);
        Task<Producto> AddItem(Producto producto);
        Task UpdateItem(Producto producto);
    }
}
