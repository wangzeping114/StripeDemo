namespace stripeapi.Dtos
{
    /// <summary>
    /// Stripe Connect Custom账户创建请求
    /// </summary>
    public class ConnectAccountCreateRequest
    {
        /// <summary>
        /// 账户类型，对于Custom模式应为"custom"
        /// </summary>
        public string Type { get; set; } = "custom";

        /// <summary>
        /// 账户国家/地区（两位ISO国家代码）
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 账户电子邮件
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 自定义账户ID，通常是你系统中的用户ID
        /// </summary>
        public string? BusinessId { get; set; }

        /// <summary>
        /// 业务名称
        /// </summary>
        public string? BusinessName { get; set; }

        /// <summary>
        /// 业务网站
        /// </summary>
        public string? BusinessUrl { get; set; }

        /// <summary>
        /// 支持的货币，默认为USD
        /// </summary>
        public string DefaultCurrency { get; set; } = "usd";

        /// <summary>
        /// 是否接受Stripe服务条款
        /// </summary>
        public bool TosAcceptance { get; set; } = true;

        /// <summary>
        /// 元数据，可以用于自定义追踪
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }
    }
} 