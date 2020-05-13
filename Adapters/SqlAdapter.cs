using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Adapters
{
    public abstract class SqlAdapter : ISqlAdapter
    {
        public abstract DbProviderFactory GetFactory();
        public bool IsSupportGuid => false;
        public virtual string EscapeTableName(string tableName)
        {
            return tableName.IndexOf('.') >= 0 ? tableName : EscapeSqlIdentifier(tableName);
        }

        public virtual string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("[{0}]", sqlIdentifier);
        }

        public virtual string PagingBuild(ref PartedSql partedSql, object args, long skip, long take)
        {
            var pageSql = $"{partedSql.Raw} LIMIT @{take} OFFSET @{skip}";
            return pageSql;
        }

        public virtual object MapParameterValue(object value)
        {
            if (value is bool)
                return ((bool)value) ? 1 : 0;

            return value;
        }

        public virtual string GetParameterPrefix()
        {
            return "@";
        }

        public virtual long Insert(IDbConnection dbConn, string sql, object param, IDbTransaction transaction)
        {
            sql += "; SELECT @@IDENTITY AS Id";
            return dbConn.ExecuteScalar<long>(sql, param, transaction);
        }

        /// <summary>
        /// 参数值转换
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string SqlValuesType(Type type, object value)
        {
            if (type == typeof(string))
            {
                return "'" + value + "'";
            }

            if (type == typeof(int))
            {
                return value.ToString();
            }

            if (type == typeof(long))
            {
                return value.ToString();
            }

            if (type == typeof(double))
            {
                return value.ToString();
            }

            if (type == typeof(float))
            {
                return value.ToString();
            }

            if (type == typeof(decimal))
            {
                return value.ToString();
            }

            if (type == typeof(bool))
            {
                return value.ToString();
            }

            if (type == typeof(DateTime))
            {
                return "'" + value + "'";
            }

            if (type == typeof(byte[]))
            {
                return "'" + value + "'";
            }

            return value.ToString();
        }
    }
}
