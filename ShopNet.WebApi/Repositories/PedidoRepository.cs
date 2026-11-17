using Microsoft.EntityFrameworkCore;
using ShopNet.WebApi.Models;
using ShopNet.WebApi.Repositories.Contracts;

namespace ShopNet.WebApi.Repositories
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ShopNetContext _context;
        public PedidoRepository(ShopNetContext context)
        {
            _context = context;
        }

        public async Task<Pedido> AddItem(Pedido pedido)
        {
            _context.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido;
        }

        public async Task DeleteItem(Pedido pedido)
        {
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
        }

        public async Task<Pedido> GetItem(int id)
        {
            return await _context.Pedidos.FindAsync(id);
        }

        public async Task<List<Pedido>> GetItems()
        {
            return await _context.Pedidos.ToListAsync();

        }

        public async Task<List<Pedido>> GetItemsByUser(int id)
        {
            return await _context.Pedidos.Where(p => p.PedUsu == id).ToListAsync();
        }

        public async Task UpdateItem(Pedido pedido)
        {
            var pedidoItem = await _context.Pedidos.FirstOrDefaultAsync(p => p.PedID == pedido.PedID);

            if (pedidoItem != null)
            {
                pedidoItem.PedVrUnit = pedido.PedVrUnit;
                pedidoItem.PedCant = pedido.PedCant;
                pedidoItem.PedIVA = pedido.PedIVA;
                pedidoItem.PedTotal = pedido.PedTotal;
                pedidoItem.PedSubTot = pedido.PedSubTot;
                await _context.SaveChangesAsync();
            }
        }

    }
}
