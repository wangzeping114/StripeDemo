namespace stripeapi.Dtos
{
    public class PayoutCreateRequest
    {
        /// <summary>
        /// 提现金额，单位为最小货币单位（如：分，美元是"美分"）
        /// 例如 10.00 USD => 1000
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 货币类型，必须是 Stripe 支持的 ISO 货币代码，如 "usd"、"eur"、"cny" 等
        /// </summary>
        public string Currency { get; set; } = "usd";

        /// <summary>
        /// 可选：提现描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 可选：目标银行账户ID（Stripe的connected account或external account ID）
        /// </summary>
        public string? Destination { get; set; }

        /// <summary>
        /// 可选：提现方式，例如："instant"（即时）或"standard"（标准）
        /// </summary>
        public string? Method { get; set; } = "standard";

        /// <summary>
        /// 可选：元数据，可以用于自定义追踪
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// 可选：来源类型，例如："bank_account"或"card"
        /// </summary>
        public string? SourceType { get; set; } = "bank_account";

        /// <summary>
        /// 可选：声明提现资金的用途
        /// </summary>
        public string? StatementDescriptor { get; set; }
    }
} 