using ShopNet.Models.Dtos;

namespace ShopNet.Front.Services.Contracts
{
    public interface IPedidoService
    {
        Task<List<PedidoDto>> GetItems();
        Task<PedidoDto> GetItem(int id);
    }
}
