using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopNet.Models.Dtos;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;


namespace ShopNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioRepository _repo;
        private readonly IMapper _mapper;
        
        public UsuarioController(IUsuarioRepository usuarioRepository, IMapper mapper)
        {
            _repo = usuarioRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            try
            {
                var listaUsuario = await _repo.GetItems();

                if (listaUsuario == null)
                {
                    return NotFound();
                }

                var listaUsuarioDto = _mapper.Map<IEnumerable<UsuarioListDto>>(listaUsuario);

                return Ok(listaUsuarioDto);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemsById(int id)
        {
            try
            {
                var usuario = await _repo.GetItem(id);

                if (usuario == null)
                {
                    return NotFound();
                }

                var usuarioDto = _mapper.Map<UsuarioListDto>(usuario);

                return Ok(usuarioDto);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem(UsuarioCreateDto usuarioDto)
        {
            try
            {
                usuarioDto.UsuNombre = usuarioDto.UsuNombre.ToLower();

                if (await _repo.ExisteUsuario(usuarioDto.UsuNombre))
                    return BadRequest("Ya existe un usuario con ese nombre");

                var usuarioNuevo = _mapper.Map<Usuario>(usuarioDto);
                var usuarioCreado = await _repo.AddItem(usuarioNuevo, usuarioDto.UsuPass);
                var usuarioCreadoDto = _mapper.Map<UsuarioListDto>(usuarioCreado);

                return Ok(usuarioCreadoDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UsuarioUpdateDto usuarioDto)
        {
            try
            {
                var usuario = _mapper.Map<Usuario>(usuarioDto);

                if (id != usuario.UsuID)
                {
                    return BadRequest();
                }

                var usuarioItem = await _repo.GetItem(id);

                if (usuarioItem == null)
                {
                    return NotFound();
                }

                await _repo.UpdateItem(usuario, usuarioDto.UsuPass);

                var usuarioItemDto = _mapper.Map<UsuarioListDto>(usuario);

                return NoContent();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var usuario = await _repo.GetItem(id);

                if (usuario == null)
                {
                    return NotFound();
                }

                await _repo.DeleteItem(usuario);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
