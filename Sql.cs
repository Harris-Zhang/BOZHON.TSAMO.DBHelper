﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BOZHON.TSAMO.DBHelper
{
    public class Sql
    {
        public Sql(string sql, object parameters)
        {
            SQL = sql;
            Params = parameters;
        }

        public Sql(string sql, params object[] args)
        {
            SQL = sql;
            Params = ConvertToDapperParam(args);
        }

        /// <summary>
        /// SQL语句
        /// </summary>
        public string SQL { get; }

        /// <summary>
        /// 语句参数
        /// </summary>
        public object Params { get; }

        /// <summary>
        /// 将DapperPoco支持SQL参数格式转换为Dapper的SQL参数格式。
        /// 
        /// DapperPoco支持的参数格式如下：
        /// 1. 基本类型数组。例如：("select ... Id = @p0 and Name = @p1", new object[] { 123, "frank" })
        /// 2. (匿名)实体对象。例如：("select ... Id = @Id and Name = @Name", model) 
        ///                     或：("select ... Id = @Id and Name = @Name", new { Id = 123, Name = "frank" })
        /// 3. 实体对象数组（实现了IEnumerable）。例如：("select ... Id = @Id and Name = @Name", new[] { model1, model2 })
        /// 4. 动态参数（IEnumerable[KeyValuePair[string, object]]）。
        /// </summary>
        /// <param name="sqlArgs">DapperPoco支持SQL参数</param>
        public static object ConvertToDapperParam(object sqlArgs)
        {
            if (sqlArgs == null)
                return null;

            var sqlArgsType = sqlArgs.GetType();
            if (sqlArgsType.IsArray && sqlArgsType.GetElementType() == typeof(object))
            {
                //对应【基本类型数组】
                var sqlArgsTmp = (Array)sqlArgs;
                var param = new KeyValuePair<string, object>[sqlArgsTmp.Length];
                for (var i = 0; i < sqlArgsTmp.Length; ++i)
                {
                    param[i] = new KeyValuePair<string, object>("p" + i, sqlArgsTmp.GetValue(i));
                }
                return param;
            }
            return sqlArgs;
        }
    }
}
