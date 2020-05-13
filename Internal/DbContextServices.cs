using BOZHON.TSAMO.DBHelper.Adapters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    public class DbContextServices : IDbContextServices
    {
        public string ConnectionString { get; set; }

        public ISqlAdapter SqlAdapter { get; set; }

        public IEntityMapper EntityMapper { get; set; }

        public ISqlGenerater SqlGenerater { get; set; }
    }
}
