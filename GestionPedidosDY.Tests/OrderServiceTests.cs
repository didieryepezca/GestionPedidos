using Xunit;
using Moq;
using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Application.Services;
using GestionPedidosDY.Core;
using GestionPedidosDY.Application.Dtos;
using FluentValidation.TestHelper;
using GestionPedidosDY.Application.Validators;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderFactory> _orderFactoryMock;
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly OrderService _orderService;
    private readonly CreateOrderDtoValidator _validator;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderFactoryMock = new Mock<IOrderFactory>();
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _productRepositoryMock = new Mock<IProductRepository>();

        _unitOfWorkMock.Setup(uow => uow.Orders).Returns(_orderRepositoryMock.Object);
        _unitOfWorkMock.Setup(uow => uow.Products).Returns(_productRepositoryMock.Object);

        _validator = new CreateOrderDtoValidator(_unitOfWorkMock.Object);
        _orderService = new OrderService(_unitOfWorkMock.Object, _orderFactoryMock.Object, _validator);        
    }

    [Fact]
    public async Task CreateOrder_Successful()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 1, Quantity = 1 }
            }
        };

        var products = new List<Product> { new Product { Id = 1, Name = "Test Product", Price = 100, Stock = 10 } };
        var order = new Order { Id = 1, CustomerName = "Test Customer", Total = 100 };

        _productRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);
        _orderFactoryMock.Setup(factory => factory.Create(createOrderDto, It.IsAny<IEnumerable<Product>>())).Returns(order);
        _unitOfWorkMock.Setup(uow => uow.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _orderService.CreateOrderAsync(createOrderDto);

        // Assert
        _orderRepositoryMock.Verify(repo => repo.AddAsync(order), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task CreateOrder_ThrowsException_WhenProductIsOutOfStock()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto
        {
            CustomerName = "Test Customer",
            OrderItems = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 1, Quantity = 20 }
            }
        };

        var products = new List<Product> { new Product { Id = 1, Name = "Test Product", Price = 100, Stock = 10 } };
        _productRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(createOrderDto));
        Assert.Equal("No hay suficiente Stock para este producto.", exception.Message);
    }

    [Fact]
    public async Task CancelOrder_Successful()
    {
        // Arrange
        var orderId = 1;
        var product = new Product { Id = 1, Name = "Test Product", Stock = 5 };
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Pending,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2, Product = product }
            }
        };

        _orderRepositoryMock.Setup(repo => repo.GetOrderByIdWithItemsAsync(orderId)).ReturnsAsync(order);
        _productRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

        // Act
        await _orderService.CancelOrderAsync(orderId);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal(7, product.Stock); // 5 (initial) + 2 (returned)
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetOrderById_ReturnsOrder()
    {
        // Arrange
        var orderId = 1;
        var expectedOrder = new Order { Id = orderId, CustomerName = "Test Customer" };
        _orderRepositoryMock.Setup(repo => repo.GetOrderByIdWithItemsAsync(orderId)).ReturnsAsync(expectedOrder);

        // Act
        var result = await _orderService.GetOrderByIdAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public void Should_Have_Error_When_CustomerName_Is_Null()
    {
        // Arrange
        var dto = new CreateOrderDto { CustomerName = null };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(order => order.CustomerName);
    }
}