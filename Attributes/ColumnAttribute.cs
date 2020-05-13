using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 列名称
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;
        /// <summary>
        /// 字段说明
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// 是否是主键
        /// </summary>
        public bool IsPrimaryKey { get; set; } = false;


        ///// <summary>
        ///// 主键是否自动增长
        ///// </summary>
        //public bool IsPrimaryKeyAuto { get; set; }
        ///// <summary>
        ///// 数据类型
        ///// </summary>
        //public string DbDataType { get; set; }
        ///// <summary>
        ///// 字符串最大长度
        ///// </summary>
        //public int MaxLength { get; set; }
        ///// <summary>
        ///// 是否不可为空
        ///// </summary>
        //public bool NotNull { get; set; }

        /// <summary>
        /// 添加时 是否忽略
        /// </summary>
        public bool IsAddIgnore { get; set; } = false;

        /// <summary>
        /// 是否忽略
        /// </summary>
        public bool IsIgnore { get; set; } = false;
    }
}
