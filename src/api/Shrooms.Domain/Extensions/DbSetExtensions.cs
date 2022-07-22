using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Shrooms.Domain.Extensions
{
    public static class DbSetExtensions
    {
        public static IQueryable<TSource> ConditionalInclude<TSource, TDestination>(
            this IDbSet<TSource> dbSet, 
            Expression<Func<TSource, TDestination>> expr,
            bool includeProperties) 
            where TSource : class
            where TDestination : class
        {
            if (!includeProperties)
            {
                return dbSet.AsQueryable();
            }

            return dbSet.Include(expr);
        }
    }
}
