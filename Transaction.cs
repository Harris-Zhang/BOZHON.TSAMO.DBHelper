using System.Data;

namespace BOZHON.TSAMO.DBHelper
{
    internal class Transaction<TDatabase> : ITransaction where TDatabase : DbContext<TDatabase>, new()
    {
        private DbContext<TDatabase> _dbCtx;

        public Transaction(DbContext<TDatabase> dbCtx, IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            _dbCtx = dbCtx;
            dbCtx.BeginTransaction(isolation);
        }

        public void Complete()
        {
            _dbCtx.CommitTransaction();
            _dbCtx = null;
        }

        public void Dispose()
        {
            _dbCtx?.RollbackTransaction();
        }
    }
}
