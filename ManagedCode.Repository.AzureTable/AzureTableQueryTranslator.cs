using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    internal class AzureTableQueryTranslator : ExpressionVisitor
    {
        private readonly Stack<BinarySide> _binarySideStack = new(new[] {BinarySide.None});
        private readonly StringBuilder _filter = new();
        private EntityPropertyConverterOptions _options = new();

        public static string TranslateExpression(Expression e)
        {
            var translator = new AzureTableQueryTranslator {_options = new EntityPropertyConverterOptions()};
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
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter && _binarySideStack.Peek() != BinarySide.Right)
            {
                _filter.Append(m.Member.Name);
                return m;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.MemberAccess && _binarySideStack.Peek() != BinarySide.Right)
            {
                var innerExpression = m.Expression as MemberExpression;
                VisitMember(innerExpression);
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

        private string ValueAsString(object value)
        {
            if (value == null)
            {
                return "NULL";
            }

            var ic = CultureInfo.InvariantCulture;
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.DateTime:
                    return $"datetime'{(DateTime) value:o}'";
                case TypeCode.String:
                    return $"'{value.ToString().Replace("'", "''")}'";
                case TypeCode.Boolean:
                    return (bool) value ? "true" : "false";
                case TypeCode.Decimal:
                    return ((decimal) value).ToString(ic);
                case TypeCode.Double:
                    return ((double) value).ToString(ic);
                case TypeCode.Single:
                    return ((float) value).ToString(ic);
                case TypeCode.Int16:
                    return ((short) value).ToString(ic);
                case TypeCode.Int32:
                    return ((int) value).ToString(ic);
                case TypeCode.Int64:
                    return ((long) value).ToString(ic);
                case TypeCode.UInt16:
                    return ((ushort) value).ToString(ic);
                case TypeCode.UInt32:
                    return ((uint) value).ToString(ic);
                case TypeCode.UInt64:
                    return ((ulong) value).ToString(ic);
                case TypeCode.Object:
                    if (value is DateTimeOffset)
                    {
                        return $"datetime'{(DateTimeOffset) value:o}'";
                    }

                    throw new NotSupportedException($"Unsupported type:{value.GetType()}");
            }

            return value.ToString();
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

            var op = string.Empty;

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    op = "and";
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    op = "or";
                    break;

                case ExpressionType.Not:
                    op = "not";
                    break;

                case ExpressionType.Equal:
                    op = QueryComparisons.Equal;
                    break;

                case ExpressionType.NotEqual:
                    op = QueryComparisons.NotEqual;
                    break;

                case ExpressionType.LessThan:
                    op = QueryComparisons.LessThan;
                    break;

                case ExpressionType.LessThanOrEqual:
                    op = QueryComparisons.LessThanOrEqual;
                    break;

                case ExpressionType.GreaterThan:
                    op = QueryComparisons.GreaterThan;
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    op = QueryComparisons.GreaterThanOrEqual;
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
            }

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
}