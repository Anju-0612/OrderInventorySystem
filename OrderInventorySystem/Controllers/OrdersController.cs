using Microsoft.AspNetCore.Mvc;
using OrderInventorySystem.Models;
using OrderInventorySystem.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderInventorySystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] List<OrderItem> items)
        {
            try
            {
                var orderId = await _orderService.PlaceOrderAsync(items);
                return Ok(new { OrderId = orderId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}