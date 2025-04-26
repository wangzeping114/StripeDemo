using System.Collections.Generic;

namespace stripeapi.Dtos
{
    /// <summary>
    /// 通用分页响应
    /// </summary>
    /// <typeparam name="T">数据项类型</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// 数据项列表
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }
    }
} 