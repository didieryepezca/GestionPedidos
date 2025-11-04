namespace GestionPedidosDY.Application.Contracts
{
    public interface IMessagePublisher
    {
        void Publish(string message);
    }
}