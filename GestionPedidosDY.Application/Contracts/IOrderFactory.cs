using GestionPedidosDY.Application.Dtos;
using GestionPedidosDY.Core;

namespace GestionPedidosDY.Application.Contracts
{
    public interface IOrderFactory
    {        
        Order Create(CreateOrderDto createOrderDto, IEnumerable<Product> products);
    }
}
