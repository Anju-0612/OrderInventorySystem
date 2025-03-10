using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using OrderInventorySystem.Models;
using OrderInventorySystem.Repositories;

namespace OrderInventorySystem.Services
{
    public class OrderService
    {
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<OrderService> _logger;
        private readonly IHostApplicationLifetime _appLifetime;
        private int _orderIdCounter = 0; // Ensure this is initialized

        public OrderService(IProductRepository productRepo, IOrderRepository orderRepo, INotificationService notificationService, ILogger<OrderService> logger, IHostApplicationLifetime appLifetime)
        {
            _productRepo = productRepo ?? throw new ArgumentNullException(nameof(productRepo));
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _appLifetime = appLifetime ?? throw new ArgumentNullException(nameof(appLifetime));
        }

        public async Task<int> PlaceOrderAsync(List<OrderItem> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null || product.StockQuantity < item.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for product {item.ProductId}");
                }
                product.StockQuantity -= item.Quantity;
                await _productRepo.UpdateAsync(product);
            }

            var order = new Order { Id = Interlocked.Increment(ref _orderIdCounter), Status = "Pending Fulfillment", Items = items };
            await _orderRepo.AddAsync(order);
            _logger.LogInformation($"Order {order.Id} placed successfully.");
            return order.Id;
        }

        public async Task CancelOrderAsync(int orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException($"Order {orderId} not found");
            }
            if (order.Status != "Pending Fulfillment")
            {
                throw new InvalidOperationException($"Cannot cancel order {orderId} with status {order.Status}");
            }

            foreach (var item in order.Items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity; // Restore stock
                    await _productRepo.UpdateAsync(product);
                }
            }

            await _orderRepo.DeleteAsync(orderId);
            _logger.LogInformation($"Order {orderId} canceled successfully.");
        }

        private async Task FulfillOrderAsync(int orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order != null && order.Status == "Pending Fulfillment")
            {
                order.Status = "Fulfilled";
                await _orderRepo.UpdateAsync(order);
                await _notificationService.SendOrderFulfilledNotificationAsync(orderId);
            }
        }
    }
}