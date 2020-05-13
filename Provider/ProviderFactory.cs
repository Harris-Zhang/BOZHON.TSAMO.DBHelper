using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Provider
{
    public class ProviderFactory
    {
        private static Dictionary<DbProviderType, string> providerInvariantNames = new Dictionary<DbProviderType, string>();
        private static Dictionary<DbProviderType, DbProviderFactory> providerFactoies = new Dictionary<DbProviderType, DbProviderFactory>(20);
        static ProviderFactory()
        {
            providerInvariantNames.Add(DbProviderType.SqlServer, "System.Data.SqlClient");
            providerInvariantNames.Add(DbProviderType.OleDb, "System.Data.OleDb");
            providerInvariantNames.Add(DbProviderType.ODBC, "System.Data.ODBC");
            providerInvariantNames.Add(DbProviderType.Oracle, "Oracle.DataAccess.Client");
            providerInvariantNames.Add(DbProviderType.MySql, "MySql.Data.MySqlClient");
            providerInvariantNames.Add(DbProviderType.SQLite, "System.Data.SQLite");
            providerInvariantNames.Add(DbProviderType.Firebird, "FirebirdSql.Data.Firebird");
            providerInvariantNames.Add(DbProviderType.PostgreSql, "Npgsql");
            providerInvariantNames.Add(DbProviderType.DB2, "IBM.Data.DB2.iSeries");
            providerInvariantNames.Add(DbProviderType.Informix, "IBM.Data.Informix");
            providerInvariantNames.Add(DbProviderType.SqlServerCe, "System.Data.SqlServerCe");
        }
        public static string GetProviderInvariantName(DbProviderType providerType)
        {
            return providerInvariantNames[providerType];
        }
        public static DbProviderFactory GetDbProviderFactory(DbProviderType providerType)
        {
            if (!providerFactoies.ContainsKey(providerType))
            {
                providerFactoies.Add(providerType, ImportDbProviderFactory(providerType));
            }
            return providerFactoies[providerType];
        }
        private static DbProviderFactory ImportDbProviderFactory(DbProviderType providerType)
        {
            string providerName = providerInvariantNames[providerType];
            DbProviderFactory factory = null;
            try
            {
                factory = null;//DbProviderFactories.GetFactory(providerName); 
                
            }
            catch (ArgumentException e)
            {
                throw new Exception(e.Message);
                //factory = null;
            }
            return factory;
        }
    }
}
