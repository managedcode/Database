// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Linq.Expressions;
//
// namespace ManagedCode.Database.AzureTable.Extensions;
//
// public static class TableQueryExtension
// {
//     internal static TableQuery<TItem> Where<TItem>(this TableQuery<TItem> query,
//         List<Expression<Func<TItem, bool>>> predicates)
//     {
//         var whereString = predicates.Select(AzureTableQueryTranslator.TranslateExpression).ToList();
//         var filter = string.Join(" and ", whereString);
//
//         return query.Where(filter);
//     }
//
//     // TODO: rename
//     internal static TableQuery<TItem> CustomSelect<TItem>(this TableQuery<TItem> query,
//         Expression<Func<TItem, object>> selectExpression)
//     {
//         var selectedProperties =
//             AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(selectExpression);
//
//         return query.Select(selectedProperties);
//     }
//
//
//     internal static TableQuery<TItem> OrderBy<TItem>(this TableQuery<TItem> query,
//         List<Expression<Func<TItem, object>>> predicates)
//     {
//         if (predicates.Count > 0)
//         {
//             foreach (var item in predicates)
//             {
//                 var names = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(item);
//
//                 query = names.Aggregate(query, (current, name) => current.OrderBy(name));
//             }
//         }
//
//         return query;
//     }
//
//     internal static TableQuery<TItem> OrderByDescending<TItem>(this TableQuery<TItem> query,
//         List<Expression<Func<TItem, object>>> predicates)
//     {
//         if (predicates.Count > 0)
//         {
//             foreach (var item in predicates)
//             {
//                 var names = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(item);
//
//                 query = names.Aggregate(query, (current, name) => current.OrderByDesc(name));
//             }
//         }
//
//         return query;
//     }
//
//     internal static TableQuery<TItem> Skip<TItem>(this TableQuery<TItem> query, int? skip)
//     {
//         if (skip.HasValue)
//         {
//             query = query.Skip(skip.Value);
//         }
//
//         return query;
//     }
//
//     internal static TableQuery<TItem> Take<TItem>(this TableQuery<TItem> query, int? take)
//     {
//         if (take.HasValue)
//         {
//             query = query.Take(take.Value);
//         }
//
//         return query;
//     }
// }