using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper
{
    public class Paged<T> where T : new()
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// 当前页记录列表
        /// </summary>
        public List<T> Items { get; set; }
    }
    
    
}
