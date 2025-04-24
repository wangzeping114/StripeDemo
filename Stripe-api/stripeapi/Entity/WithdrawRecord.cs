using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stripeapi.Entity
{
    /// <summary>
    /// 提现记录
    /// </summary>
    public class WithdrawRecord : BasePoco
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        [Column]
        public string UserId { get; set; }

        /// <summary>
        /// 币种（如：USD, CNY, EUR）
        /// </summary>
        [Required]
        [Column("CurrencyCode")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 提现金额
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 提现方式（如：银行卡、Skrill、USDT）
        /// </summary>
        [Column]
        public string WithdrawMethod { get; set; }

        /// <summary>
        /// 提现渠道（如：App、Web、H5）
        /// </summary>
        [Column]
        public string Channel { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        [Column]
        public string IpAddress { get; set; }

        /// <summary>
        /// 状态（Pending=待审核，Success=成功，Failed=失败，Rejected=驳回）
        /// </summary>
        [Column]
        public WithdrawStatus Status { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        [Column]
        public DateTime RequestedTime { get; set; }

        /// <summary>
        /// 处理完成时间
        /// </summary>
        [Column]
        public DateTime? ProcessedTime { get; set; }

        /// <summary>
        /// 提现单号/交易流水号
        /// </summary>
        [Column]
        public string TransactionId { get; set; }

        /// <summary>
        /// 钱包主键（外键）
        /// </summary>
        public Guid ProfitAccountId { get; set; }
        public string PayoutId { get; set; }

        public string BankAccountId { get; set; }
        public DateTime? CompletedAt { get;  set; }
        public string ErrorMessage { get;  set; }
    }
    /// <summary>
    /// 提现状态枚举
    /// </summary>
    public enum WithdrawStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Rejected = 3
    }
}
