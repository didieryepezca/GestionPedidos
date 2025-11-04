using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Application.Dtos;
using GestionPedidosDY.Core;

namespace GestionPedidosDY.Application.Factories
{
    public class OrderFactory : IOrderFactory
    {
        public Order Create(CreateOrderDto createOrderDto, IEnumerable<Product> products)
        {
            var order = new Order
            {
                CustomerName = createOrderDto.CustomerName,
                CustomerEmail = createOrderDto.CustomerEmail,
                OrderItems = createOrderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity
                }).ToList()
            };

            decimal total = 0;
            foreach (var item in order.OrderItems)
            {
                var product = products.First(p => p.Id == item.ProductId);
                total += product.Price * item.Quantity;

                item.UnitPrice = product.Price;                
            }
            order.Total = total;

            return order;
        }
    }
}
