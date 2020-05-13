using BOZHON.TSAMO.DBHelper.Attributes;
using BOZHON.TSAMO.DBHelper.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    internal class DefaultEntityMapper : IEntityMapper
    {
        private readonly ConcurrentCache<RuntimeTypeHandle, EntityTableInfo> _caches = new ConcurrentCache<RuntimeTypeHandle, EntityTableInfo>();

        public DefaultEntityMapper()
        {
        }

        public EntityTableInfo GetEntityTableInfo(Type entityType)
        {
            return _caches.GetOrAdd(entityType.TypeHandle, () =>
            { 
                var tableName = InflectTableName(entityType);
                var columns = InflectColumns(entityType);

                return new EntityTableInfo
                {
                    TableName = tableName,
                    Columns = columns
                };
            });
        }

        private string InflectTableName(Type entityType)
        {
            string attrName = entityType.GetCustomAttributeValue<TableAttribute>(x => x.TableName);
            string entyName = entityType.Name;

            return !string.IsNullOrEmpty(attrName) ? attrName : entyName;
        }

        private EntityColumnInfo[] InflectColumns(Type entityType)
        {
            var props = entityType.GetProperties();
            var lstRetn = new List<EntityColumnInfo>(props.Length);
            foreach (var prop in props)
            {
                var colAttr = prop.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() as ColumnAttribute;
                if (colAttr?.IsIgnore == true)
                {
                    continue;
                }

                string colName = !string.IsNullOrEmpty(colAttr?.ColumnName) ? colAttr.ColumnName : prop.Name;
                bool isPrimaryKey = false;
                bool IsAddIgnore = false;

                if (colAttr?.IsPrimaryKey == true)
                {
                    isPrimaryKey = colAttr.IsPrimaryKey;
                }
                if (colAttr?.IsAddIgnore == true)
                {
                    IsAddIgnore = colAttr.IsAddIgnore;
                }
                //var fluentEci = fluentEti?.Columns?.FirstOrDefault(p => p.Property.Name == prop.Name);
                //if (fluentEci?.IsIgnore == true)
                //    continue;

                //var colName = !string.IsNullOrEmpty(fluentEci?.ColumnName) ? fluentEci.ColumnName : prop.Name;
                //var isPrimaryKey = false;
                //var isAutoIncrement = false;
                //if (fluentEci?.IsPrimaryKey.HasValue ?? false)
                //    isPrimaryKey = fluentEci.IsPrimaryKey.Value;
                //else if (IsPrimaryKeyName(tableName, colName))
                //    isPrimaryKey = true;

                //if (fluentEci?.IsAutoIncrement.HasValue ?? false)
                //{
                //    isAutoIncrement = fluentEci.IsAutoIncrement.Value;
                //}
                //else
                //{
                //    var t = prop.PropertyType;
                //    if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                //        t = t.GetGenericArguments()[0];

                //    if (isPrimaryKey && IsPrimaryKeyName(tableName, colName) &&
                //        (t == typeof(long) || t == typeof(ulong) ||
                //        t == typeof(int) || t == typeof(uint) ||
                //        t == typeof(short) || t == typeof(ushort)))
                //        isAutoIncrement = true;
                //}

                lstRetn.Add(new EntityColumnInfo
                {
                    IsPrimaryKey = isPrimaryKey,
                    IsAddIgnore = IsAddIgnore,
                    ColumnName = colName
                });
            }

            return lstRetn.ToArray();
        }

        private bool IsPrimaryKeyName(string tableName, string colName)
        {
            return colName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                   colName.Equals(tableName + "Id", StringComparison.OrdinalIgnoreCase) ||
                   colName == tableName + "_Id";
        }
    }
}
