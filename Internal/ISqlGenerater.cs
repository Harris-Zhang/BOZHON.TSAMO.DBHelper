using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    public interface ISqlGenerater
    {
        /// <summary>
        /// 生成添加SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="tableName">表名</param>
        /// <param name="noColumnNames">去除的列</param>
        /// <returns></returns>
        string Insert(Type type, string tableName = null, ICollection<string> columns = null);
        /// <summary>
        /// 生成删除SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="tableName">表名称</param>
        /// <param name="primaryKeyName">条件列</param>
        /// <returns></returns>
        string Delete(Type type, string tableName, ICollection<string> primaryKeyName);
        /// <summary>
        /// 生成删除SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="where">删除条件</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        string Delete(Type type, string where, string tableName = null);

        /// <summary>
        /// 生成更新SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="tableName">表名称</param>
        /// <param name="columns">需要更新的列</param>
        /// <param name="primaryKeyName">条件列</param>
        /// <returns></returns>
        string Update(Type type, string tableName = null, ICollection<string> columns = null, ICollection<string> primaryKeyName = null);
 
        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <param name="primaryKeyName">条件字段</param>
        /// <returns></returns>
        string Select(Type type, string tableName = null, ICollection<string> columnNames = null, ICollection<string> primaryKeyName = null);

        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="where">查询条件</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <returns></returns>
        string Select(Type type, string where, string tableName = null, ICollection<string> columnNames = null);
        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <returns></returns>
        string SelectKey(Type type, string tableName = null, ICollection<string> columnNames = null);

        /// <summary>
        /// 生成查询SQL（in 条件）
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="primaryKeyName">条件字段</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <returns></returns>
        string SelectIn(Type type, ICollection<string> primaryKeyName, ICollection<string> columnNames = null); 
    }
}
