using System;
using System.Linq.Expressions;

namespace ManagedCode.Repository.Core.Extensions
{
    public static class RepositoryExtensions
    {
        public static Expression<Func<TItem, bool>> CreateCondition<TId,TItem>(this IRepository<TId, TItem> repository, Expression<Func<TItem, bool>> predicate) where TItem : IItem<TId>
        {
            return predicate;
        }
        
        public static Expression<Func<TItem, bool>>[] CreateCondition<TId,TItem>(this IRepository<TId, TItem> repository, params Expression<Func<TItem, bool>>[] predicates) where TItem : IItem<TId>
        {
            return predicates;
        }
    }
}