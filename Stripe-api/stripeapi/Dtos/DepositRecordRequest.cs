using System;

namespace stripeapi.Dtos
{
    /// <summary>
    /// 充值记录查询请求
    /// </summary>
    public class DepositRecordRequest
    {
        /// <summary>
        /// 用户ID（可选，不传则查询所有用户）
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 币种（可选，如：USD, CNY, EUR）
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 充值方式（可选，如：Skrill, Alipay）
        /// </summary>
        public string DepositMethod { get; set; }

        /// <summary>
        /// 状态（可选，不传则查询所有状态）
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// 开始时间（可选）
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间（可选）
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 交易流水号（可选）
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// 页码，从1开始
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
} 