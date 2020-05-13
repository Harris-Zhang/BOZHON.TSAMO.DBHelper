using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Adapters
{
    public class SqlServerAdapter : SqlAdapter
    {
        private readonly DbProviderFactory _dbProviderFac;

        public SqlServerAdapter(DbProviderFactory dbProviderFac)
        {
            _dbProviderFac = dbProviderFac;
        }

        public override DbProviderFactory GetFactory()
        {
            return _dbProviderFac;
        }

        public override string PagingBuild(ref PartedSql partedSql, object args, long skip, long take)
        {
            if (string.IsNullOrEmpty(partedSql.OrderBy))
                throw new InvalidOperationException("miss order by");

            var hasDistinct = partedSql.Select.IndexOf("DISTINCT", StringComparison.OrdinalIgnoreCase) == 0;
            var select = "SELECT";
            if (hasDistinct)
            {
                partedSql.Select = partedSql.Select.Substring("DISTINCT".Length);
                select = "SELECT DISTINCT";
            }
            if (skip <= 0)
            {
                string sql = $"{select} TOP {take} {partedSql.Select} FROM {partedSql.Body} ORDER BY {partedSql.OrderBy}";
                var sbSql = StringBuilderCache.Allocate().Append(sql);
                return StringBuilderCache.ReturnAndFree(sbSql);
            }
            else
            {
                //var sbSql = StringBuilderCache.Allocate()
                //            .AppendFormat("SELECT * FROM (SELECT {0}, ROW_NUMBER() OVER " +
                //                          "(order by {1}) As RowNum FROM {2}) AS RowConstrainedResult " +
                //                          "WHERE RowNum > {3} AND RowNum <= {4}",
                //                          partedSql.Select, partedSql.OrderBy, partedSql.Body, skip, skip + take);
                string sql = $"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {partedSql.OrderBy}) AS RID, zh.* FROM({partedSql.Raw}) zh) tb WHERE RID BETWEEN {skip} AND {skip + take}";
                var sbSql = StringBuilderCache.Allocate().Append(sql);

                return StringBuilderCache.ReturnAndFree(sbSql);
            }
        }

        public override long Insert(IDbConnection dbConn, string sql, object param, IDbTransaction transaction)
        {
            sql += "; select SCOPE_IDENTITY() Id";
            return dbConn.ExecuteScalar<long>(sql, param, transaction);
        }
    }
}
