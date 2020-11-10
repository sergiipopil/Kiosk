using System;
using System.Linq.Expressions;

namespace KioskBrains.Server.Domain.Helpers.Expressions
{
    public class ExpressionHelper
    {
        public static LambdaExpression GetMemberExpression(Type type, string propertyName)
        {
            var parameter = Expression.Parameter(type);
            var body = Expression.PropertyOrField(parameter, propertyName);
            return Expression.Lambda(body, parameter);
        }
    }
}