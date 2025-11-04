using Microsoft.AspNetCore.Http;

namespace GestionPedidosDY.Application.Middleware.Errors
{
    public class NotFoundException(string Message) : ServiceException(StatusCodes.Status404NotFound, Message)
    {
    }
}
