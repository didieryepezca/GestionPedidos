using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace GestionPedidosDY
{
    public class OrderProcessingBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<OrderProcessingBackgroundService> _logger;
        private readonly IConfiguration _configuration;
        private IConnection _connection;        

        public OrderProcessingBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<OrderProcessingBackgroundService> logger, IConfiguration configuration)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => _logger.LogInformation("Otro proceso de servicio se está deteniendo..."));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        var factory = new ConnectionFactory()
                        {
                            HostName = _configuration["RabbitMq:HostName"],
                            UserName = _configuration["RabbitMq:UserName"],
                            Password = _configuration["RabbitMq:Password"],                            
                        };
                        using var _connection = await factory.CreateConnectionAsync();
                        using var _channel = await _connection.CreateChannelAsync();

                        await _channel.QueueDeclareAsync(queue: "order-created-queue",
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        var consumer = new AsyncEventingBasicConsumer(_channel);
                        consumer.ReceivedAsync += async (sender, eventArgs) =>
                        {
                            byte[] body = eventArgs.Body.ToArray();
                            string message = Encoding.UTF8.GetString(body);

                            _logger.LogInformation(message);
                            await OnMessageReceived(eventArgs);

                            await ((AsyncEventingBasicConsumer)sender).Channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
                        };                      
                        _logger.LogInformation("Conectado a RabbitMQ y el queue.");

                        await _channel.BasicConsumeAsync("order-created-queue", autoAck: false, consumer);
                    }                          

                    // Keep the service alive
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not connect to RabbitMQ. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task OnMessageReceived(BasicDeliverEventArgs ea)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var order = JsonSerializer.Deserialize<Order>(message);

                _logger.LogInformation($"Received order {order.Id}. Sending confirmation email.");

                await emailService.SendOrderConfirmationEmailAsync(order);

                _logger.LogInformation($"Confirmation email for order {order.Id} sent.");

                //_channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message.");
                // Acknowledge the message to prevent it from being re-queued
                //channel.BasicAck(ea.DeliveryTag, false);
            }
        }

        //public override void Dispose()
        //{
        //    _channel?.Close();
        //    _connection?.Close();
        //    base.Dispose();
        //}
    }
}