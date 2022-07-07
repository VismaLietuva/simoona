﻿using Shrooms.Contracts.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Shrooms.Domain.Extensions
{
    public static class DbSetExtensions
    {
        private const string DefaultSortDirection = "asc";

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
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault();

            return query.OrderBy($"{firstProperty.Name} {sortDirection}");
        }

        private static bool EntityHasProperty<TEntity>(string propertyName) where TEntity : class
        {
            var propertyNames = propertyName.Split('.');
            var bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

            var type = typeof(TEntity).GetProperty(propertyNames[0], bindingFlags);

            if (type == null)
            {
                return false;
            }
            
            if (propertyNames.Length == 1)
            {
                return true;
            }

            for (var i = 1; i < propertyNames.Length; i++)
            {
                type = type.PropertyType.GetProperty(propertyNames[i], bindingFlags);

                if (type == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}