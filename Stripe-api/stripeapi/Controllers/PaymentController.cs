using Microsoft.AspNetCore.Mvc;
using Stripe;
using stripeapi.Dtos;

namespace stripeapi.Controllers
{
    /// <summary>
    /// 提供Stripe支付和提现相关的API接口
    /// </summary>
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        /// <summary>
        /// 创建Stripe支付意向
        /// </summary>
        /// <param name="request">支付意向创建请求</param>
        /// <returns>包含clientSecret和paymentIntentId的响应</returns>
        /// <remarks>
        /// 该API用于创建支付意向，客户端可以使用返回的clientSecret完成支付流程。
        /// 支持的金额单位为最小货币单位（如：分，美元是"美分"）。
        /// </remarks>
        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentCreateRequest request)
        {
            var options = new PaymentIntentCreateOptions
            {
                // 支付金额，单位为最小货币单位（如：分，美元是"美分"） 比如美元的 10.00 就是 1000（美分）
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

        /// <summary>
        /// 更新订单支付状态
        /// </summary>
        /// <param name="dto">订单更新请求</param>
        /// <returns>更新成功状态</returns>
        /// <remarks>
        /// 该API用于在支付完成后更新订单状态。
        /// 在实际应用中，需要根据支付意向ID查询并更新相应的订单记录。
        /// </remarks>
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

        /// <summary>
        /// 创建Stripe提现请求
        /// </summary>
        /// <param name="request">提现创建请求</param>
        /// <returns>提现详情，包含ID、金额、状态等信息</returns>
        /// <remarks>
        /// 该API用于创建提现请求，将资金从Stripe账户提现到指定的银行账户。
        /// 如需为用户提现，需要先设置Stripe Connect并创建关联账户。
        /// 支持的金额单位为最小货币单位（如：分，美元是"美分"）。
        /// </remarks>
        [HttpPost("create-payout")]
        public async Task<IActionResult> CreatePayout([FromBody] PayoutCreateRequest request)
        {
            try
            {
                var options = new PayoutCreateOptions
                {
                    // 提现金额，单位为最小货币单位
                    Amount = request.Amount,
                    // 货币类型，必须是 Stripe 支持的 ISO 货币代码
                    Currency = request.Currency,
                    // 可选：提现描述
                    Description = request.Description,
                    // 可选：目标银行账户ID
                    Destination = request.Destination,
                    // 可选：提现方式
                    Method = request.Method,
                    // 可选：元数据
                    Metadata = request.Metadata,
                    // 可选：来源类型
                    SourceType = request.SourceType,
                    // 可选：声明提现资金的用途
                    StatementDescriptor = request.StatementDescriptor
                };

                var service = new PayoutService();
                var payout = await service.CreateAsync(options);

                return Ok(new
                {
                    payoutId = payout.Id,
                    amount = payout.Amount,
                    currency = payout.Currency,
                    arrivalDate = payout.ArrivalDate,
                    status = payout.Status
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }

        /// <summary>
        /// 获取提现详情
        /// </summary>
        /// <param name="payoutId">提现ID</param>
        /// <returns>提现详细信息</returns>
        /// <remarks>
        /// 该API用于查询提现请求的当前状态和详细信息。
        /// 可用于跟踪提现进度和检查是否有失败情况。
        /// </remarks>
        [HttpGet("get-payout/{payoutId}")]
        public async Task<IActionResult> GetPayout(string payoutId)
        {
            try
            {
                if (string.IsNullOrEmpty(payoutId))
                {
                    return BadRequest(new { error = "提现ID不能为空" });
                }

                var service = new PayoutService();
                var payout = await service.GetAsync(payoutId);

                return Ok(new
                {
                    payoutId = payout.Id,
                    amount = payout.Amount,
                    currency = payout.Currency,
                    arrivalDate = payout.ArrivalDate,
                    status = payout.Status,
                    created = payout.Created,
                    description = payout.Description,
                    failureCode = payout.FailureCode,
                    failureMessage = payout.FailureMessage
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }

        /// <summary>
        /// 取消待处理的提现请求
        /// </summary>
        /// <param name="payoutId">提现ID</param>
        /// <returns>取消结果</returns>
        /// <remarks>
        /// 该API用于取消尚未完成的提现请求。
        /// 注意：只有状态为pending的提现才能被取消。
        /// 已经完成或失败的提现无法取消。
        /// </remarks>
        [HttpPost("cancel-payout/{payoutId}")]
        public async Task<IActionResult> CancelPayout(string payoutId)
        {
            try
            {
                if (string.IsNullOrEmpty(payoutId))
                {
                    return BadRequest(new { error = "提现ID不能为空" });
                }

                var service = new PayoutService();
                var payout = await service.CancelAsync(payoutId);

                return Ok(new
                {
                    payoutId = payout.Id,
                    status = payout.Status,
                    canceled = true
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }

        /// <summary>
        /// 处理提现请求状态更新
        /// </summary>
        /// <param name="request">Stripe事件请求体</param>
        /// <returns>处理结果</returns>
        /// <remarks>
        /// 该API用于接收和处理Stripe的Webhook事件通知，特别是提现状态变更的事件。
        /// 需要在Stripe后台配置Webhook，URL指向该接口，并选择payout相关事件类型。
        /// 常见的事件类型包括：payout.created, payout.paid, payout.failed等。
        /// </remarks>
        [HttpPost("webhook/payout")]
        public async Task<IActionResult> HandlePayoutWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                
                // 获取Stripe签名，用于验证请求来源
                string stripeSignature = Request.Headers["Stripe-Signature"];
                
                // 替换为你在Stripe Dashboard中设置的Webhook密钥
                string webhookSecret = "whsec_your_webhook_secret_key";
                
                // 构造Stripe事件
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret
                );
                
                // 处理不同类型的提现事件
                if (stripeEvent.Type.StartsWith("payout."))
                {
                    var payout = stripeEvent.Data.Object as Payout;
                    
                    switch (stripeEvent.Type)
                    {
                        case "payout.created":
                            // 提现已创建，可以在这里更新数据库中的提现记录状态
                            // await _payoutRepository.UpdateStatusAsync(payout.Id, "created");
                            Console.WriteLine($"提现已创建: {payout.Id}, 金额: {payout.Amount / 100.0m} {payout.Currency}");
                            break;
                            
                        case "payout.paid":
                            // 提现已支付，资金已经到达用户银行账户
                            // await _payoutRepository.UpdateStatusAsync(payout.Id, "paid");
                            Console.WriteLine($"提现已完成: {payout.Id}, 到账时间: {payout.ArrivalDate}");
                            break;
                            
                        case "payout.failed":
                            // 提现失败，需要通知用户并记录失败原因
                            // await _payoutRepository.UpdateStatusAsync(payout.Id, "failed", payout.FailureMessage);
                            Console.WriteLine($"提现失败: {payout.Id}, 原因: {payout.FailureMessage}");
                            break;
                            
                        case "payout.canceled":
                            // 提现已取消
                            // await _payoutRepository.UpdateStatusAsync(payout.Id, "canceled");
                            Console.WriteLine($"提现已取消: {payout.Id}");
                            break;
                    }
                }
                
                return Ok(new { received = true });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "内部服务器错误", details = ex.Message });
            }
        }
    }
}
