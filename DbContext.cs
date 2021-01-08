using Dapper;
using BOZHON.TSAMO.DBHelper.Internal;
using BOZHON.TSAMO.DBHelper.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Collections;
using BOZHON.TSAMO.DBHelper.Adapters;
using System.Data.SqlClient;

namespace BOZHON.TSAMO.DBHelper
{
    public abstract partial class DbContext<TDatabase> : IDisposable, IInfrastructure<IDbContextServices> where TDatabase : DbContext<TDatabase>, new()
    {

        private IDbContextServices _dbContextServices;

        private IDbConnection _dbConn;
        private IDbTransaction _dbTransaction;

        private bool _disposed;
        private bool _initializing;
        static DbContext()
        {
            if (!ValidLicense.IsValidLicense(out string message))
            {
                throw new Exception(message);
            }

        }

        //添加连接字符串属性
        protected string ConnectionString { get; set; }


        /// <summary>
        /// 配置连接字符串--修改日期2020/05/25(王磊)
        /// </summary>
        //public abstract void SetConnectionString(string connectionString = null);

        #region 属性
        public int? CommandTimeout { get; set; }
        #endregion

        public partial class Table<T, TId>
        {
            internal DbContext<TDatabase> database;

            public Table(DbContext<TDatabase> database)
            {
                this.database = database;
            }

            #region Insert、Update、Delete、Save、Select

