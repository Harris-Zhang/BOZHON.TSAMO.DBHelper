using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    public class EntityMapperFactory
    {
        private readonly ConcurrentCache<RuntimeTypeHandle, IEntityMapper> _entityMappers = new ConcurrentCache<RuntimeTypeHandle, IEntityMapper>();

        static EntityMapperFactory()
        {
            Instance = new EntityMapperFactory();
        }

        private EntityMapperFactory() { }

        public static EntityMapperFactory Instance { get; }


        public IEntityMapper GetEntityMapper(RuntimeTypeHandle key)
        {
            return _entityMappers.GetOrAdd(key, () =>
            {
                return new DefaultEntityMapper();
            });
        }
        //public IEntityMapper GetEntityMapper(DbContext dbContext)
        //{
        //    var key = dbContext.GetType().TypeHandle;
        //    return _entityMappers.GetOrAdd(key, () =>
        //    {
        //        return new DefaultEntityMapper();
        //    });
        //}
    }
}
