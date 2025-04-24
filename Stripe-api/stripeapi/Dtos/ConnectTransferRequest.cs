namespace stripeapi.Dtos
{
    /// <summary>
    /// Stripe Connect转账请求
    /// </summary>
    public class ConnectTransferRequest
    {
        public Guid? UserId { get; set; } 
        /// <summary>
        /// 转账金额，单位为最小货币单位（如：分，美元是"美分"）
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 货币类型，必须是Stripe支持的ISO货币代码
        /// </summary>
        public string Currency { get; set; } = "usd";

        /// <summary>
        /// 目标Connect账户ID
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// 转账描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 源支付ID，如果转账关联特定支付
        /// </summary>
        public string? SourceTransaction { get; set; }

        /// <summary>
        /// 是否自动计算应用平台费用
        /// </summary>
        public bool? AutomaticApplicationFee { get; set; } = false;

        /// <summary>
        /// 平台应用费用金额（如果不自动计算）
        /// </summary>
        public long? ApplicationFeeAmount { get; set; }

        /// <summary>
        /// 元数据，可以用于自定义追踪
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// 转账组ID，用于批量转账
        /// </summary>
        public string? TransferGroup { get; set; }
    }
} 