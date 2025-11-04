namespace GestionPedidosDY.Application.Dtos
{
    public class CreateOrderDto
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
