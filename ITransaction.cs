using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper
{
    public interface ITransaction : IDisposable, IHideObjectMethods
    {
        /// <summary>
        ///     Completes the transaction. Not calling complete will cause the transaction to rollback on dispose.
        /// </summary>
        void Complete();
    }
}
