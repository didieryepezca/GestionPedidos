using GestionPedidosDY.Application.Contracts;
using GestionPedidosDY.Core;
using GestionPedidosDY.Infraestructure.Data;

namespace GestionPedidosDY.Infraestructure.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
