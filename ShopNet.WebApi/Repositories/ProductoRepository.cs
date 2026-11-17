using Microsoft.EntityFrameworkCore;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;

namespace ShopNet.WebApi.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly ShopNetContext _context;
        public ProductoRepository(ShopNetContext context)
        {
            _context = context;
        }

        public async Task<Producto> AddItem(Producto producto)
        {
            _context.Add(producto);
            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task DeleteItem(Producto producto)
        {
            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }

        public async Task<Producto> GetItem(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<List<Producto>> GetItems()
        {
            return await _context.Productos.ToListAsync();

        }

        public async Task UpdateItem(Producto producto)
        {
            var productoItem = await _context.Productos.FirstOrDefaultAsync(p => p.ProdID == producto.ProdID);

            if (productoItem != null)
            {
                productoItem.ProDesc = producto.ProDesc;
                productoItem.ProValor = producto.ProValor;
                await _context.SaveChangesAsync();
            }
        }
    }
}
