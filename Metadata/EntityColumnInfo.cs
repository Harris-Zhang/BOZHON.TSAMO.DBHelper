using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Metadata
{
    public class EntityColumnInfo
    { 
        /// <summary>
        /// 字段名称
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 是否主键
        /// </summary>
        public bool IsPrimaryKey { get; set; }
        /// <summary>
        /// 是否忽视
        /// </summary>
        public bool IsIgnore { get; set; }
        /// <summary>
        /// 是否添加时忽视
        /// </summary>
        public bool IsAddIgnore { get; set; }

    }
}
