using GestionPedidosDY.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionPedidosDY.Application.Contracts
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order> GetOrderByIdWithItemsAsync(int id);
        Task<IEnumerable<Order>> GetAllWithItemsAsync();
    }
}
