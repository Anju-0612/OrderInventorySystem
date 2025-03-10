using OrderInventorySystem.Models;
using System.Threading.Tasks;

namespace OrderInventorySystem.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> GetByIdAsync(int id);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
    }
}