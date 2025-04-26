using System;
using System.Collections.Generic;

namespace stripeapi.Dtos
{
    /// <summary>
    /// 提现充值统计查询响应
    /// </summary>
    public class TransactionStatisticsResponse
    {
        /// <summary>
        /// 统计结果列表
        /// </summary>
        public List<TransactionStatisticsItem> Items { get; set; } = new List<TransactionStatisticsItem>();

        /// <summary>
        /// 汇总信息
        /// </summary>
        public TransactionSummary Summary { get; set; } = new TransactionSummary();
    }

    /// <summary>
    /// 交易统计项
    /// </summary>
    public class TransactionStatisticsItem
    {
        /// <summary>
        /// 统计日期（天/月/年）
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// 充值总额
        /// </summary>
        public decimal TotalDeposit { get; set; }

        /// <summary>
        /// 充值成功笔数
        /// </summary>
        public int DepositCount { get; set; }

        /// <summary>
        /// 最大单笔充值金额
        /// </summary>
        public decimal MaxDepositAmount { get; set; }

        /// <summary>
        /// 提现总额
        /// </summary>
        public decimal TotalWithdraw { get; set; }

        /// <summary>
        /// 提现成功笔数
        /// </summary>
        public int WithdrawCount { get; set; }

        /// <summary>
        /// 最大单笔提现金额
        /// </summary>
        public decimal MaxWithdrawAmount { get; set; }

        /// <summary>
        /// 净收入（充值 - 提现）
        /// </summary>
        public decimal NetIncome { get; set; }
    }

    /// <summary>
    /// 交易汇总信息
    /// </summary>
    public class TransactionSummary
    {
        /// <summary>
        /// 充值总额
        /// </summary>
        public decimal TotalDeposit { get; set; }

        /// <summary>
        /// 充值总笔数
        /// </summary>
        public int TotalDepositCount { get; set; }

        /// <summary>
        /// 最大单笔充值金额
        /// </summary>
        public decimal MaxDepositAmount { get; set; }

        /// <summary>
        /// 最大单笔充值时间
        /// </summary>
        public DateTime? MaxDepositTime { get; set; }

        /// <summary>
        /// 最大单笔充值用户ID
        /// </summary>
        public string MaxDepositUserId { get; set; }

        /// <summary>
        /// 提现总额
        /// </summary>
        public decimal TotalWithdraw { get; set; }

        /// <summary>
        /// 提现总笔数
        /// </summary>
        public int TotalWithdrawCount { get; set; }

        /// <summary>
        /// 最大单笔提现金额
        /// </summary>
        public decimal MaxWithdrawAmount { get; set; }

        /// <summary>
        /// 最大单笔提现时间
        /// </summary>
        public DateTime? MaxWithdrawTime { get; set; }

        /// <summary>
        /// 最大单笔提现用户ID
        /// </summary>
        public string MaxWithdrawUserId { get; set; }

        /// <summary>
        /// 净收入（充值 - 提现）
        /// </summary>
        public decimal NetIncome { get; set; }
    }
} 