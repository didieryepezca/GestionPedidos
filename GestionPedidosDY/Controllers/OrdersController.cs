using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GestionPedidosDY.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {        
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderService _orderService;        
        private readonly IMessagePublisher _messagePublisher;

        public OrdersController(IUnitOfWork unitOfWork, IOrderService orderService, IMessagePublisher messagePublisher)
        {
            _unitOfWork = unitOfWork;
            _orderService = orderService;            
            _messagePublisher = messagePublisher;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateOrderDto createOrderDto)
        {
            var order = await _orderService.CreateOrderAsync(createOrderDto);
            await _unitOfWork.SaveChangesAsync();

            var options = new System.Text.Json.JsonSerializerOptions
            {
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
            var orderJson = System.Text.Json.JsonSerializer.Serialize(order, options);
            _messagePublisher.Publish(orderJson);

            return CreatedAtRoute("GetOrderById", new { id = order.Id }, order);
        }

        [HttpGet("{id}", Name = "GetOrderById")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            await _orderService.CancelOrderAsync(id);
            return NoContent();
        }
    }
}
