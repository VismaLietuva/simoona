﻿using Shrooms.Contracts.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Shrooms.Domain.Extensions
{
    public static class DbSetExtensions
    {
        private const string DefaultSortDirection = "asc";

        private const BindingFlags Flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

        public static IQueryable<TEntity> OrderByPropertyName<TEntity>(
            this IQueryable<TEntity> query,
            ISortableProperty sortableProperty) where TEntity : class
        {
            return query.OrderByPropertyName(sortableProperty.SortByColumnName, sortableProperty.SortDirection);
        }

        public static IQueryable<TEntity> OrderByPropertyName<TEntity>(
            this IQueryable<TEntity> query,
            string propertyName,
            string sortDirection) where TEntity : class
        {
            sortDirection = sortDirection?.ToLower();

            if (sortDirection == null || (sortDirection != "asc" && sortDirection != "desc"))
            {
                sortDirection = DefaultSortDirection;
            }

            if (propertyName == null || !EntityHasProperty<TEntity>(propertyName))
            {
                return query.OrderByFirstPropertyName(sortDirection);
            }

            return query.OrderBy($"{propertyName} {sortDirection}");
        }

        private static IQueryable<TEntity> OrderByFirstPropertyName<TEntity>(this IQueryable<TEntity> query, string sortDirection)
        {
            var firstProperty = typeof(TEntity)
                    .GetProperties(Flags)
                    .First();

            return query.OrderBy($"{firstProperty.Name} {sortDirection}");
        }

        private static bool EntityHasProperty<TEntity>(string propertyName) where TEntity : class
        {
            var propertyNameParts = propertyName.Split('.');

            var type = typeof(TEntity);

            foreach (var property in propertyNameParts)
            {
                type = type.GetProperty(property, Flags)?.PropertyType;

                if (type == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
