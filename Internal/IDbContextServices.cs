using BOZHON.TSAMO.DBHelper.Adapters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    public interface IDbContextServices
    {
        string ConnectionString { get; }

        ISqlAdapter SqlAdapter { get; }

        IEntityMapper EntityMapper { get; }
        ISqlGenerater SqlGenerater { get; }
    }
}
