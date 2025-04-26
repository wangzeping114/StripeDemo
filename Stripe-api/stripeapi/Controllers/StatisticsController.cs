using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stripeapi.Dtos;
using stripeapi.Entity;

namespace stripeapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 提现充值统计查询接口
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns>统计结果</returns>
        [HttpPost("transactions")]
        public async Task<ActionResult<TransactionStatisticsResponse>> GetTransactionStatistics(TransactionStatisticsRequest request)
        {
            // 参数验证
            if (request.EndTime < request.StartTime)
            {
                return BadRequest("结束时间不能早于开始时间");
            }

            // 查询充值记录
            var depositQuery = _context.DepositRecords.AsQueryable();
            // 查询提现记录
            var withdrawQuery = _context.WithdrawRecords.AsQueryable();

            // 根据请求参数过滤
            // 时间范围过滤
            depositQuery = depositQuery.Where(d => d.ConfirmedTime >= request.StartTime && d.ConfirmedTime <= request.EndTime);
            withdrawQuery = withdrawQuery.Where(w => w.ProcessedTime >= request.StartTime && w.ProcessedTime <= request.EndTime);
            
            // 只统计成功的交易
            depositQuery = depositQuery.Where(d => d.Status == DepositStatus.Success);
            withdrawQuery = withdrawQuery.Where(w => w.Status == WithdrawStatus.Success);

            // 用户ID过滤（如果提供）
            if (!string.IsNullOrEmpty(request.UserId))
            {
                depositQuery = depositQuery.Where(d => d.UserId == request.UserId);
                withdrawQuery = withdrawQuery.Where(w => w.UserId == request.UserId);
            }

            // 币种过滤（如果提供）
            if (!string.IsNullOrEmpty(request.CurrencyCode))
            {
                depositQuery = depositQuery.Where(d => d.CurrencyCode == request.CurrencyCode);
                withdrawQuery = withdrawQuery.Where(w => w.CurrencyCode == request.CurrencyCode);
            }

            // 执行查询获取原始数据
            var deposits = await depositQuery.ToListAsync();
            var withdraws = await withdrawQuery.ToListAsync();

            // 按照时间粒度对数据进行分组汇总
            var response = new TransactionStatisticsResponse();
            
            // 分组统计
            var depositGroups = GroupByPeriod(deposits, request.Granularity);
            var withdrawGroups = GroupByPeriod(withdraws, request.Granularity);

            // 合并充值和提现的统计结果
            var allPeriods = depositGroups.Keys.Union(withdrawGroups.Keys).OrderBy(p => p).ToList();
            
            foreach (var period in allPeriods)
            {
                var statisticsItem = new TransactionStatisticsItem
                {
                    Period = period
                };

                // 添加充值数据
                if (depositGroups.TryGetValue(period, out var depositGroup))
                {
                    statisticsItem.TotalDeposit = depositGroup.Sum(d => d.Amount);
                    statisticsItem.DepositCount = depositGroup.Count;
                    // 计算最大单笔充值
                    statisticsItem.MaxDepositAmount = depositGroup.Count > 0 ? depositGroup.Max(d => d.Amount) : 0;
                }

                // 添加提现数据
                if (withdrawGroups.TryGetValue(period, out var withdrawGroup))
                {
                    statisticsItem.TotalWithdraw = withdrawGroup.Sum(w => w.Amount);
                    statisticsItem.WithdrawCount = withdrawGroup.Count;
                    // 计算最大单笔提现
                    statisticsItem.MaxWithdrawAmount = withdrawGroup.Count > 0 ? withdrawGroup.Max(w => w.Amount) : 0;
                }

                // 计算净收入
                statisticsItem.NetIncome = statisticsItem.TotalDeposit - statisticsItem.TotalWithdraw;

                response.Items.Add(statisticsItem);
            }

            // 计算汇总信息
            var maxDepositRecord = deposits.Count > 0 ? deposits.OrderByDescending(d => d.Amount).FirstOrDefault() : null;
            var maxWithdrawRecord = withdraws.Count > 0 ? withdraws.OrderByDescending(w => w.Amount).FirstOrDefault() : null;

            response.Summary = new TransactionSummary
            {
                TotalDeposit = deposits.Sum(d => d.Amount),
                TotalDepositCount = deposits.Count,
                MaxDepositAmount = maxDepositRecord?.Amount ?? 0,
                MaxDepositTime = maxDepositRecord?.ConfirmedTime,
                MaxDepositUserId = maxDepositRecord?.UserId,
                
                TotalWithdraw = withdraws.Sum(w => w.Amount),
                TotalWithdrawCount = withdraws.Count,
                MaxWithdrawAmount = maxWithdrawRecord?.Amount ?? 0,
                MaxWithdrawTime = maxWithdrawRecord?.ProcessedTime,
                MaxWithdrawUserId = maxWithdrawRecord?.UserId,
                
                NetIncome = deposits.Sum(d => d.Amount) - withdraws.Sum(w => w.Amount)
            };

            return response;
        }

        /// <summary>
        /// 根据时间粒度对数据进行分组
        /// </summary>
        private Dictionary<string, List<T>> GroupByPeriod<T>(List<T> data, TimeGranularity granularity) where T : class
        {
            Dictionary<string, List<T>> result = new Dictionary<string, List<T>>();
            
            foreach (var item in data)
            {
                DateTime? date = null;
                
                // 根据实体类型获取对应的时间字段
                if (item is DepositRecord deposit && deposit.ConfirmedTime.HasValue)
                {
                    date = deposit.ConfirmedTime.Value;
                }
                else if (item is WithdrawRecord withdraw && withdraw.ProcessedTime.HasValue)
                {
                    date = withdraw.ProcessedTime.Value;
                }

                if (!date.HasValue)
                    continue;

                // 根据时间粒度格式化期间
                string period = FormatPeriod(date.Value, granularity);
                
                if (!result.ContainsKey(period))
                {
                    result[period] = new List<T>();
                }
                
                result[period].Add(item);
            }
            
            return result;
        }

        /// <summary>
        /// 根据时间粒度格式化时间段
        /// </summary>
        private string FormatPeriod(DateTime date, TimeGranularity granularity)
        {
            return granularity switch
            {
                TimeGranularity.Day => date.ToString("yyyy-MM-dd"),
                TimeGranularity.Month => date.ToString("yyyy-MM"),
                TimeGranularity.Year => date.ToString("yyyy"),
                _ => date.ToString("yyyy-MM-dd")
            };
        }
    }
} 