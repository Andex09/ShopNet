using AutoMapper;
using ShopNet.Models.Dtos;
using ShopNet.WebApi.Models;

namespace ShopNet.WebApi.Profiles
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<UsuarioLoginRegisterDto, Usuario>();
            CreateMap<UsuarioCreateDto, Usuario>();
            CreateMap<UsuarioUpdateDto, Usuario>();
            CreateMap<Usuario, UsuarioListDto>();
        }

    }
}
