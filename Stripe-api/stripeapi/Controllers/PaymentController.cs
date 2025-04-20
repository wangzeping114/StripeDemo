using Microsoft.AspNetCore.Mvc;
using Stripe;
using stripeapi.Dtos;

namespace stripeapi.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentCreateRequest request)
        {
            var options = new PaymentIntentCreateOptions
            {
                // 支付金额，单位为最小货币单位（如：分，美元是“美分”） 比如美元的 10.00 就是 1000（美分）
                Amount = request.Amount,
                // 货币类型，必须是 Stripe 支持的 ISO 货币代码，如 "usd"、"eur"、"cny" 等
                Currency = request.Currency,
                // 可选：支付描述
                Description = request.Description,
                // 可选：用于绑定某个 Stripe customer（客户）
                Customer = request.CustomerId,
                // 可选：是否立即确认（推荐设置为 false，前端确认）
                Confirm = request.Confirm,
                // 可选：元数据（推荐传订单号、用户ID等业务关键信息）
                Metadata = request.Metadata,
                // 支持的支付方式类型，最常见的是卡支付（card）
                PaymentMethodTypes = new List<string>
                {
                    "card",
                },
                // 可选：设置是否自动捕获（如果为 false，可用于授权后再手动扣款）
                CaptureMethod = "automatic",// "automatic" 或 "manual"

                // 可选：成功后回调地址（通常用于移动端/重定向场景）
                //ReturnUrl = "https://your-site.com/checkout/success",

            };
            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(new
            {
                clientSecret = paymentIntent.ClientSecret,
                paymentIntentId = paymentIntent.Id
            });
        }

        [HttpPost("update-order-status")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderRequst dto) 
        {
            var PaymentIntent= dto.PaymentIntentId;
            //if (string.IsNullOrWhiteSpace(dto.OrderId))
            //    return BadRequest("订单编号不能为空");

            // 示例：更新订单状态
            //var order = await _orderRepository.FindAsync(dto.OrderId);
            //if (order == null) return NotFound("订单不存在");

            //order.PaymentStatus = dto.PaymentStatus;
            //order.StripePaymentIntentId = dto.PaymentIntentId;
            //order.PaidAt = dto.PaidAt ?? DateTime.UtcNow;

            //await _orderRepository.UpdateAsync(order);

            return Ok(new { success = true });
        }
    }
}
