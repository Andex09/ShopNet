using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNet.Models.Dtos;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;
using ShopNet.WebApi.Services.Interfaces;
using System.Security.Cryptography;

namespace ShopNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;


        public AuthController(IAuthRepository repo, ITokenService tokenService, IMapper mapper)
        {
            _repo = repo;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UsuarioLoginRegisterDto usuarioDto)
        {
            usuarioDto.UsuNombre = usuarioDto.UsuNombre.ToLower();
            
            if (await _repo.ExisteUsuario(usuarioDto.UsuNombre))
                return BadRequest("Ya existe un usuario con ese nombre");

            var usuarioNuevo = _mapper.Map<Usuario>(usuarioDto);
            usuarioNuevo.UsuRol = "Basic";
            var usuarioCreado = await _repo.Register(usuarioNuevo, usuarioDto.UsuPass);
            var usuarioCreadoDto = _mapper.Map<UsuarioListDto>(usuarioCreado);

            return Ok(usuarioCreadoDto);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(UsuarioLoginRegisterDto usuarioLoginDto)
        {
            var usuarioFromDb = await _repo.Login(usuarioLoginDto.UsuNombre, usuarioLoginDto.UsuPass);

            if (usuarioFromDb == null)
                return Unauthorized();

            var usuario = _mapper.Map<UsuarioListDto>(usuarioFromDb);

            var token = _tokenService.CreateToken(usuarioFromDb);

            return Ok(
                new
                {
                    token = token,
                    usuario = usuario
                });
        }

    }
}
