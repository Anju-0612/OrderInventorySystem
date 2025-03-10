using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OrderInventorySystem.Models;
using OrderInventorySystem.Repositories;
using OrderInventorySystem.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace OrderInventorySystem.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IProductRepository> _productRepoMock;
        private readonly Mock<IOrderRepository> _orderRepoMock;
        private readonly Mock<INotificationService> _notificationMock;
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly OrderService _service;

        public OrderServiceTests()
        {
            _productRepoMock = new Mock<IProductRepository>();
            _orderRepoMock = new Mock<IOrderRepository>();
            _notificationMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<OrderService>>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _service = new OrderService(
                _productRepoMock.Object,
                _orderRepoMock.Object,
                _notificationMock.Object,
                _loggerMock.Object,
                _appLifetimeMock.Object);
        }

        [Fact]
        public async Task PlaceOrderAsync_ValidOrder_ReservesStockAndPlacesOrder()
        {
            var product = new Product { Id = 1, StockQuantity = 15 };
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Callback<Product>(p => product.StockQuantity = p.StockQuantity).Returns(Task.CompletedTask);
            _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            var items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 3 } };

            var orderId = await _service.PlaceOrderAsync(items);

            Assert.True(orderId > 0);
            Assert.Equal(12, product.StockQuantity); // Should now be 12
            _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == 1 && p.StockQuantity == 12)), Times.Once());
            _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once());
        }

        [Fact]
        public async Task PlaceOrderAsync_InsufficientStock_ThrowsException()
        {
            _productRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new Product { Id = 1, StockQuantity = 1 });
            var items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 2 } };

            var exception = await Record.ExceptionAsync(() => _service.PlaceOrderAsync(items));
            Assert.NotNull(exception); // Fails here
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("Insufficient stock for product 1", exception.Message);
        }

        [Fact]
        public async Task CancelOrderAsync_PendingOrder_RestoresStockAndDeletes()
        {
            var order = new Order { Id = 1, Status = "Pending Fulfillment", Items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 3 } } };
            var product = new Product { Id = 1, StockQuantity = 12 };
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _orderRepoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

            await _service.CancelOrderAsync(1);

            Assert.Equal(15, product.StockQuantity);
            _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == 1 && p.StockQuantity == 15)), Times.Once());
            _orderRepoMock.Verify(r => r.DeleteAsync(1), Times.Once());
        }

        [Fact]
        public async Task CancelOrderAsync_FulfilledOrder_ThrowsException()
        {
            var order = new Order { Id = 1, Status = "Fulfilled", Items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 3 } } };
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelOrderAsync(1));
        }

        [Fact]
        public async Task FulfillOrderAsync_PendingOrder_UpdatesStatusAndNotifies()
        {
            var order = new Order { Id = 1, Status = "Pending Fulfillment" };
            _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
            _orderRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            _notificationMock.Setup(n => n.SendOrderFulfilledNotificationAsync(1)).Returns(Task.CompletedTask);

            var method = typeof(OrderService).GetMethod("FulfillOrderAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method == null) throw new Exception("Method not found");

            await (Task)method.Invoke(_service, new object[] { 1 });

            Assert.Equal("Fulfilled", order.Status);
            _orderRepoMock.Verify(r => r.UpdateAsync(It.Is<Order>(o => o.Status == "Fulfilled")), Times.Once());
            _notificationMock.Verify(n => n.SendOrderFulfilledNotificationAsync(1), Times.Once());
        }

        [Fact]
        public async Task PlaceOrderAsync_ConcurrentOrders_HandlesStockSafely()
        {
            var product = new Product { Id = 1, StockQuantity = 6 }; // Increase stock to allow both orders
            _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            _orderRepoMock.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);
            var items = new List<OrderItem> { new OrderItem { ProductId = 1, Quantity = 3 } };

            var task1 = _service.PlaceOrderAsync(items);
            var task2 = _service.PlaceOrderAsync(items);
            await Task.WhenAll(task1, task2);

            _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == 1 && p.StockQuantity == 0)), Times.Exactly(2)); // Both should succeed
        }
    }
}