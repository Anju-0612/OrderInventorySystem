using OrderInventorySystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderInventorySystem.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly List<Product> _products = new List<Product>();
        private readonly object _lock = new object();

        public Task<Product> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_products.FirstOrDefault(p => p.Id == id));
            }
        }

        public Task<List<Product>> GetAllAsync()
        {
            lock (_lock)
            {
                return Task.FromResult(_products.ToList());
            }
        }

        public Task AddAsync(Product product)
        {
            lock (_lock)
            {
                product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
                _products.Add(product);
                return Task.CompletedTask;
            }
        }

        public Task UpdateAsync(Product product)
        {
            lock (_lock)
            {
                var existing = _products.FirstOrDefault(p => p.Id == product.Id);
                if (existing != null)
                {
                    existing.Name = product.Name;
                    existing.Price = product.Price;
                    existing.StockQuantity = product.StockQuantity;
                }
                return Task.CompletedTask;
            }
        }

        public Task DeleteAsync(int id)
        {
            lock (_lock)
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product != null) _products.Remove(product);
                return Task.CompletedTask;
            }
        }
    }
}