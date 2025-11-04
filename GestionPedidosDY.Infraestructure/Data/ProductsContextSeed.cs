using GestionPedidosDY.Core;
using Microsoft.Extensions.Logging;

namespace GestionPedidosDY.Infraestructure.Data
{
    public class ProductsContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger<ProductsContextSeed> logger)
        {
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { Name = "Laptop", Price = 1200, Stock = 50 },
                    new Product { Name = "Mouse", Price = 25, Stock = 200 },
                    new Product { Name = "Keyboard", Price = 75, Stock = 150 },
                    new Product { Name = "Monitor", Price = 300, Stock = 100 },
                    new Product { Name = "Webcam", Price = 50, Stock = 80 },
                    new Product { Name = "Headphones", Price = 100, Stock = 120 },
                    new Product { Name = "Microphone", Price = 80, Stock = 70 },
                    new Product { Name = "USB Hub", Price = 20, Stock = 250 },
                    new Product { Name = "Docking Station", Price = 150, Stock = 60 },
                    new Product { Name = "External Hard Drive", Price = 90, Stock = 90 },
                    new Product { Name = "Printer", Price = 200, Stock = 40 },
                    new Product { Name = "Scanner", Price = 120, Stock = 30 },
                    new Product { Name = "Projector", Price = 400, Stock = 20 },
                    new Product { Name = "Speakers", Price = 60, Stock = 180 },
                    new Product { Name = "Ethernet Cable", Price = 10, Stock = 500 },
                    new Product { Name = "Wi-Fi Router", Price = 80, Stock = 110 },
                    new Product { Name = "Power Strip", Price = 15, Stock = 300 },
                    new Product { Name = "Laptop Stand", Price = 30, Stock = 130 },
                    new Product { Name = "Mouse Pad", Price = 5, Stock = 400 },
                    new Product { Name = "Chair", Price = 250, Stock = 25 }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
                logger.LogInformation("Se agregaron productos a la BD asociada a la clase de contexto {ApplicationDbContext}", typeof(ApplicationDbContext).Name);
            }
        }
    }
}
