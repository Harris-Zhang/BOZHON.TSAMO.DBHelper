using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Adapters
{
    public class SqlServer2012Adapter : SqlServerAdapter
    {
        public SqlServer2012Adapter(DbProviderFactory dbProviderFac)
            : base(dbProviderFac)
        {
        }

        public override string PagingBuild(ref PartedSql partedSql, object args, long skip, long take)
        {
            if (string.IsNullOrEmpty(partedSql.OrderBy))
                throw new ArgumentException("miss order by");

            return $"{partedSql.Raw} OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
        }
    }
}
