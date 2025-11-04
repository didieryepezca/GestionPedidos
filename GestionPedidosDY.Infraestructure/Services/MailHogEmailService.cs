using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Core;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GestionPedidosDY.Infraestructure.Services
{
    public class MailHogEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public MailHogEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendOrderConfirmationEmailAsync(Order order)
        {
            var smtpClient = new SmtpClient(_configuration["Smtp:Host"], int.Parse(_configuration["Smtp:Port"]));

            var mailMessage = new MailMessage
            {
                From = new MailAddress("no-reply@pedidos.com"),
                Subject = $"Confirmación de Pedido #{order.Id}",
                Body = GetEmailBody(order),
                IsBodyHtml = true,
            };
            mailMessage.To.Add(order.CustomerEmail);

            await smtpClient.SendMailAsync(mailMessage);
        }

        private string GetEmailBody(Order order)
        {
            var html = $"""
            <html>
            <body>
                <h1>Gracias por tu pedido, {order.CustomerName}!</h1>
                <p>Tu pedido con ID #{order.Id} ha sido recibido y está siendo procesado.</p>
                <h2>Detalles del Pedido</h2>
                <p><strong>Total:</strong> {order.Total:C}</p>
            </body>
            </html>
            """;
            return html;
        }
    }
}