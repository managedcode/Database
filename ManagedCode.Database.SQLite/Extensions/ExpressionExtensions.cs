using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.SQLite.Extensions;

public static class ExpressionExtensions
{
    public static Expression<Func<T, TU>> CorrectExpression<T, TU>(this Expression<Func<T, TU>> orderExpr)
    {
        LambdaExpression lambdaExpression = orderExpr.NodeType switch
        {
            ExpressionType.Lambda => orderExpr,
            _ => throw new NotSupportedException("Must be a predicate")
        };

        var memberExpression = lambdaExpression.Body switch
        {
            UnaryExpression { NodeType: ExpressionType.Convert } body => body.Operand as MemberExpression,
            _ => lambdaExpression.Body as MemberExpression
        };

        if (memberExpression is null) throw new NotSupportedException("Order By does not support: " + orderExpr);

        const string parameterName = "x";

        var parameter = Expression.Parameter(typeof(T), parameterName);
        var property = Expression.Property(parameter, typeof(T), memberExpression.Member.Name);
        var convert = Expression.Convert(property, typeof(TU));
        var lambda = Expression.Lambda<Func<T, TU>>(convert, parameterName, new[] { parameter });

        return lambda;
    }
}