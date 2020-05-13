using BOZHON.TSAMO.DBHelper.Adapters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper
{
    public class DbContextOptionsBuilder
    {

        private string _connectionString;
        private ISqlAdapter _sqlAdapter;

        public DbContextOptionsBuilder UseConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            return this;
        }
        
        public DbContextOptionsBuilder UseSqlAdapter(ISqlAdapter sqlAdapter)
        {
            _sqlAdapter = sqlAdapter;
            return this;
        }

        internal DbContextOptions Build()
        {
            return new DbContextOptions(_connectionString, _sqlAdapter);
        }
    }
}
