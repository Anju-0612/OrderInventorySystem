using OrderInventorySystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderInventorySystem.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(int id);
        Task<List<Product>> GetAllAsync();
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}