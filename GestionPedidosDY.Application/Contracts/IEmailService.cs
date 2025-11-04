using GestionPedidosDY.Core;
using System.Threading.Tasks;

namespace GestionPedidosDY.Application.Contracts
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmailAsync(Order order);
    }
}