namespace stripeapi.Dtos
{
    public class UpdateOrderRequst
    {
        /// <summary>
        /// 订单编号（你自己系统内的订单号）
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// 支付状态（如："paid"、"failed"、"refunded" 等）
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Stripe 生成的支付意图 ID，用于对账
        /// </summary>
        public string PaymentIntentId { get; set; }

        /// <summary>
        /// 实际支付金额（单位：分、元、美元，视业务而定）
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 付款成功时间（可选）
        /// </summary>
        public DateTime? PaidAt { get; set; }
    }
}