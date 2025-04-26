using System;
using stripeapi.Entity;

namespace stripeapi.Dtos
{
    /// <summary>
    /// 提现记录DTO
    /// </summary>
    public class WithdrawRecordDto
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
        /// 提现金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 币种
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// 提现方式
        /// </summary>
        public string WithdrawMethod { get; set; }

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
        /// 提交时间
        /// </summary>
        public DateTime RequestedTime { get; set; }

        /// <summary>
        /// 处理完成时间
        /// </summary>
        public DateTime? ProcessedTime { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 从实体转换为DTO
        /// </summary>
        public static WithdrawRecordDto FromEntity(WithdrawRecord entity)
        {
            if (entity == null) return null;

            return new WithdrawRecordDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Amount = entity.Amount,
                CurrencyCode = entity.CurrencyCode,
                WithdrawMethod = entity.WithdrawMethod,
                TransactionId = entity.TransactionId,
                StatusValue = (int)entity.Status,
                StatusDescription = GetStatusDescription(entity.Status),
                RequestedTime = entity.RequestedTime,
                ProcessedTime = entity.ProcessedTime,
                Channel = entity.Channel,
                IpAddress = entity.IpAddress,
                ErrorMessage = entity.ErrorMessage,
                CompletedAt = entity.CompletedAt,
                CreateTime = entity.CreateTime
            };
        }

        /// <summary>
        /// 获取状态描述
        /// </summary>
        private static string GetStatusDescription(WithdrawStatus status)
        {
            return status switch
            {
                WithdrawStatus.Pending => "待审核",
                WithdrawStatus.Success => "成功",
                WithdrawStatus.Failed => "失败",
                WithdrawStatus.Rejected => "已驳回",
                _ => "未知状态"
            };
        }
    }
} 