using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Adapters
{
    public interface ISqlAdapter
    {
        DbProviderFactory GetFactory();

        /// <summary>
        /// 获取数据库是否具有GUID / UUID的本机支持的标志。
        /// </summary>
        bool IsSupportGuid { get; }

        /// <summary>
        /// 将表名转义
        /// </summary>
        /// <param name="tableName">
        /// 表名称
        /// </param>
        /// <returns></returns>
        string EscapeTableName(string tableName);

        /// <summary>
        /// 将任意SQL标识符转义为适合关联数据库提供程序的格式
        /// </summary>
        /// <param name="sqlIdentifier"></param>
        /// <returns></returns>
        string EscapeSqlIdentifier(string sqlIdentifier);

        /// <summary>
        /// 返回sql语句中参数前缀符号
        /// </summary>
        /// <returns></returns>
        string GetParameterPrefix();

        /// <summary>
        /// 构建适合相应数据库  的分页sql
        /// </summary>
        /// <param name="partedSql">分割的sql（主体部分）</param>
        /// <param name="sqlArgs">参数</param>
        /// <param name="skip">开始行数</param>
        /// <param name="take">应返回的行数</param>
        /// <returns></returns>
        string PagingBuild(ref PartedSql partedSql, object sqlArgs, long skip, long take);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbConn"></param>
        /// <param name="sql"></param>
        /// <param name="sqlArgs"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        long Insert(IDbConnection dbConn, string sql, object sqlArgs, IDbTransaction dbTransaction = null);

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        string SqlValuesType(Type type, object value);
    }
}
