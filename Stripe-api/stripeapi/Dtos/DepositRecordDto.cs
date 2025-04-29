using System;
using stripeapi.Entity;

namespace stripeapi.Dtos
{
    /// <summary>
    /// 充值记录DTO
    /// </summary>
    public class DepositRecordDto
    {
        /// <summary>
        /// 记录ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 充值金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 充值方式
        /// </summary>
        public string DepositMethod { get; set; }

        /// <summary>
        /// 交易流水号
        /// </summary>
        public string TransactionId { get; set; }

        /// <summary>
        /// 状态值
        /// </summary>
        public int StatusValue { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// 发起充值时间
        /// </summary>
        public DateTime? RequestedTime { get; set; }

        /// <summary>
        /// 到账确认时间
        /// </summary>
        public DateTime? ConfirmedTime { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 从实体转换为DTO
        /// </summary>
        public static DepositRecordDto FromEntity(DepositRecord entity)
        {
            if (entity == null) return null;

            return new DepositRecordDto
            {
                Id = entity.ID,
                UserId = entity.UserId,
                Amount = entity.Amount,
                CurrencyCode = entity.CurrencyCode,
                DepositMethod = entity.DepositMethod,
                TransactionId = entity.TransactionId,
                StatusValue = (int)entity.Status,
                StatusDescription = GetStatusDescription(entity.Status),
                RequestedTime = entity.RequestedTime,
                ConfirmedTime = entity.ConfirmedTime,
                Operator = entity.Operator,
                IpAddress = entity.IpAddress,
                Channel = entity.Channel,
                Remark = entity.Remark,
                CreateTime = entity.CreateTime.Value
            };
        }

        /// <summary>
        /// 获取状态描述
        /// </summary>
        private static string GetStatusDescription(DepositStatus status)
        {
            return status switch
            {
                DepositStatus.Pending => "待处理",
                DepositStatus.Success => "成功",
                DepositStatus.Failed => "失败",
                DepositStatus.Cancelled => "已取消",
                _ => "未知状态"
            };
        }
    }
} 