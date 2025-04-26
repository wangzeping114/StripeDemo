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
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 查询充值记录
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns>充值记录列表</returns>
        [HttpPost("deposits")]
        public async Task<ActionResult<PagedResponse<DepositRecordDto>>> GetDepositRecords(DepositRecordRequest request)
        {
            // 参数验证
            if (request.StartTime.HasValue && request.EndTime.HasValue && request.EndTime < request.StartTime)
            {
                return BadRequest("结束时间不能早于开始时间");
            }

            // 确保页码和页大小有效
            if (request.PageIndex < 1)
            {
                request.PageIndex = 1;
            }
            if (request.PageSize < 1)
            {
                request.PageSize = 20;
            }

            // 查询充值记录
            var query = _context.DepositRecords.AsQueryable();

            // 根据请求参数过滤
            // 用户ID过滤
            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(d => d.UserId == request.UserId);
            }

            // 币种过滤
            if (!string.IsNullOrEmpty(request.CurrencyCode))
            {
                query = query.Where(d => d.CurrencyCode == request.CurrencyCode);
            }

            // 充值方式过滤
            if (!string.IsNullOrEmpty(request.DepositMethod))
            {
                query = query.Where(d => d.DepositMethod == request.DepositMethod);
            }

            // 状态过滤
            if (request.Status.HasValue)
            {
                query = query.Where(d => (int)d.Status == request.Status.Value);
            }

            // 交易流水号过滤
            if (!string.IsNullOrEmpty(request.TransactionId))
            {
                query = query.Where(d => d.TransactionId.Contains(request.TransactionId));
            }

            // 时间范围过滤 - 使用ConfirmedTime如果查询充值成功的记录，否则使用RequestedTime
            if (request.StartTime.HasValue)
            {
                // 如果是查询充值成功的记录，使用到账时间过滤，否则使用请求时间
                if (request.Status == (int)DepositStatus.Success)
                {
                    query = query.Where(d => d.ConfirmedTime >= request.StartTime);
                }
                else
                {
                    query = query.Where(d => d.RequestedTime >= request.StartTime);
                }
            }

            if (request.EndTime.HasValue)
            {
                // 如果是查询充值成功的记录，使用到账时间过滤，否则使用请求时间
                if (request.Status == (int)DepositStatus.Success)
                {
                    query = query.Where(d => d.ConfirmedTime <= request.EndTime);
                }
                else
                {
                    query = query.Where(d => d.RequestedTime <= request.EndTime);
                }
            }

            // 计算总记录数
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // 排序 - 按时间倒序
            query = query.OrderByDescending(d => d.CreateTime);

            // 应用分页
            var pagedRecords = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // 转换为DTO
            var dtoList = pagedRecords.Select(DepositRecordDto.FromEntity).ToList();

            // 返回分页结果
            return new PagedResponse<DepositRecordDto>
            {
                Items = dtoList,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// 查询提现记录
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns>提现记录列表</returns>
        [HttpPost("withdraws")]
        public async Task<ActionResult<PagedResponse<WithdrawRecordDto>>> GetWithdrawRecords(WithdrawRecordRequest request)
        {
            // 参数验证
            if (request.StartTime.HasValue && request.EndTime.HasValue && request.EndTime < request.StartTime)
            {
                return BadRequest("结束时间不能早于开始时间");
            }

            // 确保页码和页大小有效
            if (request.PageIndex < 1)
            {
                request.PageIndex = 1;
            }
            if (request.PageSize < 1)
            {
                request.PageSize = 20;
            }

            // 查询提现记录
            var query = _context.WithdrawRecords.AsQueryable();

            // 根据请求参数过滤
            // 用户ID过滤
            if (!string.IsNullOrEmpty(request.UserId))
            {
                query = query.Where(w => w.UserId == request.UserId);
            }

            // 币种过滤
            if (!string.IsNullOrEmpty(request.CurrencyCode))
            {
                query = query.Where(w => w.CurrencyCode == request.CurrencyCode);
            }

            // 提现方式过滤
            if (!string.IsNullOrEmpty(request.WithdrawMethod))
            {
                query = query.Where(w => w.WithdrawMethod == request.WithdrawMethod);
            }

            // 状态过滤
            if (request.Status.HasValue)
            {
                query = query.Where(w => (int)w.Status == request.Status.Value);
            }

            // 交易流水号过滤
            if (!string.IsNullOrEmpty(request.TransactionId))
            {
                query = query.Where(w => w.TransactionId.Contains(request.TransactionId));
            }

            // 时间范围过滤 - 使用ProcessedTime如果查询提现成功的记录，否则使用RequestedTime
            if (request.StartTime.HasValue)
            {
                // 如果是查询提现成功的记录，使用处理时间过滤，否则使用请求时间
                if (request.Status == (int)WithdrawStatus.Success)
                {
                    query = query.Where(w => w.ProcessedTime >= request.StartTime);
                }
                else
                {
                    query = query.Where(w => w.RequestedTime >= request.StartTime);
                }
            }

            if (request.EndTime.HasValue)
            {
                // 如果是查询提现成功的记录，使用处理时间过滤，否则使用请求时间
                if (request.Status == (int)WithdrawStatus.Success)
                {
                    query = query.Where(w => w.ProcessedTime <= request.EndTime);
                }
                else
                {
                    query = query.Where(w => w.RequestedTime <= request.EndTime);
                }
            }

            // 计算总记录数
            int totalCount = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            // 排序 - 按时间倒序
            query = query.OrderByDescending(w => w.CreateTime);

            // 应用分页
            var pagedRecords = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            // 转换为DTO
            var dtoList = pagedRecords.Select(WithdrawRecordDto.FromEntity).ToList();

            // 返回分页结果
            return new PagedResponse<WithdrawRecordDto>
            {
                Items = dtoList,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }
    }
} 