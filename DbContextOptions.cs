using BOZHON.TSAMO.DBHelper.Adapters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper
{
    public class DbContextOptions
    {
        public string ConnectionString { get; }

        public ISqlAdapter SqlAdapter { get; }

        public DbContextOptions(string connectionString,ISqlAdapter sqlAdapter)
        {
            ConnectionString = connectionString;
            SqlAdapter = sqlAdapter;
        }
    }
}
