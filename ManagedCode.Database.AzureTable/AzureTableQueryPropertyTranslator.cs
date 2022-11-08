using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

internal class AzureTableQueryPropertyTranslator : ExpressionVisitor
{
    private readonly List<string> _memberNames = new();
    private string _currentMemberName = string.Empty;
    private int _memberDepth;

    private EntityPropertyConverterOptions _options = new();

    public static List<string> TranslateExpressionToMemberNames(Expression e)
    {
        var translator = new AzureTableQueryPropertyTranslator();
        translator._options = new EntityPropertyConverterOptions();
        translator.Visit(e);
        return translator._memberNames;
    }

    protected override Expression VisitNew(NewExpression node)
    {
        foreach (var argument in node.Arguments)
        {
            Visit(argument);
        }

        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression is { NodeType: ExpressionType.Parameter })
        {
            _currentMemberName += node.Member.Name;
            if (_memberDepth != 0)
            {
                return node;
            }

            _memberNames.Add(_currentMemberName);
            _currentMemberName = string.Empty;

            return node;
        }

        if (node.Expression is { NodeType: ExpressionType.MemberAccess })
        {
            var innerExpression = node.Expression as MemberExpression;
            _memberDepth++;
            VisitMember(innerExpression!);
            _currentMemberName += _options.PropertyNameDelimiter + node.Member.Name;
            _memberDepth--;
            if (_memberDepth == 0)
            {
                _memberNames.Add(_currentMemberName);
                _currentMemberName = string.Empty;
            }

            return node;
        }

        throw new NotSupportedException("Expression not supported");
    }
}