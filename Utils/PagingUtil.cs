﻿using BOZHON.TSAMO.DBHelper.Adapters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BOZHON.TSAMO.DBHelper.Utils
{
    public class PagingUtil
    {
        //private static readonly Regex _rexSelect = new Regex(@"^\s*SELECT\s+(.+?)\sFROM\s", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _rexSelect = new Regex(@"^\s*SELECT\s+(.+?)\sFROM\s(?![\w\W]*?FROM)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _rexOrderBy = new Regex(@"\s+ORDER\s+BY\s+([^\s]+(?:\s+ASC|\s+DESC)?)\s*$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// 分割SQL
        /// </summary>
        public static PartedSql SplitSql(string sql)
        {
            var parts = new PartedSql { Raw = sql };

             
            // Extract the sql from "SELECT <whatever> FROM"
            var m = _rexSelect.Match(sql);
            if (!m.Success)
                throw new ArgumentException("Unable to parse SQL statement for select");
            parts.Select = m.Groups[1].Value;

            sql = sql.Substring(m.Length);
            m = _rexOrderBy.Match(sql);
            if (m.Success)
            {
                sql = sql.Substring(0, m.Index);
                parts.OrderBy = m.Groups[1].Value;
            }
            parts.Body = sql;

            return parts;
        }

        public static string GetCountSql(PartedSql sql)
        {
            return $"SELECT COUNT(*) FROM {sql.Body}";
        }
    }
}
