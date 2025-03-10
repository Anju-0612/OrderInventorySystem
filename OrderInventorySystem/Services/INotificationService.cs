using System.Threading.Tasks;

namespace OrderInventorySystem.Services
{
    public interface INotificationService
    {
        Task SendOrderFulfilledNotificationAsync(int orderId);
    }
}