using GestionPedidosDY.Application.Dtos;
using GestionPedidosDY.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionPedidosDY.Application.Contracts
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIdAsync(int id);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task CancelOrderAsync(int id);
    }
}
