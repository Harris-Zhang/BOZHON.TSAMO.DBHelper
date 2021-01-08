using Dapper;
using BOZHON.TSAMO.DBHelper.Internal;
using BOZHON.TSAMO.DBHelper.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace BOZHON.TSAMO.DBHelper
{
    public partial class DbContext<TDatabase> where TDatabase : DbContext<TDatabase>, new()
    {
        #region Insert、Update 、Delete、Select   Async
        public partial class Table<T, TId>
        {
            /// <summary>
            /// 添加表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="tableName">表名称</param>
            /// <param name="noColumnNames">去除的字段</param>
            /// <example>
            /// <code>
            /// _db.SYS_USER.Insert(new SYS_USER { USER_CODE = '',USER_NAME = ''})
            /// </code>
            /// </example>
            /// <returns>添加个数</returns>
            public Task<int> InsertAsync(T entity, string tableName = null, ICollection<string> noColumnNames = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Insert(type, tableName, noColumnNames);
                return database.ExecuteAsync(sql, entity);
            }
            /// <summary>
            /// 添加表数据(批量添加)
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entitys">对象列表</param>
            /// <param name="tableName">表名称</param>
            /// <param name="noColumnNames">去除的字段</param>
            /// <example>
            /// <code>
            /// _db.SYS_USER.InsertBatch(listUser)
            /// </code>
            /// </example>
            /// <returns>添加个数</returns>
            public Task<int> InsertBatchAsync(List<T> entitys, string tableName = null, ICollection<string> noColumnNames = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Insert(type, tableName, noColumnNames);
                return database.ExecuteAsync(sql, entitys);
            }
            /// <summary>
            /// 更新表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="updateColumns">需要更新的列(new string[]{"USER_CODE","USER_NAME"})</param>
            /// <param name="tableName">表名</param>
            /// <param name="primaryKeyName">条件列(默认主键更新)(new string[]{"USER_CODE"})</param>
            /// <example>
            /// <code>
            /// _db.SYS_USER.Update(new SYS_USER { USER_CODE = '',USER_NAME = ''}, new string[]{"USER_NAME"})
            /// </code>
            /// </example>
            /// <returns>修改个数</returns>
            public Task<int> UpdateAsync(T entity, ICollection<string> updateColumns = null, string tableName = null, ICollection<string> primaryKeyName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumberable);
                var sql = database.DbContextServices.SqlGenerater.Update(type, tableName, updateColumns, primaryKeyName);
                return database.ExecuteAsync(sql, entity);
            }
            /// <summary>
            /// 更新表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="columns">指定对象属性名（x => new { x.XXX, x.XXX, x.XXX }）</param>
            /// <param name="updateColumns">需要更新的列(new string[]{"USER_CODE","USER_NAME"})</param>
            /// <param name="tableName">表名</param>
            /// <example>
            /// <code>
            /// _db.SYS_USER.Update(new SYS_USER { USER_CODE = '',USER_NAME = ''}, (p=> new { x.XXX, x.XXX, x.XXX } ),new string[]{"USER_CODE","USER_NAME"})
            /// </code>
            /// </example>
            /// <returns>修改个数</returns>
            public Task<int> UpdateAsync(T entity, Expression<Func<T, object>> updateColumns, string tableName = null, ICollection<string> primaryKeyName = null)
            {
                var eTableInfo = database.DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));

                ICollection<string> colNames = null;
                if (updateColumns != null)
                {
                    var cPis = ExpressionUtil.GetPropertyAccessList(updateColumns);
                    colNames = cPis.Select(p => eTableInfo.Columns.First(p1 => p1.ColumnName == p.Name).ColumnName).ToList();

                }
                return UpdateAsync(entity, colNames, tableName, primaryKeyName);
            }
            /// <summary>
            /// 批量更新表数据
            /// </summary>
            /// <param name="entitys"></param>
            /// <param name="updateColumns">需要更新的列(new string[]{"USER_CODE","USER_NAME"})</param>
            /// <param name="primaryKeyName">条件列(默认主键更新)(new string[]{"USER_CODE"})</param>
            /// <param name="tableName">表名</param>
            /// <example>
            /// <code>
            /// _db.SYS_USER.UpdateBatch(listUser, new string[]{"USER_NAME"}, new string[]{"USER_CODE"})
            /// </code>
            /// </example>
            /// <returns></returns>
            public Task<int> UpdateBatchAsync(List<T> entitys, ICollection<string> updateColumns = null, string tableName = null, ICollection<string> primaryKeyName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumberable);
                var sql = database.DbContextServices.SqlGenerater.Update(type, tableName, updateColumns, primaryKeyName);
                return database.ExecuteAsync(sql, entitys);
            }
             
            /// <summary>
            /// 删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="primaryKeyName">条件列(默认主键)(new string[]{"USER_CODE"})</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns>
            public Task<int> DeleteAsync(T entity, ICollection<string> primaryKeyName = null, string tableName = null)
            {
                bool isEnumerable;
                var type = GetEnumerableElementType(typeof(T), out isEnumerable);

                var sql = database.DbContextServices.SqlGenerater.Delete(type, tableName, primaryKeyName);
                return database.ExecuteAsync(sql, entity);
            }
            /// <summary>
            /// 删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="columns">条件列(默认主键)（x => new { x.XXX, x.XXX, x.XXX }）</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns>
            public Task<int> DeleteAsync(T entity, Expression<Func<T, object>> primaryKeyName, string tableName = null)
            {
                bool isEnumerable;
                var type = GetEnumerableElementType(typeof(T), out isEnumerable);
                var eTabInfo = database.DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));
                ICollection<string> colNames = null;
                if (primaryKeyName != null)
                {
                    var cPis = ExpressionUtil.GetPropertyAccessList(primaryKeyName);
                    colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.ColumnName == p.Name).ColumnName).ToList();
                }
                var sql = database.DbContextServices.SqlGenerater.Delete(type, tableName, colNames);
                return database.ExecuteAsync(sql, entity);
            }


            /// <summary>
            /// 删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="where">删除条件（p=>p.USER_CODE == "" p.PWD == "" (p.USER_NAME.Contains("123") p.USER_NAME.StartsWith("123") p.USER_NAME.EndWith("123")) p.EMAIL.In(new string[]{"2","3"}）</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns>
            public Task<int> DeleteAsync(Expression<Func<T, object>> where, string tableName = null)
            {
                ExpressionUtil2 exp = new ExpressionUtil2(where, database.DbContextServices.SqlAdapter.GetParameterPrefix());
                var sql = database.DbContextServices.SqlGenerater.Delete(typeof(T), exp.SqlCmd, tableName);
                return database.ExecuteAsync(sql, exp.Param);
            }

            /// <summary>
            /// 批量删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entitys">对象</param>
            /// <param name="primaryKeyName">条件列(默认主键)(new string[]{"USER_CODE"})</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns>
            public Task<int> DeleteBatchAsync(List<T> entitys, ICollection<string> primaryKeyName = null, string tableName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);

                var sql = database.DbContextServices.SqlGenerater.Delete(type, tableName, primaryKeyName);
                return database.ExecuteAsync(sql, entitys);
            }
            /// <summary>
            /// 保存表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="columns">需要更新的列(new string[]{"USER_CODE","USER_NAME"})</param>
            /// <param name="tableName">表名</param>
            /// <param name="primaryKeyName">条件列(默认主键)(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public Task<int> SaveAsync(T entity, ICollection<string> updateColumns = null, string tableName = null, ICollection<string> primaryKeyName = null)
            {
                var t = GetByIdAsync(entity);
                if (t.Result == null)
                {
                    return InsertAsync(entity, tableName);
                }
                else
                {
                    return UpdateAsync(entity, updateColumns, tableName, primaryKeyName);
                }

            }

            /// <summary>
            /// 根据主键查询数据
            /// </summary>
            /// <param name="entity">对象</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public Task<T> GetByIdAsync(T entity, ICollection<string> columns = null)
            {
                var sql = database.DbContextServices.SqlGenerater.SelectKey(typeof(T), columnNames: columns);
                return database.FirstOrDefaultAsync<T>(sql, entity);
            }

            /// <summary>
            /// 查询默认第一个数据
            /// </summary>
            /// <param name="entity">对象</param>
            /// <param name="primaryKeyName">条件列(new string[]{"USER_CODE"})</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public Task<T> FirstOrDefaultAsync(T entity, ICollection<string> primaryKeyName, ICollection<string> columns = null)
            {
                bool isEnumerable;
                var type = GetEnumerableElementType(typeof(T), out isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), null, columns, primaryKeyName);
                return database.FirstOrDefaultAsync<T>(sql, entity);
            }

            /// <summary>
            /// 查询默认第一个数据
            /// </summary>
            /// <param name="where">条件（p=>p.USER_CODE == "" p.PWD == "" (p.USER_NAME.Contains("123") p.USER_NAME.StartsWith("123") p.USER_NAME.EndWith("123")) p.EMAIL.In(new string[]{"2","3"}）</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public Task<T> FirstOrDefaultAsync(Expression<Func<T, object>> where, ICollection<string> columns = null)
            {
                ExpressionUtil2 exp = new ExpressionUtil2(where, database.DbContextServices.SqlAdapter.GetParameterPrefix());
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), exp.SqlCmd, "", columnNames: columns);
                return database.FirstOrDefaultAsync<T>(sql, exp.Param);
            }

            /// <summary>
            /// 查询所有数据
            /// </summary>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public Task<IEnumerable<T>> GetListAsync(ICollection<string> columns = null)
            {
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), columnNames: columns);
                return database.QueryAsync<T>(sql);
            }
            /// <summary>
            /// 查询所有数据
            /// </summary>
            /// <param name="entity"></param>
            /// <param name="primaryKeyName">条件列（AND 连接符）(new string[]{"USER_CODE"})</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public Task<IEnumerable<T>> GetListAsync(T entity, ICollection<string> primaryKeyName, ICollection<string> columns = null)
            {
                bool isEnumerable;
                var type = GetEnumerableElementType(typeof(T), out isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), null, columns, primaryKeyName);
                return database.QueryAsync<T>(sql, entity);
            }

            /// <summary>
            /// 查询所有数据
            /// </summary>
            /// <param name="where">条件（p=>p.USER_CODE == "" p.PWD == "" (p.USER_NAME.Contains("123") p.USER_NAME.StartsWith("123") p.USER_NAME.EndWith("123")) p.EMAIL.In(new string[]{"2","3"}）</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public Task<IEnumerable<T>> GetListAsync(Expression<Func<T, object>> where, ICollection<string> columns = null)
            {
                ExpressionUtil2 exp = new ExpressionUtil2(where, database.DbContextServices.SqlAdapter.GetParameterPrefix());
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), exp.SqlCmd, "", columnNames: columns);
                return database.QueryAsync<T>(sql, exp.Param);
            }

            /// <summary>
            /// 分页查询
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="totalCount">返回的总行数</param>
            /// <param name="page">第几页</param>
            /// <param name="rows">每页多少行</param>
            /// <param name="sortOrder">排序方式（createTime desc）</param>
            /// <param name="pageSql">sql</param>
            /// <param name="pageSqlArgs">sql参数</param>
            /// <param name="countSql">总数量sql（默认和分页SQL一样）</param>
            /// <param name="countSqlArgs">总数量sql的参数</param>
            /// <returns></returns>
            public async Task<Tuple<long, List<T>>> PagedAsync(int page, int rows, string sortOrder, string whereStr = null, object sqlArgs = null)
            {
                string where = "";
                if (!string.IsNullOrEmpty(whereStr) && whereStr.Length >= 5)
                {
                    string wh1 = whereStr.Substring(1, 5);
                    string wh2 = whereStr.Substring(6);
                    where = "WHERE " + wh1.Replace("AND", "").Replace("OR", "") + wh2;
                }

                var pageSql = database.DbContextServices.SqlGenerater.Select(typeof(T)) + where;
                var partedSql = PagingUtil.SplitSql(pageSql);
                if (!string.IsNullOrEmpty(sortOrder))
                    partedSql.OrderBy = sortOrder;
                pageSql = database.DbContextServices.SqlAdapter.PagingBuild(ref partedSql, sqlArgs, (page - 1) * rows, rows);

                var countSql = PagingUtil.GetCountSql(partedSql);

                if (page < 1)
                    throw new ArgumentOutOfRangeException(nameof(page));
                if (rows < 1 || rows > 10000)
                    throw new ArgumentOutOfRangeException(nameof(rows));

                long totalCount = await database.ExecuteScalarAsync<long>(database.ReplaceParameterSymbol(countSql), sqlArgs);
                List<T> data = await database.FetchAsync<T>(pageSql, sqlArgs);
                return new Tuple<long, List<T>>(totalCount, data);

            }

        }
        #endregion

        #region Query/Execute

        public async Task<List<T>> FetchAsync<T>(string sql, object sqlArgs = null)
        {
            IEnumerable<T> result = await DbConnection.QueryAsync<T>(ReplaceParameterSymbol(sql), Sql.ConvertToDapperParam(sqlArgs), _dbTransaction, CommandTimeout);
            return result.AsList<T>();
        }

        public async Task<List<TReturn>> FetchAsync<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            IEnumerable<TReturn> result = await DbConnection.QueryAsync(ReplaceParameterSymbol(sql), map, Sql.ConvertToDapperParam(sqlArgs), _dbTransaction, true, splitOn, CommandTimeout);
            return result.AsList<TReturn>();
        }

        public async Task<List<TReturn>> FetchAsync<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            IEnumerable<TReturn> result = await DbConnection.QueryAsync(ReplaceParameterSymbol(sql), map, Sql.ConvertToDapperParam(sqlArgs), _dbTransaction, true, splitOn, CommandTimeout);
            return result.AsList<TReturn>();
        }

        public Task<T> FirstOrDefaultAsync<T>(string sql, object sqlArgs = null)
        {
            return DbConnection.QueryFirstOrDefaultAsync<T>(ReplaceParameterSymbol(sql), Sql.ConvertToDapperParam(sqlArgs), _dbTransaction, CommandTimeout);
        }

        public Task<IEnumerable<T>> QueryAsync<T>(string sql, object sqlArgs = null)
        {
            return DbConnection.QueryAsync<T>(ReplaceParameterSymbol(sql), Sql.ConvertToDapperParam(sqlArgs), _dbTransaction, CommandTimeout);
        }

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, object sqlArgs = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            DbConnection.QueryAsync(ReplaceParameterSymbol(sql), map, sqlArgs, _dbTransaction, buffered, splitOn, commandTimeout);

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> map, string sql, object sqlArgs = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            DbConnection.QueryAsync(ReplaceParameterSymbol(sql), map, sqlArgs, _dbTransaction, buffered, splitOn, commandTimeout);

        public Task<int> ExecuteAsync(string sql, object sqlArgs = null)
        {
            return DbConnection.ExecuteAsync(ReplaceParameterSymbol(sql), sqlArgs, _dbTransaction, CommandTimeout);
        }

        public Task<T> ExecuteScalarAsync<T>(string sql, object sqlArgs = null)
        {
            return DbConnection.ExecuteScalarAsync<T>(ReplaceParameterSymbol(sql), sqlArgs, _dbTransaction, CommandTimeout);
        }

        public Task<Paged<T>> PagedAsync<T>(int page, int rows, string sortOrder, string pageSql, object pageSqlArgs = null, string countSql = null, object countSqlArgs = null, CancellationToken cancellationToken = default(CancellationToken)) where T : new()
        {
            return Task.Run(() => Paged<T>(page, rows, sortOrder, pageSql, pageSqlArgs, countSql, countSqlArgs), cancellationToken);
        }

        public Task<Paged<TReturn>> PagedAsync<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, int page, int rows, string sortOrder, string pageSql, object pageSqlArgs = null,
                                                              string countSql = null, object countSqlArgs = null, string splitOn = "Id", CancellationToken cancellationToken = default(CancellationToken)) where TReturn : new()
        {
            return Task.Run(() => Paged(map, page, rows, sortOrder, pageSql, pageSqlArgs, countSql, countSqlArgs, splitOn), cancellationToken);
        }

        #endregion

        #region Procedure

        public async Task<int> ExecuteProcedureAsync(string proName, DynamicParameters sqlArgs)
        {
            int result = await DbConnection.ExecuteAsync(proName, sqlArgs, _dbTransaction, CommandTimeout, CommandType.StoredProcedure);

            return result;

        }

        public async Task<List<T>> QueryProcedureAsync<T>(string proName, DynamicParameters sqlArgs)
        {
            IEnumerable<T> result = await DbConnection.QueryAsync<T>(proName, sqlArgs, _dbTransaction, CommandTimeout, CommandType.StoredProcedure);
            return result.AsList<T>();
        }

        #endregion
    }
}
