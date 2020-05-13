using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Metadata
{
    public class EntityTableInfo
    {
        /// <summary>
        /// 表名称
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 字段
        /// </summary>
        public EntityColumnInfo[] Columns { get; set; }
    }
}
