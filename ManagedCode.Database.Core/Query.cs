using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core;

/*
  public interface IQueryProvider {
        IQueryable CreateQuery(Expression expression);
        IQueryable<TElement> CreateQuery<TElement>(Expression expression);
        object Execute(Expression expression);
        TResult Execute<TResult>(Expression expression);
    }

Looking at the IQueryProvider interface you might be thinking, “why all these methods?”  The truth is that there are really only two operations, CreateQuery and Execute, we just have both a generic and a non-generic form of each. The generic forms are used most often when you write queries directly in the programming language and perform better since we can avoid using reflection to construct instances.

The CreateQuery method does exactly what it sounds like it does.  It creates a new instance of an IQueryable query based on the specified expression tree. 

The Execute method is the entry point into your provider for actually executing query expressions.  Having an explicit execute instead of just relying on IEnumerable.GetEnumerator() is important because it allows execution of expressions that do not necessarily yield sequences.  For example, the query “myquery.Count()” returns a single integer.  The expression tree for this query is a method call to the Count method that returns the integer.  The Queryable.Count method (as well as the other aggregates and the like) use this method to execute the query ‘right now’.

There, that doesn’t seem so frightening does it?   You could implement all those methods easily, right? Sure you could, but why bother.  I’ll do it for you.  Well all except for the execute method.  I’ll show you how to do that in a later post. First let’s start with the IQuerayble. Since this interface has been split into two, it’s now possible to implement the IQueryable part just once and re-use it for any provider.

  I’ll implement a class called Query<T> that implements IQueryable<T> and all the rest.

*/
public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
{
    private readonly Expression expression;

    private readonly QueryProvider provider;

    public Query() : this(new QueryProviderImpl())
    {
        
    }
    public Query(QueryProvider provider)
    {
        if (provider == null)
        {
            throw new ArgumentNullException("provider");
        }

        this.provider = provider;
        expression = Expression.Constant(this);
    }

    public Query(QueryProvider provider, Expression expression)
    {
        if (provider == null)
        {
            throw new ArgumentNullException(nameof(provider));
        }

        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        if (!typeof(IQueryable<T>).IsAssignableFrom(expression.Type))
        {
            throw new ArgumentOutOfRangeException(nameof(expression));
        }

        this.provider = provider;

        this.expression = expression;
    }

    Expression IQueryable.Expression => expression;

    Type IQueryable.ElementType => typeof(T);

    IQueryProvider IQueryable.Provider => provider;

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>)provider.Execute(expression)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)provider.Execute(expression)).GetEnumerator();
    }

    public override string ToString()
    {
        return provider.GetQueryText(expression);
    }
}

//As you can see now, the IQueryable implementation is straightforward.
//This little object really does just hold onto an expression tree and a provider instance. The provider is where it really gets juicy.

//Okay, now I need some provider to show you.
//I’ve implemented an abstract base class called QueryProvider that Query<T> referred to above.
//A real provider can just derive from this class and implement the Execute method.

//  IQueryProvider interface on my base class QueryProvider.  The CreateQuery methods create new instances of Query<T>  

// UPDATE: It looks like I’ve forget to define a little helper class my implementation was using, so here it is:

//MethodCallExpression e = query.Expression as MethodCallExpressio