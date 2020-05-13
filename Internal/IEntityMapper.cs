using BOZHON.TSAMO.DBHelper.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    public interface IEntityMapper
    {
        EntityTableInfo GetEntityTableInfo(Type entityType);
    }
}
