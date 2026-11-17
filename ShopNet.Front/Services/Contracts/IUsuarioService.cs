using ShopNet.Models.Dtos;

namespace ShopNet.Front.Services.Contracts
{
    public interface IUsuarioService
    {
        Task<List<UsuarioListDto>> GetItems();
        Task<UsuarioListDto> GetItem(int id);
        Task<UsuarioUpdateDto> UpdateItem(int id, UsuarioUpdateDto usuario);
    }
}
