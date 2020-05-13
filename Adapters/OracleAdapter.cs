using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Adapters
{
    public class OracleAdapter: SqlAdapter
    {
        private readonly DbProviderFactory _dbProviderFac;

        public OracleAdapter(DbProviderFactory dbProviderFac)
        {
            _dbProviderFac = dbProviderFac;
        }

        public override DbProviderFactory GetFactory()
        {
            return _dbProviderFac;
        }

        public override string EscapeTableName(string tableName)
        {
            return tableName.IndexOf('.') >= 0 ? tableName : EscapeSqlIdentifier(tableName);
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("{0}", sqlIdentifier);
        }

        public override string PagingBuild(ref PartedSql partedSql, object args, long skip, long take)
        {
            if (string.IsNullOrEmpty(partedSql.OrderBy))
                throw new InvalidOperationException("miss order by");

            var hasDistinct = partedSql.Select.IndexOf("DISTINCT", StringComparison.OrdinalIgnoreCase) == 0;
            //var select = "SELECT";
            if (hasDistinct)
            {
                partedSql.Select = partedSql.Select.Substring("DISTINCT".Length);
                //select = "SELECT DISTINCT";
            }
            if (skip <= 0)
            {
                string sql = $"SELECT * FROM ({partedSql.Raw} ORDER BY {partedSql.OrderBy}) zh WHERE ROWNUM < {take}";
                //string sql = $"{select} TOP {take} {partedSql.Select} FROM {partedSql.Body} ORDER BY {partedSql.OrderBy}";
                var sbSql = StringBuilderCache.Allocate().Append(sql);
                return StringBuilderCache.ReturnAndFree(sbSql);
            }
            else
            {
                string sql = $"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {partedSql.OrderBy}) AS RID, zh.* FROM({partedSql.Raw}) zh) tb WHERE RID BETWEEN {skip} AND {skip + take}";
                var sbSql = StringBuilderCache.Allocate().Append(sql);

                return StringBuilderCache.ReturnAndFree(sbSql);
            }
        }

        public override string GetParameterPrefix()
        {
            return ":";
        }

        public override long Insert(IDbConnection dbConn, string sql, object param, IDbTransaction transaction)
        {
            sql += "";
            return dbConn.ExecuteScalar<long>(sql, param, transaction);
        }
    }
}
