using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Adapters
{
    public class MySqlAdapter:SqlAdapter
    {
        private readonly DbProviderFactory _dbProviderFac;

        public MySqlAdapter(DbProviderFactory dbProviderFac)
        {
            _dbProviderFac = dbProviderFac;
        }

        public override DbProviderFactory GetFactory()
        {
            return _dbProviderFac;
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("`{0}`", sqlIdentifier);
        }
    }
}
