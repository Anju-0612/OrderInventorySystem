using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OrderInventorySystem.Services
{
    public class ConsoleNotificationService : INotificationService
    {
        private readonly ILogger<ConsoleNotificationService> _logger;

        public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendOrderFulfilledNotificationAsync(int orderId)
        {
            _logger.LogInformation($"Simulated email sent: Order {orderId} has been fulfilled.");
            return Task.CompletedTask;
        }
    }
}