using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShopNet.Models.Dtos;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;
using System.Linq;
using System.Security.Claims;

namespace ShopNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    
    public class PedidoController : ControllerBase
    {
        private readonly IPedidoRepository _repo;
        private readonly ShopNetContext _context;
        private readonly IMapper _mapper;

        public PedidoController(IPedidoRepository repo, IMapper mapper, ShopNetContext context)
        {
            _repo = repo;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetItems()
        {
            try
            {
                var listaPedido = await _repo.GetItems();

                if (listaPedido == null)
                {
                    return NotFound();
                }

                var listaPedidoDto = _mapper.Map<IEnumerable<PedidoDto>>(listaPedido);

                return Ok(listaPedidoDto);


            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetItemsById(int id)
        {
            try
            {
                var pedido = await _repo.GetItem(id);

                if (pedido == null)
                {
                    return NotFound();
                }

                var pedidoDto = _mapper.Map<PedidoDto>(pedido);

                return Ok(pedidoDto);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("/api/PedidoUsuario/{id}")]
        public async Task<IActionResult> GetItemsByUser(int id)
        {
            try
            {
                var usuarioClaim = User.Claims.FirstOrDefault( x => x.Type.Equals(ClaimTypes.Name, StringComparison.InvariantCultureIgnoreCase));

                var usuario = _context.Usuarios.Where( u => u.UsuNombre == usuarioClaim.Value && u.UsuID == id).FirstOrDefault();

                if (usuario == null)
                {
                    return BadRequest();
                }

                var listaPedido = await _repo.GetItemsByUser(id);

                if (listaPedido == null)
                {
                    return NotFound();
                }

                var listaPedidoDto = _mapper.Map<IEnumerable<PedidoDto>>(listaPedido);

                return Ok(listaPedidoDto);


            }
            catch (Exception)
            {

                return BadRequest();
            }
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pedido = await _repo.GetItem(id);

                if (pedido == null)
                {
                    return NotFound();
                }

                await _repo.DeleteItem(pedido);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateItem(PedidoDto pedidoDto)
        {
            try
            {
                var pedido = _mapper.Map<Pedido>(pedidoDto);

                pedido = await _repo.AddItem(pedido);

                var pedidoItemDto = _mapper.Map<PedidoDto>(pedido);

                return Ok(pedidoItemDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        [Route("/api/PedidoUsuario")]
        public async Task<IActionResult> CreateItemByUser(PedidoDto pedidoDto)
        {
            try
            {
                var usuarioClaim = User.Claims.FirstOrDefault(x => x.Type.Equals(ClaimTypes.Name, StringComparison.InvariantCultureIgnoreCase));

                var usuario = _context.Usuarios.Where(u => u.UsuNombre == usuarioClaim.Value && u.UsuID == pedidoDto.PedUsu).FirstOrDefault();

                if (usuario == null)
                {
                    return BadRequest();
                }

                var pedido = _mapper.Map<Pedido>(pedidoDto);

                pedido = await _repo.AddItem(pedido);

                var pedidoItemDto = _mapper.Map<PedidoDto>(pedido);

                return Ok(pedidoItemDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(int id, PedidoDto pedidoDto)
        {
            try
            {
                var pedido = _mapper.Map<Pedido>(pedidoDto);

                if (id != pedido.PedID)
                {
                    return BadRequest();
                }

                var pedidoItem = await _repo.GetItem(id);

                if (pedidoItem == null)
                {
                    return NotFound();
                }

                await _repo.UpdateItem(pedido);

                return NoContent();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
