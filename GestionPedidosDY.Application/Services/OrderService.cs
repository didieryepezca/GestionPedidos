using FluentValidation;
using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Application.Dtos;
using GestionPedidosDY.Core;

namespace GestionPedidosDY.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderFactory _orderFactory;
        private readonly IValidator<CreateOrderDto> _validator;

        public OrderService(IUnitOfWork unitOfWork, IOrderFactory orderFactory, IValidator<CreateOrderDto> validator)
        {
            _unitOfWork = unitOfWork;
            _orderFactory = orderFactory;
            _validator = validator;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var validationResult = await _validator.ValidateAsync(createOrderDto);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var productIds = createOrderDto.OrderItems.Select(oi => oi.ProductId).ToList();
            var products = await _unitOfWork.Products.GetAllAsync();
            var productsForOrder = products.Where(p => productIds.Contains(p.Id));

            var order = _orderFactory.Create(createOrderDto, productsForOrder);

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return order;
        }

        public async Task CancelOrderAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetOrderByIdWithItemsAsync(id);
            if (order == null)
            {
                return;
            }

            order.Status = OrderStatus.Cancelled;

            foreach (var item in order.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                product.Stock += item.Quantity;
                _unitOfWork.Products.Update(product);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _unitOfWork.Orders.GetOrderByIdWithItemsAsync(id);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _unitOfWork.Orders.GetAllWithItemsAsync();
        }
    }
}
