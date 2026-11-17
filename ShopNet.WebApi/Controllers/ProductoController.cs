using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopNet.Models.Dtos;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;

namespace ShopNet.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoRepository _repo;
        private readonly IMapper _mapper;

        public ProductoController(IProductoRepository productoRepository, IMapper mapper)
        {
            _repo = productoRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetItems()
        {
            try
            {
                var listaProducto = await _repo.GetItems();

                if (listaProducto == null)
                {
                    return NotFound();
                }

                var listaProductoDto = _mapper.Map<IEnumerable<ProductoDto>>(listaProducto);

                return Ok(listaProductoDto);


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
                var producto = await _repo.GetItem(id);

                if (producto == null)
                {
                    return NotFound();
                }

                var productoDto = _mapper.Map<ProductoDto>(producto);

                return Ok(productoDto);


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var producto = await _repo.GetItem(id);

                if (producto == null)
                {
                    return NotFound();
                }

                await _repo.DeleteItem(producto);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost, Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateItem(ProductoDto productoDto)
        {
            try
            {
                var producto = _mapper.Map<Producto>(productoDto) ;

                producto = await _repo.AddItem(producto);

                var productoItemDto = _mapper.Map<ProductoDto>(producto);

                return Ok(productoItemDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPut("{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(int id, ProductoDto productoDto)
        {
            try
            {
                var producto = _mapper.Map<Producto>(productoDto);

                if (id != producto.ProdID)
                {
                    return BadRequest();
                }

                var productoItem = await _repo.GetItem(id);

                if (productoItem == null)
                {
                    return NotFound();
                }

                await _repo.UpdateItem(producto);

                return NoContent();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
