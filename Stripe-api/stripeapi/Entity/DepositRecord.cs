using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stripeapi.Entity
{
    /// <summary>
    /// 充值记录明细表
    /// </summary>
    [Table("DepositRecords")]
    public class DepositRecord : BasePoco
    {
        /// <summary>
        /// 外键：对应钱包主键
        /// </summary>
        public Guid ProfitAccountId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// 币种（如：CNY, USD）
        /// </summary>
        [MaxLength(10)]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 充值方式（如：Skrill, Alipay）
        /// </summary>
        [MaxLength(50)]
        public string DepositMethod { get; set; }

        /// <summary>
        /// 第三方交易流水号
        /// </summary>
        [MaxLength(100)]
        public string TransactionId { get; set; }

        /// <summary>
        /// 状态（0待处理，1成功，2失败，3取消）
        /// </summary>
        public DepositStatus Status { get; set; }

        /// <summary>
        /// 发起充值时间
        /// </summary>
        public DateTime? RequestedTime { get; set; }

        /// <summary>
        /// 实际到账或确认时间
        /// </summary>
        public DateTime? ConfirmedTime { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        [MaxLength(50)]
        public string Operator { get; set; }

        /// <summary>
        /// 用户IP地址
        /// </summary>
        [MaxLength(45)]
        public string IpAddress { get; set; }

        /// <summary>
        /// 来源渠道（如 Web、App）
        /// </summary>
        [MaxLength(50)]
        public string Channel { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
    /// <summary>
    /// 充值状态枚举
    /// </summary>
    public enum DepositStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Cancelled = 3
    }
}
