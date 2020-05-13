using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Attributes
{
    /// <summary>
    /// 表实体特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// 数据表名称
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// 注释
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
