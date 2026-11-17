using ShopNet.WebApi.Models;

namespace ShopNet.WebApi.Repositories.Contracts
{
    public interface IPedidoRepository
    {
        Task<List<Pedido>> GetItems();
        Task<List<Pedido>> GetItemsByUser(int id);
        Task<Pedido> GetItem(int id);
        Task DeleteItem(Pedido pedido);
        Task<Pedido> AddItem(Pedido pedido);
        Task UpdateItem(Pedido pedido);

    }
}
