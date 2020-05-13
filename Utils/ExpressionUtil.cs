using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BOZHON.TSAMO.DBHelper.Utils
{
    [DebuggerStepThrough]
    public static class ExpressionUtil
    {
        public static IReadOnlyList<PropertyInfo> GetPropertyAccessList(LambdaExpression propertyAccessExpression)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            var propertyPaths = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));
            if (propertyPaths == null)
            {
                throw new ArgumentException(
                    //CoreStrings.InvalidPropertiesExpression(propertyAccessExpression),
                    "propertyAccessExpression is null",
                    nameof(propertyAccessExpression));
            }

            return propertyPaths;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccessList(this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyInfo> propertyMatcher)
        {
            Debug.Assert(lambdaExpression.Body != null);
            var newExpression = RemoveConvert(lambdaExpression.Body) as NewExpression;

            var parameterExpression = lambdaExpression.Parameters.Single();

            if (newExpression != null)
            {
                var propertyInfos = newExpression
                    .Arguments
                    .Select(a => propertyMatcher(a, parameterExpression))
                    .Where(p => p != null)
                    .ToList();
                return propertyInfos.Count != newExpression.Arguments.Count ? null : propertyInfos;
            }

            var propertyPath = propertyMatcher(lambdaExpression.Body, parameterExpression);

            return propertyPath != null ? new[] { propertyPath } : null;
        }

        private static PropertyInfo MatchSimplePropertyAccess(this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return (propertyInfos != null) && (propertyInfos.Count == 1) ? propertyInfos[0] : null;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccess(
    this Expression parameterExpression, Expression propertyAccessExpression)
        {
            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveConvert(propertyAccessExpression) as MemberExpression;

                var propertyInfo = memberExpression?.Member as PropertyInfo;

                if (propertyInfo == null)
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            //while (memberExpression.Expression.RemoveConvert() != parameterExpression);
            while (RemoveConvert(memberExpression.Expression) != parameterExpression);

            return propertyInfos;
        }

        public static Expression RemoveConvert(Expression expression)
        {
            while ((expression != null) && ((expression.NodeType == ExpressionType.Convert) || (expression.NodeType == ExpressionType.ConvertChecked)))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }
            return expression;
        }
    }
}
