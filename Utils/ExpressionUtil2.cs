
using BOZHON.TSAMO.DBHelper.ExtensionHelper;
using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Utils
{
    //[DebuggerStepThrough]
    public class ExpressionUtil2 : ExpressionVisitor
    {
        #region sql指令
        private readonly StringBuilder _sqlCmd;

        /// <summary>
        /// sql指令
        /// </summary>
        public string SqlCmd => _sqlCmd.Length > 0 ? $" WHERE {_sqlCmd} " : "";

        public DynamicParameters Param { get; }

        private string _tempFieldName;

        private string TempFieldName
        {
            get => _tempFieldName + ParameterCount;
            set => _tempFieldName = value;
        }

        private string ParamName => _parameterPrefix + TempFieldName;

        private readonly string _parameterPrefix;

        private int ParameterCount { get; set; }

        #endregion

        #region 执行解析

        /// <inheritdoc />
        /// <summary>
        /// 执行解析
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="prefix">字段前缀</param>
        /// <param name="providerOption"></param>
        /// <returns></returns>
        public ExpressionUtil2(LambdaExpression expression, string parameterPrefix)
        {
            _sqlCmd = new StringBuilder(100);
            Param = new DynamicParameters();

            _parameterPrefix = parameterPrefix;
            var exp = TrimExpression.Trim(expression);
            Visit(exp);
        }
        #endregion

        #region 访问成员表达式

        /// <inheritdoc />
        /// <summary>
        /// 访问成员表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            _sqlCmd.Append(node.Member.GetColumnAttributeName());
            TempFieldName = node.Member.Name;
            ParameterCount++;
            return node;
        }

        #endregion

        #region 访问二元表达式
        /// <inheritdoc />
        /// <summary>
        /// 访问二元表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            _sqlCmd.Append("(");
            Visit(node.Left);

            _sqlCmd.Append(node.GetExpressionType());

            Visit(node.Right);
            _sqlCmd.Append(")");

            return node;
        }
        #endregion

        #region 访问常量表达式
        /// <inheritdoc />
        /// <summary>
        /// 访问常量表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            SetParam(node.Value);

            return node;
        }
        #endregion

        #region 访问方法表达式
        /// <inheritdoc />
        /// <summary>
        /// 访问方法表达式
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //if (node.Method.Name == "Contains" && typeof(IEnumerable).IsAssignableFrom(node.Method.DeclaringType) &&
            //    node.Method.DeclaringType != typeof(string))
            if (node.Method.Name.ToUpper() == "IN")
                In(node);
            else if (node.Method.Name == "Equals")
                Equal(node);
            else
                Like(node);

            return node;
        }

        #endregion

        private void SetParam(object value)
        {
            if (value != null)
            {
                if (!string.IsNullOrWhiteSpace(TempFieldName))
                {
                    _sqlCmd.Append(ParamName);
                    Param.Add(TempFieldName, value);
                }
            }
            else
            {
                _sqlCmd.Append("NULL");
            }
        }

        private void Like(MethodCallExpression node)
        {
            Visit(node.Object);
            _sqlCmd.AppendFormat(" LIKE {0}", ParamName);
            switch (node.Method.Name)
            {
                case "StartsWith":
                    {
                        var argumentExpression = (ConstantExpression)node.Arguments[0];
                        Param.Add(TempFieldName, argumentExpression.Value + "%");
                    }
                    break;
                case "EndsWith":
                    {
                        var argumentExpression = (ConstantExpression)node.Arguments[0];
                        Param.Add(TempFieldName, "%" + argumentExpression.Value);
                    }
                    break;
                case "Contains":
                    {
                        var argumentExpression = (ConstantExpression)node.Arguments[0];
                        Param.Add(TempFieldName, "%" + argumentExpression.Value + "%");
                    }
                    break;
                default:
                    throw new Exception("the expression is no support this function");
            }
        }

        private void Equal(MethodCallExpression node)
        {
            Visit(node.Object);
            _sqlCmd.AppendFormat(" ={0}", ParamName);
            var argumentExpression = node.Arguments[0].ToConvertAndGetValue();
            Param.Add(TempFieldName, argumentExpression);
        }

        private void In(MethodCallExpression node)
        {
            //var arrayValue = (IList)((ConstantExpression)node.Object).Value;
            var arrayValue = (string[])(node.Arguments[1] as ConstantExpression).Value;
            if (arrayValue.Length == 0)
            {
                _sqlCmd.Append(" 1 = 2");
                return;
            }
            Visit(node.Arguments[0]);
            _sqlCmd.AppendFormat(" IN {0}", ParamName);
            Param.Add(TempFieldName, arrayValue);
        }
    }
}
