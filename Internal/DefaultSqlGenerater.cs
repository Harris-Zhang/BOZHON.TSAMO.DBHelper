using BOZHON.TSAMO.DBHelper.Adapters;
using BOZHON.TSAMO.DBHelper.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Internal
{
    internal class DefaultSqlGenerater : ISqlGenerater
    {
        private readonly IEntityMapper _mapper;
        private readonly ISqlAdapter _sqlAdapter;
        private static readonly ConcurrentCache<string, string> _sqlsCache = new ConcurrentCache<string, string>();

        public DefaultSqlGenerater(IEntityMapper mapper, ISqlAdapter sqlAdapter)
        {
            _mapper = mapper;
            _sqlAdapter = sqlAdapter;
        }

        /// <summary>
        /// 生成添加SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="tableName">表名</param>
        /// <param name="noColumnNames">去除的列</param>
        /// <returns></returns>
        public string Insert(Type type, string tableName, ICollection<string> noColumnNames = null)
        {
            var key = $"{nameof(Insert)}_{type.FullName}_{tableName}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);
                List<EntityColumnInfo> insertCols = null;
                if (noColumnNames?.Count > 0)
                {
                    insertCols = tableInfo.Columns.Where(p => !p.IsAddIgnore && (!noColumnNames.Any(c => c == p.ColumnName))).ToList();
                }
                else
                {
                    insertCols = tableInfo.Columns.Where(p => !p.IsAddIgnore).ToList();
                }

                var iColsName = string.Join(", ", insertCols.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));
                var iColsParams = string.Join(", ", insertCols.Select(p => paramPrefix + p.ColumnName));
                return $"insert into {tableName} ({iColsName}) values ({iColsParams})";
            });
        }

        /// <summary>
        /// 生成更新SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="tableName">表名称</param>
        /// <param name="updateColumns">需要更新的列</param>
        /// <param name="primaryKeyName">条件列(默认主键更新)</param>
        /// <returns></returns>
        public string Update(Type type, string tableName = null, ICollection<string> updateColumns = null, ICollection<string> primaryKeyName = null)
        {
            var keyCols = updateColumns?.Count > 0 ? string.Join(",", updateColumns) : "All";
            var key = $"{nameof(Update)}_{type.FullName}_{tableName}_{keyCols}_{GetKey(primaryKeyName)}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var pkInfo = GetPrimaryKey(tableInfo, primaryKeyName);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                List<EntityColumnInfo> updCols;
                if (updateColumns?.Count > 0)
                {
                    updCols = updateColumns.Select(p =>
                    {
                        var c = tableInfo.Columns.FirstOrDefault(p1 => p1.ColumnName == p);
                        if (null == c)
                            throw new ArgumentException($"指定的列 {p} 不存在");
                        return c;
                    }).ToList();
                }
                else
                {
                    updCols = tableInfo.Columns.Where(p => !pkInfo.Any(p2 => p.ColumnName == p2.ColumnName)).ToList();
                }

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);


                var where = string.Join(" AND ", pkInfo.Select(p =>
                {
                    var colName = _sqlAdapter.EscapeSqlIdentifier(p.ColumnName);
                    return $"{colName} = {paramPrefix}{p.ColumnName}";
                }));

                var setCols = string.Join(", ", updCols.Select(p =>
                {
                    var colName = _sqlAdapter.EscapeSqlIdentifier(p.ColumnName);
                    var colParam = paramPrefix + p.ColumnName;
                    return $"{colName} = {colParam}";
                }));
                return $"update {tableName} set {setCols} where {where}";
            });
        }
  
        /// <summary>
        /// 生成删除SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="tableName">表名称</param>
        /// <param name="primaryKeyName">条件字段</param>
        /// <returns></returns>
        public string Delete(Type type, string tableName = null, ICollection<string> primaryKeyName = null)
        {
            var key = $"{nameof(Delete)}_{type.FullName}_{tableName}_{GetKey(primaryKeyName)}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);

                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = _sqlAdapter.EscapeTableName(tableInfo.TableName);

                List<EntityColumnInfo> pkInfo = GetPrimaryKey(tableInfo, primaryKeyName);

                var where = string.Join(" AND ", pkInfo.Select(p =>
                {
                    var colName = _sqlAdapter.EscapeSqlIdentifier(p.ColumnName);
                    return $"{colName} = {paramPrefix}{p.ColumnName}";
                }));

                return $"delete from {tableName} where {where}";
            });
        }

        /// <summary>
        /// 生成删除SQL
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="where">查询条件</param>
        /// <param name="tableName">表名</param>
        /// <returns></returns>
        public string Delete(Type type, string where, string tableName = null)
        {
            var key = $"{nameof(Delete)}_{type.FullName}_{tableName}_{where}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);

                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = _sqlAdapter.EscapeTableName(tableInfo.TableName);

                 
                return $"delete from {tableName} {where}";
            });
        }

        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <param name="primaryKeyName">条件字段</param>
        /// <returns></returns>
        public string Select(Type type, string tableName = null, ICollection<string> columnNames = null, ICollection<string> primaryKeyName = null)
        {
            var key = $"{nameof(Select)}_{type.FullName}_{tableName}_{GetKey(columnNames)}_{GetKey(primaryKeyName)}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);

                var qCols = "";
                if (columnNames?.Count > 0)
                    qCols = string.Join(", ", columnNames.Select(p => _sqlAdapter.EscapeSqlIdentifier(p)));
                else
                    qCols = string.Join(", ", tableInfo.Columns.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));

                string where = "";
                if (primaryKeyName?.Count > 0)
                {
                    where = " where " + string.Join(" AND ", primaryKeyName.Select(p =>
                     {
                         var colName = _sqlAdapter.EscapeSqlIdentifier(p);
                         return $"{colName} = {paramPrefix}{p}";
                     }));
                }

                return $"select {qCols} from {tableName} {where}";
            });
        }

        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <param name="primaryKeyName">条件字段</param>
        /// <returns></returns>
        public string Select(Type type, string where, string tableName = null, ICollection<string> columnNames = null)
        {
            var key = $"{nameof(Select)}_{type.FullName}_{tableName}_{GetKey(columnNames)}_{where}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);

                var qCols = "";
                if (columnNames?.Count > 0)
                    qCols = string.Join(", ", columnNames.Select(p => _sqlAdapter.EscapeSqlIdentifier(p)));
                else
                    qCols = string.Join(", ", tableInfo.Columns.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));
 
                return $"select {qCols} from {tableName} {where}";
            });
        }

        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="tableName">表名</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <returns></returns>
        public string SelectKey(Type type, string tableName = null, ICollection<string> columnNames = null)
        {
            var key = $"{nameof(SelectKey)}_{type.FullName}_{tableName}_{GetKey(columnNames)}_ID";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);

                var qCols = "";
                if (columnNames?.Count > 0)
                    qCols = string.Join(", ", columnNames.Select(p => _sqlAdapter.EscapeSqlIdentifier(p)));
                else
                    qCols = string.Join(", ", tableInfo.Columns.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));

                List<EntityColumnInfo> pkInfo = GetPrimaryKey(tableInfo);
                string where = "where " + string.Join(" AND ", pkInfo.Select(p =>
                {
                    var colName = _sqlAdapter.EscapeSqlIdentifier(p.ColumnName);
                    return $"{colName} = {paramPrefix}{p.ColumnName}";
                }));

                return $"select {qCols} from {tableName} {where}";
            });
        }

        /// <summary>
        /// 生成查询SQL（in 条件）
        /// </summary>
        /// <param name="type">类别</param>
        /// <param name="primaryKeyName">条件字段</param>
        /// <param name="columnNames">需要查询的字段（默认全部）</param>
        /// <returns></returns>
        public string SelectIn(Type type, ICollection<string> primaryKeyName, ICollection<string> columnNames = null)
        {
            var key = $"{nameof(SelectIn)}_{type.FullName}_{GetKey(columnNames)}_{GetKey(primaryKeyName)}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();
                 
                string tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);

                var qCols = "";
                if (columnNames?.Count > 0)
                    qCols = string.Join(", ", columnNames.Select(p => _sqlAdapter.EscapeSqlIdentifier(p)));
                else
                    qCols = string.Join(", ", tableInfo.Columns.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));

                string where = "";
                if (primaryKeyName?.Count > 0)
                {
                    where = " where " + string.Join(" AND ", primaryKeyName.Select(p =>
                    {
                        var colName = _sqlAdapter.EscapeSqlIdentifier(p);
                        return $"{colName} IN {paramPrefix}{p}";
                    }));
                }

                return $"select {qCols} from {tableName} {where}";
            });
        }
 
        private List<EntityColumnInfo> GetPrimaryKey(EntityTableInfo tableInfo, ICollection<string> primaryKey = null)
        {
            List<EntityColumnInfo> pkInfo = new List<EntityColumnInfo>();

            if (primaryKey?.Count > 0)
            {
                foreach (string pk in primaryKey)
                {
                    pkInfo.Add(tableInfo.Columns.FirstOrDefault(p => p.ColumnName == pk));
                }
            }
            else
            {
                var pks = tableInfo.Columns.Where(p => p.IsPrimaryKey);
                pkInfo = pks.ToList();
            }
            if (pkInfo == null || pkInfo.Count < 1)
                throw new InvalidProgramException($"获取实体 {tableInfo.TableName} 的主键失败");
            return pkInfo;
        }

        private string GetKey(ICollection<string> arr)
        {
            if (arr?.Count > 0)
            {
                return string.Join(",", arr);
            }
            else
            {
                return "";
            }
        }
    }
}
