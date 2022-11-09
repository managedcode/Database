using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

internal class AzureTableQueryTranslator : ExpressionVisitor
{
    private readonly Stack<BinarySide> _binarySideStack = new(new[] { BinarySide.None });
    private readonly StringBuilder _filter = new();
    private EntityPropertyConverterOptions _options = new();

    public static string TranslateExpression(Expression e)
    {
        var translator = new AzureTableQueryTranslator { _options = new EntityPropertyConverterOptions() };
        translator.Visit(e);

        return translator._filter.ToString();
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        var expression = node.Body;
        Visit(expression);
        return node;
    }

    protected override Expression VisitMember(MemberExpression m)
    {
        if (m.Expression is { NodeType: ExpressionType.Parameter } &&
            _binarySideStack.Peek() != BinarySide.Right)
        {
            _filter.Append(m.Member.Name);
            return m;
        }

        if (m.Expression is { NodeType: ExpressionType.MemberAccess } &&
            _binarySideStack.Peek() != BinarySide.Right)
        {
            var innerExpression = m.Expression as MemberExpression;
            VisitMember(innerExpression!);
            _filter.Append(_options.PropertyNameDelimiter + m.Member.Name);
            return m;
        }

        if (_binarySideStack.Peek() != BinarySide.Right)
        {
            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }

        var value = GetValue(m);
        _filter.Append(ValueAsString(value));
        return m;
    }

    private static string ValueAsString(object? value)
    {
        if (value is null)
        {
            return "NULL";
        }

        var ic = CultureInfo.InvariantCulture;

        return Type.GetTypeCode(value.GetType()) switch
        {
            TypeCode.DateTime => $"datetime'{(DateTime)value:o}'",
            TypeCode.String => $"'{value.ToString()!.Replace("'", "''")}'",
            TypeCode.Boolean => (bool)value ? "true" : "false",
            TypeCode.Decimal => ((decimal)value).ToString(ic),
            TypeCode.Double => ((double)value).ToString(ic),
            TypeCode.Single => ((float)value).ToString(ic),
            TypeCode.Int16 => ((short)value).ToString(ic),
            TypeCode.Int32 => ((int)value).ToString(ic),
            TypeCode.Int64 => ((long)value).ToString(ic),
            TypeCode.UInt16 => ((ushort)value).ToString(ic),
            TypeCode.UInt32 => ((uint)value).ToString(ic),
            TypeCode.UInt64 => ((ulong)value).ToString(ic),
            TypeCode.Object when value is DateTimeOffset offset => $"datetime'{offset:o}'",
            TypeCode.Object => throw new NotSupportedException($"Unsupported type:{value.GetType()}"),
            _ => value.ToString()!
        };
    }

    protected override Expression VisitConstant(ConstantExpression c)
    {
        var value = c.Value;
        _filter.Append(value == null ? "NULL" : ValueAsString(value));
        return c;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType != ExpressionType.Not)
        {
            return base.VisitUnary(node);
        }

        Visit(node.Operand);
        _filter.Append(" not true");
        return node.Operand;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        if (node.Type == typeof(DateTimeOffset) && _binarySideStack.Peek() == BinarySide.Left)
        {
            _filter.Append("Timestamp");
        }

        return base.VisitParameter(node);
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        _filter.Append("(");

        _binarySideStack.Push(BinarySide.Left);
        Visit(node.Left);
        _binarySideStack.Pop();

        var op = node.NodeType switch
        {
            ExpressionType.And => "and",
            ExpressionType.AndAlso => "and",
            ExpressionType.Or => "or",
            ExpressionType.OrElse => "or",
            ExpressionType.Not => "not",
            ExpressionType.Equal => QueryComparisons.Equal,
            ExpressionType.NotEqual => QueryComparisons.NotEqual,
            ExpressionType.LessThan => QueryComparisons.LessThan,
            ExpressionType.LessThanOrEqual => QueryComparisons.LessThanOrEqual,
            ExpressionType.GreaterThan => QueryComparisons.GreaterThan,
            ExpressionType.GreaterThanOrEqual => QueryComparisons.GreaterThanOrEqual,
            _ => throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported")
        };

        _filter.Append($" {op} ");

        _binarySideStack.Push(BinarySide.Right);
        Visit(node.Right);
        _binarySideStack.Pop();

        _filter.Append(")");

        return node;
    }

    private object GetValue(MemberExpression member)
    {
        var objectMember = Expression.Convert(member, typeof(object));
        var getterLambda = Expression.Lambda<Func<object>>(objectMember);
        var getter = getterLambda.Compile();
        return getter();
    }

    private enum BinarySide
    {
        None,
        Left,
        Right
    }
}