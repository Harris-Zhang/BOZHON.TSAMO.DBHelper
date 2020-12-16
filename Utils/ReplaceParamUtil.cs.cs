using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BOZHON.TSAMO.DBHelper.Utils
{
    public static class ReplaceParamUtil
    {
        public static string WhereToString(List<string> wheres)
        {
            if (wheres == null)
                return "";
            if (wheres.Count == 0)
                return "";
            if (wheres.Count == 1)
                return "WHERE " + wheres[0];
            else
                return wheres.Count > 0 ? " WHERE " + string.Join(" AND ", wheres) : "";
        }
    }
}
