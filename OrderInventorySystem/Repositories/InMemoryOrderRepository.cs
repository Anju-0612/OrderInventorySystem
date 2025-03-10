using OrderInventorySystem.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderInventorySystem.Repositories
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new List<Order>();
        private readonly object _lock = new object();

        public Task<Order> GetByIdAsync(int id)
        {
            lock (_lock)
            {
                return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
            }
        }

        public Task AddAsync(Order order)
        {
            lock (_lock)
            {
                order.Id = _orders.Any() ? _orders.Max(o => o.Id) + 1 : 1;
                _orders.Add(order);
                return Task.CompletedTask;
            }
        }

        public Task UpdateAsync(Order order)
        {
            lock (_lock)
            {
                var existing = _orders.FirstOrDefault(o => o.Id == order.Id);
                if (existing != null)
                {
                    existing.Items = order.Items;
                    existing.Status = order.Status;
                    existing.CreatedAt = order.CreatedAt;
                }
                return Task.CompletedTask;
            }
        }

        public Task DeleteAsync(int id)
        {
            lock (_lock)
            {
                var order = _orders.FirstOrDefault(o => o.Id == id);
                if (order != null)
                {
                    _orders.Remove(order);
                }
                return Task.CompletedTask;
            }
        }
    }
}