            #region 添加
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
            public int Insert(T entity, string tableName = null, ICollection<string> noColumnNames = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Insert(type, tableName, noColumnNames);
                return database.Execute(sql, entity);

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
            public int InsertBatch(List<T> entitys, string tableName = null, ICollection<string> noColumnNames = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Insert(type, tableName, noColumnNames);
                return database.Execute(sql, entitys);

            }
            #endregion

            #region 更新
            /// <summary>
            /// 更新表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="updateColumns">需要更新的列(new string[]{"USER_CODE","USER_NAME"})</param>
            /// <param name="primaryKeyName">条件列(默认主键更新)(new string[]{"USER_CODE"})</param>
            /// <param name="tableName">表名</param>
            /// <example>
            /// <code>
            /// _db.SYS_USER.Update(new SYS_USER { USER_CODE = '',USER_NAME = ''}, new string[]{"USER_NAME"})
            /// </code>
            /// </example>
            /// <returns>修改个数</returns>
            public int Update(T entity, ICollection<string> updateColumns = null, ICollection<string> primaryKeyName = null, string tableName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumberable);
                var sql = database.DbContextServices.SqlGenerater.Update(type, tableName, updateColumns, primaryKeyName);
                return database.Execute(sql, entity);
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
            public int Update(T entity, Expression<Func<T, object>> columns, ICollection<string> updateColumns = null, string tableName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);
                var eTabInfo = database.DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));
                ICollection<string> colNames = null;
                if (columns != null)
                {
                    var cPis = ExpressionUtil.GetPropertyAccessList(columns);
                    colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.ColumnName == p.Name).ColumnName).ToList();
                }
                var sql = database.DbContextServices.SqlGenerater.Update(typeof(T), tableName, updateColumns, colNames);
                return database.Execute(sql, entity);
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
            public int UpdateBatch(List<T> entitys, ICollection<string> updateColumns = null, ICollection<string> primaryKeyName = null, string tableName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumberable);
                var sql = database.DbContextServices.SqlGenerater.Update(type, tableName, updateColumns, primaryKeyName);
                return database.Execute(sql, entitys);
            }

            #endregion

            #region 删除
            /// <summary>
            /// 删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="primaryKeyName">条件列(默认主键)(new string[]{"USER_CODE"})</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns>
            public int Delete(T entity, ICollection<string> primaryKeyName = null, string tableName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);

                var sql = database.DbContextServices.SqlGenerater.Delete(type, tableName, primaryKeyName);
                return database.Execute(sql, entity);
            }
            /// <summary>
            /// 删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="columns">条件列(默认主键)（x => new { x.XXX, x.XXX, x.XXX }）</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns>
            public int Delete(T entity, Expression<Func<T, object>> columns, string tableName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);
                var eTabInfo = database.DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));
                ICollection<string> colNames = null;
                if (columns != null)
                {
                    var cPis = ExpressionUtil.GetPropertyAccessList(columns);
                    colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.ColumnName == p.Name).ColumnName).ToList();
                }
                var sql = database.DbContextServices.SqlGenerater.Delete(type, tableName, colNames);
                return database.Execute(sql, entity);
            }

            /// <summary>
            /// 删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="where">删除条件（p=>p.USER_CODE == "" p.PWD == "" (p.USER_NAME.Contains("123") p.USER_NAME.StartsWith("123") p.USER_NAME.EndWith("123")) p.EMAIL.In(new string[]{"2","3"}）</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns> 
            public int Delete(Expression<Func<T, object>> where, string tableName = null)
            {
                ExpressionUtil2 exp = new ExpressionUtil2(where, database.DbContextServices.SqlAdapter.GetParameterPrefix());
                var sql = database.DbContextServices.SqlGenerater.Delete(typeof(T), exp.SqlCmd, tableName);
                return database.Execute(sql, exp.Param);
            }

            /// <summary>
            /// 批量删除表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entitys">对象</param>
            /// <param name="primaryKeyName">条件列(默认主键)(new string[]{"USER_CODE"})</param>
            /// <param name="tableName">表名</param>
            /// <returns></returns>
            public int DeleteBatch(List<T> entitys, ICollection<string> primaryKeyName = null, string tableName = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);

                var sql = database.DbContextServices.SqlGenerater.Delete(type, tableName, primaryKeyName);
                return database.Execute(sql, entitys);
            }


            #endregion

            #region 保存
            /// <summary>
            /// 保存表数据
            /// </summary>
            /// <typeparam name="T">对象类型</typeparam>
            /// <param name="entity">对象</param>
            /// <param name="columns">需要更新的列(new string[]{"USER_CODE","USER_NAME"})</param>
            /// <param name="tableName">表名</param>
            /// <param name="primaryKeyName">条件列(默认主键)(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public int Save(T entity, ICollection<string> columns = null, string tableName = null, ICollection<string> primaryKeyName = null)
            {
                var t = GetById(entity);
                if (t == null)
                {
                    return Insert(entity, tableName);
                }
                else
                {
                    return Update(entity, columns, primaryKeyName, tableName);
                }
            }

            #endregion

            #region 查询

            /// <summary>
            /// 根据主键查询数据
            /// </summary>
            /// <param name="entity">对象</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public T GetById(T entity, ICollection<string> columns = null)
            {
                var sql = database.DbContextServices.SqlGenerater.SelectKey(typeof(T), columnNames: columns);
                return database.FirstOrDefault<T>(sql, entity);
            }

            /// <summary>
            /// 查询默认第一个数据
            /// </summary>
            /// <param name="entity">对象</param>
            /// <param name="primaryKeyName">条件列(new string[]{"USER_CODE"})</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public T FirstOrDefault(T entity, ICollection<string> primaryKeyName, ICollection<string> columns = null)
            {
                bool isEnumerable;
                var type = GetEnumerableElementType(typeof(T), out isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), null, columns, primaryKeyName);
                return database.FirstOrDefault<T>(sql, entity);
            }
            /// <summary>
            /// 查询默认第一个数据
            /// </summary>
            /// <param name="where">条件（p=>p.USER_CODE == "" p.PWD == "" (p.USER_NAME.Contains("123") p.USER_NAME.StartsWith("123") p.USER_NAME.EndWith("123")) p.EMAIL.In(new string[]{"2","3"}）</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public T FirstOrDefault(Expression<Func<T, object>> where, ICollection<string> columns = null)
            {
                ExpressionUtil2 exp = new ExpressionUtil2(where, database.DbContextServices.SqlAdapter.GetParameterPrefix());
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), exp.SqlCmd, "", columnNames: columns);
                return database.FirstOrDefault<T>(sql, exp.Param);
            }

            /// <summary>
            /// 查询所有数据
            /// </summary>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public IEnumerable<T> GetList(ICollection<string> columns = null)
            {
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), columnNames: columns);
                return database.Query<T>(sql);
            }
            /// <summary>
            /// 查询所有数据
            /// </summary>
            /// <param name="entity"></param>
            /// <param name="primaryKeyName">条件列（AND 连接符）(new string[]{"USER_CODE"})</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public IEnumerable<T> GetList(T entity, ICollection<string> primaryKeyName, ICollection<string> columns = null)
            {
                var type = GetEnumerableElementType(typeof(T), out bool isEnumerable);
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), null, columns, primaryKeyName);
                return database.Query<T>(sql, entity);
            }
            /// <summary>
            /// 查询所有数据
            /// </summary>
            /// <param name="where">条件（p=>p.USER_CODE == "" p.PWD == "" (p.USER_NAME.Contains("123") p.USER_NAME.StartsWith("123") p.USER_NAME.EndWith("123")) p.EMAIL.In(new string[]{"2","3"}）</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public IEnumerable<T> GetList(Expression<Func<T, object>> where, ICollection<string> columns = null)
            {
                ExpressionUtil2 exp = new ExpressionUtil2(where, database.DbContextServices.SqlAdapter.GetParameterPrefix());
                var sql = database.DbContextServices.SqlGenerater.Select(typeof(T), exp.SqlCmd, "", columnNames: columns);
                return database.Query<T>(sql, exp.Param);
            }
            /// <summary>
            /// 查询列表（条件IN）
            /// </summary>
            /// <param name="param">条件参数（new { USER_CODE = string[]}）</param>
            /// <param name="primaryKeyName">条件列（AND 连接符）(new string[]{"USER_CODE"})</param>
            /// <param name="columns">需要查询的列（默认全部）(new string[]{"USER_CODE"})</param>
            /// <returns></returns>
            public IEnumerable<T> GetListByIn(object param, ICollection<string> primaryKeyName, ICollection<string> columns = null)
            {
                var sql = database.DbContextServices.SqlGenerater.SelectIn(typeof(T), primaryKeyName, columns);
                return database.Query<T>(sql, param);
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
            public Tuple<long, List<T>> Paged(int page, int rows, string sortOrder, string whereStr = null, object sqlArgs = null)
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

                long totalCount = database.ExecuteScalar<long>(database.ReplaceParameterSymbol(countSql), sqlArgs);
                List<T> data = database.Fetch<T>(pageSql, sqlArgs);
                return new Tuple<long, List<T>>(totalCount, data);

            }

            #endregion

            #endregion


            /// <summary>
            /// 判断类型
            /// </summary>
            /// <param name="type"></param>
            /// <param name="isEnumerable"></param>
            /// <returns></returns>
            private Type GetEnumerableElementType(Type type, out bool isEnumerable)
            {
                isEnumerable = false;
                if (type.IsArray)
                {
                    isEnumerable = true;
                    //获取数组元素的type
                    return type.GetElementType();
                }
                //是否泛型
                else if (type.IsGenericType)
                {
                    isEnumerable = true;
                    return type.GetGenericArguments()[0];
                }
                return type;
            }
        }

        public partial class Table<T> : Table<T, int>
        {
            /// <summary>
            /// Creates a table in the specified database with a given name.
            /// </summary>
            /// <param name="database">The database this table belongs in.</param>
            /// <param name="likelyTableName">The name for this table.</param>
			public Table(DbContext<TDatabase> database, string likelyTableName)
                : base(database)
            {
            }
        }




        #region Dapper 原始查询与执行


        #endregion

        #region Query/Execute
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="sqlArgs">参数：
        /// <para> 1. 基本类型数组。例如：("select ... Id = @p0 and Name = @p1", new object[] { 123, "frank" }) </para>
        /// <para> 2. (匿名)实体对象。例如：("select ... Id = @Id and Name = @Name", model)  </para>
        /// <para>                      或：("select ... Id = @Id and Name = @Name", new { Id = 123, Name = "frank" }) </para>
        /// <para> 3. 实体对象数组（实现了IEnumerable）。例如：("select ... Id = @Id and Name = @Name", new[] { model1, model2 }) </para>
        /// <para> 4. 动态参数（IEnumerable[KeyValuePair[string, object]]） </para>
        /// </param>
        /// <returns></returns>
        public IEnumerable<T> Query<T>(string sql, object sqlArgs = null)
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return DbConnection.Query<T>(tmpSql, tmpArgs, _dbTransaction, true, CommandTimeout);
        }


        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return DbConnection.Query(tmpSql, map, tmpArgs, _dbTransaction, false, splitOn, CommandTimeout);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return DbConnection.Query(tmpSql, map, tmpArgs, _dbTransaction, false, splitOn, CommandTimeout);
        }

        public T FirstOrDefault<T>(string sql, object sqlArgs = null)
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return DbConnection.QueryFirstOrDefault<T>(tmpSql, tmpArgs, _dbTransaction, CommandTimeout);
        }

        public List<T> Fetch<T>(string sql, object sqlArgs = null)
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return (List<T>)DbConnection.Query<T>(tmpSql, tmpArgs, _dbTransaction, true, CommandTimeout);
        }

        public List<TReturn> Fetch<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return (List<TReturn>)DbConnection.Query(tmpSql, map, tmpArgs, _dbTransaction, true, splitOn, CommandTimeout);
        }

        public List<TReturn> Fetch<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return (List<TReturn>)DbConnection.Query(tmpSql, map, tmpArgs, _dbTransaction, true, splitOn, CommandTimeout);
        }

        public int Execute(string sql, object sqlArgs = null)
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return DbConnection.Execute(tmpSql, tmpArgs, _dbTransaction, CommandTimeout);
        }

        public T ExecuteScalar<T>(string sql, object sqlArgs = null)
        {
            string tmpSql = ReplaceParameterSymbol(sql);
            object tmpArgs = Sql.ConvertToDapperParam(sqlArgs);
#if DEBUG
            LogHelper.Info(SpellCommandText(tmpSql, tmpArgs));
#endif
            return DbConnection.ExecuteScalar<T>(tmpSql, tmpArgs, _dbTransaction, CommandTimeout);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="page">第几页</param>
        /// <param name="rows">每页多少行</param>
        /// <param name="sortOrder">排序方式（createTime desc）</param>
        /// <param name="pageSql">sql</param>
        /// <param name="pageSqlArgs">sql参数</param>
        /// <param name="countSql">总数量sql（默认和分页SQL一样）</param>
        /// <param name="countSqlArgs">总数量sql的参数</param>
        /// <returns></returns>
        public Paged<T> Paged<T>(int page, int rows, string sortOrder, string pageSql, object pageSqlArgs = null, string countSql = null, object countSqlArgs = null) where T : new()
        {
            var partedSql = PagingUtil.SplitSql(pageSql);
            if (!string.IsNullOrEmpty(sortOrder))
                partedSql.OrderBy = sortOrder;
            pageSql = DbContextServices.SqlAdapter.PagingBuild(ref partedSql, pageSqlArgs, (page - 1) * rows, rows);
            if (string.IsNullOrEmpty(countSql))
            {
                countSql = PagingUtil.GetCountSql(partedSql);
                countSqlArgs = pageSqlArgs;
            }

            return PagedInternal(page, rows, countSql, countSqlArgs, () =>
                Fetch<T>(pageSql, pageSqlArgs)
            );
        }

        public Paged<TReturn> Paged<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, int page, int rows, string sortOrder, string pageSql, object pageSqlArgs = null,
                                                              string countSql = null, object countSqlArgs = null, string splitOn = "Id") where TReturn : new()
        {
            var partedSql = PagingUtil.SplitSql(pageSql);
            pageSql = DbContextServices.SqlAdapter.PagingBuild(ref partedSql, pageSqlArgs, (page - 1) * rows, rows);
            if (string.IsNullOrEmpty(countSql))
            {
                countSql = PagingUtil.GetCountSql(partedSql);
                countSqlArgs = pageSqlArgs;
            }

            return PagedInternal(page, rows, countSql, countSqlArgs, () =>
                 Fetch(map, pageSql, pageSqlArgs, splitOn)
            );
        }

        private Paged<T> PagedInternal<T>(int page, int rows, string sqlCount, object countArgs, Func<List<T>> callBackItems) where T : new()
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (rows < 1 || rows > 1000)
                throw new ArgumentOutOfRangeException(nameof(rows));

            var totalCount = ExecuteScalar<long>(ReplaceParameterSymbol(sqlCount), countArgs);
            return new Paged<T>
            {
                CurrentPage = page,
                Rows = rows,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / rows),
                Items = callBackItems()
            };
        }

        #endregion

        #region Procedure

        public int ExecuteProcedure(string proName, DynamicParameters sqlArgs)
        {
            int result = DbConnection.Execute(proName, sqlArgs, _dbTransaction, CommandTimeout, CommandType.StoredProcedure);

            return result;

        }

        public List<T> QueryProcedure<T>(string proName, DynamicParameters sqlArgs)
        {
            IEnumerable<T> result = DbConnection.Query<T>(proName, sqlArgs, _dbTransaction, true, CommandTimeout, CommandType.StoredProcedure);
            return result.AsList<T>();
        }

        #endregion

        #region Transaction               

        public ITransaction GetTransaction(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            return new Transaction<TDatabase>(this, isolation);
        }

        public void BeginTransaction(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            if (_dbTransaction != null)
                throw new InvalidOperationException("当前已有一个事务");
            DbConnection.Open();
            _dbTransaction = DbConnection.BeginTransaction(isolation);
        }

        public void CommitTransaction()
        {
            //System.Threading.Thread.Sleep(100);
            _dbTransaction.Commit();
            DbConnection.Close();
            _dbTransaction = null;
        }

        public void RollbackTransaction()
        {
            _dbTransaction.Rollback();
            DbConnection.Close();
            _dbTransaction = null;
        }
        public bool HasActiveTransaction
        {
            get
            {
                return _dbTransaction != null;
            }
        }

        public void RunInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action();
                CommitTransaction();
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                {
                    RollbackTransaction();
                }

                throw ex;
            }
        }

        public T RunInTransaction<T>(Func<T> func)
        {
            BeginTransaction();
            try
            {
                T result = func();
                CommitTransaction();
                return result;
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                {
                    RollbackTransaction();
                }

                throw ex;
            }
        }

        #endregion

        IDbContextServices IInfrastructure<IDbContextServices>.Instance => DbContextServices;

        /// <summary>
        /// 配置数据库连接字符串,可以在子类中重新--修改日期2020/05/25(王磊)
        /// 配置数据库类型,默认使用SqlServer
        /// </summary>
        /// <param name="optionsBuilder"></param>
        protected internal virtual void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //配置字符串连接
            optionsBuilder.UseConnectionString(ConnectionString);
            //使用SQL Server数据库
            optionsBuilder.UseSqlAdapter(new SqlServerAdapter(SqlClientFactory.Instance));
        }

        protected internal virtual void OnConfigured()
        {

        }


        private IDbContextServices DbContextServices
        {
            get
            {
                if (_disposed)//抛出空异常
                    throw new ObjectDisposedException(GetType().FullName);
                if (_dbContextServices == null)
                    InitServices();
                return _dbContextServices;
            }
        }
        internal static Action<TDatabase> tableConstructor;

        public static TDatabase Init(string connectionString = "")
        {

            TDatabase db = new TDatabase();
            db.ConnectionString = connectionString;
            //db.SetConnectionString(connectionStr);
            db.InitDatabase();
            return db;
        }

        internal void InitDatabase()
        {
            InitServices();
            tableConstructor = tableConstructor ?? CreateTableConstructorForTable();

            tableConstructor(this as TDatabase);
        }

        internal virtual Action<TDatabase> CreateTableConstructorForTable()
        {
            return CreateTableConstructor(typeof(Table<>), typeof(Table<,>));
        }

        /// <summary>
        /// Gets a table creation function for the specified type.
        /// </summary>
        /// <param name="tableType">The object type to create a table for.</param>
        /// <returns>The function to create the <paramref name="tableType"/> table.</returns>
        protected Action<TDatabase> CreateTableConstructor(Type tableType)
        {
            return CreateTableConstructor(new[] { tableType });
        }

        /// <summary>
        /// Gets a table creation function for the specified types.
        /// </summary>
        /// <param name="tableTypes">The object types to create a table for.</param>
        /// <returns>The function to create the <paramref name="tableTypes"/> tables.</returns>
        protected Action<TDatabase> CreateTableConstructor(params Type[] tableTypes)
        {
            var dm = new DynamicMethod("ConstructInstances", null, new[] { typeof(TDatabase) }, true);
            var il = dm.GetILGenerator();

            var setters = GetType().GetProperties()
                .Where(p => p.PropertyType.IsGenericType() && tableTypes.Contains(p.PropertyType.GetGenericTypeDefinition()))
                .Select(p => Tuple.Create(
                        p.GetSetMethod(true),
                        p.PropertyType.GetConstructor(new[] { typeof(TDatabase), typeof(string) }),
                        p.Name,
                        p.DeclaringType
                 ));

            foreach (var setter in setters)
            {
                il.Emit(OpCodes.Ldarg_0);
                // [db]

                il.Emit(OpCodes.Ldstr, setter.Item3);
                // [db, likelyname]

                il.Emit(OpCodes.Newobj, setter.Item2);
                // [table]

                var table = il.DeclareLocal(setter.Item2.DeclaringType);
                il.Emit(OpCodes.Stloc, table);
                // []

                il.Emit(OpCodes.Ldarg_0);
                // [db]

                il.Emit(OpCodes.Castclass, setter.Item4);
                // [db cast to container]

                il.Emit(OpCodes.Ldloc, table);
                // [db cast to container, table]

                il.Emit(OpCodes.Callvirt, setter.Item1);
                // []
            }

            il.Emit(OpCodes.Ret);
            return (Action<TDatabase>)dm.CreateDelegate(typeof(Action<TDatabase>));
        }

        /// <summary>
        /// 替换SQL语句中参数前缀符号
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        private string ReplaceParameterSymbol(string sql)
        {
            string regStr1 = @"\s{0,}=\s{0,}\$";
            var reg1 = new Regex(regStr1);
            string aims1 = @" = " + DbContextServices.SqlAdapter.GetParameterPrefix();
            sql = reg1.Replace(sql, aims1);

            string regStr2 = @"\s{0,}LIKE\s{0,}\$";
            var reg2 = new Regex(regStr2);
            string aims2 = @" LIKE " + DbContextServices.SqlAdapter.GetParameterPrefix();
            sql = reg2.Replace(sql, aims2);

            string regStr3 = @"\s{0,}>\s{0,}\$";
            var reg3 = new Regex(regStr3);
            string aims3 = @" > " + DbContextServices.SqlAdapter.GetParameterPrefix();
            sql = reg3.Replace(sql, aims3);

            string regStr4 = @"\s{0,}<\s{0,}\$";
            var reg4 = new Regex(regStr4);
            string aims4 = @" < " + DbContextServices.SqlAdapter.GetParameterPrefix();
            sql = reg4.Replace(sql, aims4);

            string regStr5 = @"\s{0,}IN\s{0,}\$";
            var reg5 = new Regex(regStr5);
            string aims5 = @" IN " + DbContextServices.SqlAdapter.GetParameterPrefix();
            sql = reg5.Replace(sql, aims5);

            string regStr6 = @"\s{0,}>=\s{0,}\$";
            var reg6 = new Regex(regStr6);
            string aims6 = @" >= " + DbContextServices.SqlAdapter.GetParameterPrefix();
            sql = reg6.Replace(sql, aims6);

            string regStr7 = @"\s{0,}<=\s{0,}\$";
            var reg7 = new Regex(regStr7);
            string aims7 = @" <= " + DbContextServices.SqlAdapter.GetParameterPrefix();
            sql = reg7.Replace(sql, aims7);
            return sql;
        }

        /// <summary>
        /// 替换参数列表，用于写入log用的
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="parameterValues"></param>
        /// <returns></returns>
        private string SpellCommandText(string commandText, object sqlArgs)
        {
            string sql = commandText;
            string prefix = DbContextServices.SqlAdapter.GetParameterPrefix();
            if (sqlArgs != null)
            {
                var sqlArgsType = sqlArgs.GetType();
                if (sqlArgsType.IsArray && sqlArgsType.GetElementType() == typeof(object))
                {
                    //对应【基本类型数组】
                    var sqlArgsTmp = (Array)sqlArgs;
                    for (var i = 0; i < sqlArgsTmp.Length; ++i)
                    {
                        sql = sql.Replace(prefix + "p" + i, sqlArgsTmp.GetValue(i).ToString());
                    }
                }
                else if (sqlArgs is List<KeyValuePair<string, object>>)
                {
                    List<KeyValuePair<string, object>> tmpArgs = (List<KeyValuePair<string, object>>)sqlArgs;
                    foreach (var r in tmpArgs)
                    {
                        string value = DbContextServices.SqlAdapter.SqlValuesType(r.Value.GetType(), r.Value);
                        sql = sql.Replace(prefix + r.Key, value);
                    }
                }
                else if (sqlArgsType.IsGenericType && sqlArgsType.Name.IndexOf("Anonymous") <= 0)
                {
                    IList tmpArgs = (IList)sqlArgs;
                    foreach (var r in tmpArgs)
                    {
                        string tmpsql = commandText;
                        foreach (var p in r.GetType().GetProperties())
                        {
                            if (p.GetValue(r) != null)
                            {
                                string value = DbContextServices.SqlAdapter.SqlValuesType(p.GetValue(r).GetType(), p.GetValue(r));
                                tmpsql = tmpsql.Replace(prefix + p.Name, value);
                            }

                        }
                        sql += tmpsql + ";\r\n";
                    }
                }
                else
                {
                    foreach (var p in sqlArgs.GetType().GetProperties())
                    {
                        if (p.GetValue(sqlArgs) != null)
                        {
                            string value = DbContextServices.SqlAdapter.SqlValuesType(p.GetValue(sqlArgs).GetType(), p.GetValue(sqlArgs));
                            sql = sql.Replace(prefix + p.Name, value);
                        }

                    }
                }
            }

            return sql;
        }


        /// <summary>
        /// 初始化
        /// </summary>
        private void InitServices()
        {
            if (_initializing)
                throw new InvalidDataException("当前正在初始化");
            try
            {
                _initializing = true;
                var optsBuilder = new DbContextOptionsBuilder();
                OnConfiguring(optsBuilder);
                var options = optsBuilder.Build();

                var entityMapper = EntityMapperFactory.Instance.GetEntityMapper(this.GetType().TypeHandle);
                var sqlGenerater = new DefaultSqlGenerater(entityMapper, options.SqlAdapter);
                var dbCtxSves = new DbContextServices
                {
                    ConnectionString = options.ConnectionString,
                    SqlAdapter = options.SqlAdapter,
                    EntityMapper = entityMapper,
                    SqlGenerater = sqlGenerater
                };
                _dbContextServices = dbCtxSves;
                OnConfigured();
            }
            finally
            {
                _initializing = false;
            }
        }

        private IDbConnection DbConnection
        {
            get
            {
                if (_dbConn == null)
                {
                    var dbSce = DbContextServices;
                    _dbConn = dbSce.SqlAdapter.GetFactory().CreateConnection();
                    _dbConn.ConnectionString = dbSce.ConnectionString;

                    //_dbConn.Open();
                }
                return _dbConn;
            }
        }


        public void Dispose()
        {
            _disposed = true;
            if (_dbConn?.State != ConnectionState.Closed)
            {
                if (_dbTransaction != null)
                {
                    _dbTransaction.Rollback();
                    _dbTransaction = null;
                }

                _dbConn.Close();
            }
            _dbTransaction?.Dispose();
            _dbConn?.Dispose();
        }
    }
}
