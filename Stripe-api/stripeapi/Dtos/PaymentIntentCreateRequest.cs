namespace stripeapi.Dtos
{
    public class PaymentIntentCreateRequest
    {
        /// <summary>
        /// 支付金额，单位为最小货币单位（如：分，美元是“美分”）
        /// 例如 10.00 USD => 1000
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 货币类型，必须是 Stripe 支持的 ISO 货币代码，如 "usd"、"eur"、"cny" 等
        /// </summary>
        public string Currency { get; set; } = "usd";

        /// <summary>
        /// 可选：支付描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 可选：客户ID（Stripe 的 customer ID，如果你有创建客户的话）
        /// </summary>
        public string? CustomerId { get; set; }

        /// <summary>
        /// 可选：是否自动确认
        /// </summary>
        public bool? Confirm { get; set; } = false;

        /// <summary>
        /// 可选：元数据，可以用于自定义追踪（如订单号）
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
