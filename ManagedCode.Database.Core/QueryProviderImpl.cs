using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core;

public class QueryProviderImpl : QueryProvider
{
    public override string GetQueryText(Expression expression)
    {
        return expression.ToString();
    }

    public override object Execute(Expression expression)
    {
        throw new NotImplementedException();
    }
}