using GestionPedidosDY.Application.Contracts;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace GestionPedidosDY.Infraestructure.Services
{
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly IConfiguration _configuration;        

        public RabbitMqPublisher(IConfiguration configuration)
        {
            _configuration = configuration;           
        }

        public async void Publish(string message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMq:HostName"],
                UserName = _configuration["RabbitMq:UserName"],
                Password = _configuration["RabbitMq:Password"],
            };
            using var _connection = await factory.CreateConnectionAsync();
            using var _channel = await _connection.CreateChannelAsync();

            var body = Encoding.UTF8.GetBytes(message);

            await _channel.QueueDeclareAsync(
                queue: "order-created-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            await _channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "order_created_queue",
                mandatory: true,
                basicProperties: new BasicProperties { Persistent = true },
                body: body
                );
        }
    }
}