using System;
using System.Linq;
using System.Reflection;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Helpers.Expressions;

namespace KioskBrains.Server.Domain.Helpers.Queries
{
    public static class QueryableExtensions
    {
        public static IQueryable<TRecord> ApplyMetadata<TRecord>(this IQueryable<TRecord> query, SearchMetadata metadata, bool ignoreOrdering = false)
        {
            if (!ignoreOrdering)
            {
                // apply ordering
                var orderingColumn = !string.IsNullOrEmpty(metadata?.OrderBy) ? metadata.OrderBy : "Id";
                var orderingKeyExpression = ExpressionHelper.GetMemberExpression(typeof(TRecord), orderingColumn);

                var orderingDirection = metadata?.OrderDirection ?? OrderDirectionEnum.DESC;
                var orderingMethodName = orderingDirection == OrderDirectionEnum.ASC
                    ? "OrderBy"
                    : "OrderByDescending";

                // todo: replace with compiled expression
                var method = typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                    .Single(x => x.Name == orderingMethodName
                                 && x.GetParameters().Length == 2);
                var genericMethod = method.MakeGenericMethod(typeof(TRecord), orderingKeyExpression.ReturnType);
                query = (IQueryable<TRecord>)genericMethod.Invoke(null, new object[] { query, orderingKeyExpression });
            }

            // apply paging
            if (metadata?.Start > 1)
            {
                query = query.Skip(metadata.Start - 1);
            }

            var pageSize = Math.Min(metadata?.PageSize ?? 20, Constants.MaxSearchPageSize);
            query = query.Take(pageSize);

            return query;
        }
    }
}