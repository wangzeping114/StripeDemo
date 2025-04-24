using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace stripeapi.Entity
{
    /// <summary>
    /// 权益账户（钱包）
    /// </summary>
    public class ProfitAccount : BasePoco
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [Column]
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        [Column]
        public string UserName { get; set; }

        /// <summary>
        /// 钱包数额(账户余额)
        /// </summary>
        [Column("WalletValue", TypeName = "decimal(18,2)")]
        public decimal WalletValue { set; get; }

        /// <summary>
        /// 币种（如：USD, CNY, EUR）
        /// </summary>
        [Column("CurrencyCode")]
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 预划扣的钱包消费数额
        /// </summary>
        [Column("SpendingValue", TypeName = "decimal(18,2)")]
        public decimal SpendingValue { set; get; }

        /// <summary>
        /// Vip等级
        /// </summary>
        [Column]
        public int VipLevelTier { get; set; }

        /// <summary>
        /// Vip经验值
        /// </summary>
        [Column("VipEXP", TypeName = "decimal(18,2)")]
        public decimal VipEXP { get; set; }

        /// <summary>
        /// 净预存额
        /// </summary>
        [Column("NetDepositValue", TypeName = "decimal(18,2)")]
        public decimal NetDepositValue { set; get; }

        /// <summary>
        /// 净投注额
        /// </summary>
        [Column("NetStakeValue", TypeName = "decimal(18,2)")]
        public decimal NetStakeValue { set; get; }

        /// <summary>
        /// 最大投注额度限制
        /// </summary>
        [Column("MaxStakeValue", TypeName = "decimal(18,2)")]
        public decimal MaxStakeValue { set; get; }

        /// <summary>
        /// 最大提现额度限制
        /// </summary>
        [Column("MaxCashValue", TypeName = "decimal(18,2)")]
        public decimal MaxCashValue { set; get; }

        public string ConnectAccountId { get; set; }
    }
}
