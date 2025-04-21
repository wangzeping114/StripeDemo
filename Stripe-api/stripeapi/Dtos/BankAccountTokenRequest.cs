namespace stripeapi.Dtos
{
    /// <summary>
    /// 银行账户Token创建请求
    /// </summary>
    public class BankAccountTokenRequest
    {
        /// <summary>
        /// 银行账户所在国家/地区（两位ISO国家代码）
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// 货币类型
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 账户号码
        /// </summary>
        public string AccountNumber { get; set; }

        /// <summary>
        /// 银行路由号码/分行代码（美国为9位数字）
        /// </summary>
        public string? RoutingNumber { get; set; }

        /// <summary>
        /// 账户持有人姓名
        /// </summary>
        public string? AccountHolderName { get; set; }

        /// <summary>
        /// 账户持有人类型："individual"(个人) 或 "company"(公司)
        /// </summary>
        public string AccountHolderType { get; set; } = "individual";
    }
} 