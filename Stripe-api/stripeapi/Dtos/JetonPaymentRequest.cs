namespace stripeapi.Dtos
{
    public class JetonPaymentRequest
    {
        public string MerchantId { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string OrderId { get; set; }
        public string CallbackUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string Signature { get; set; }  // 通常需要签名验证
    }
}
