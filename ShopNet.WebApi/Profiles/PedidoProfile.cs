using AutoMapper;
using ShopNet.Models.Dtos;
using ShopNet.WebApi.Models;

namespace ShopNet.WebApi.Profiles
{
    public class PedidoProfile: Profile
    {
        public PedidoProfile()
        {
            CreateMap<Pedido, PedidoDto>();
            CreateMap<PedidoDto, Pedido>();
        }
    }
}
