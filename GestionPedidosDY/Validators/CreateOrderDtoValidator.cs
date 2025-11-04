using FluentValidation;
using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Application.Dtos;

namespace GestionPedidosDY.Application.Validators
{
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateOrderDtoValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.CustomerName).NotEmpty();
            RuleFor(x => x.CustomerEmail).EmailAddress();
            RuleFor(x => x.OrderItems).NotEmpty();

            RuleForEach(x => x.OrderItems).MustAsync(HaveEnoughStock).WithMessage("No hay suficiente Stock para este producto.");
        }

        private async Task<bool> HaveEnoughStock(OrderItemDto orderItem, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId);
            if (product == null) return false;
            return product.Stock >= orderItem.Quantity;
        }
    }
}
