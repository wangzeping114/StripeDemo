using System;

namespace stripeapi.Dtos
{
    /// <summary>
    /// 提现充值统计查询请求
    /// </summary>
    public class TransactionStatisticsRequest
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
        /// 开始时间（必填）
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间（必填）
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 时间粒度（日、月、年），默认日
        /// </summary>
        public TimeGranularity Granularity { get; set; } = TimeGranularity.Day;

        /// <summary>
        /// 页码，从1开始
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// 时间粒度枚举
    /// </summary>
    public enum TimeGranularity
    {
        /// <summary>
        /// 按日统计
        /// </summary>
        Day = 0,
        
        /// <summary>
        /// 按月统计
        /// </summary>
        Month = 1,
        
        /// <summary>
        /// 按年统计
        /// </summary>
        Year = 2
    }
} 