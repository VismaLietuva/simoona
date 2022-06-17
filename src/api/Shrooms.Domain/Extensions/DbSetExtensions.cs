using Shrooms.Contracts.Infrastructure;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Shrooms.Domain.Extensions
{
    public static class DbSetExtensions
    {
        private static readonly HashSet<string> _availableSortOrders = new ()
        {
            "asc",
            "desc"
        };

        public static IQueryable<TEntity> OrderByPropertyName<TEntity>(
            this IQueryable<TEntity> query, 
            ISortableProperty sortableProperty,
            string defaultSortingPropertyName = null,
            string defaultSortDirection = "asc") where TEntity : class
        {
            return query.OrderByPropertyName(sortableProperty.SortByColumnName, sortableProperty.SortDirection, defaultSortingPropertyName, defaultSortDirection);
        }

        public static IQueryable<TEntity> OrderByPropertyName<TEntity>(
            this IQueryable<TEntity> query, 
            string propertyName, 
            string sortDirection,
            string defaultSortPropertyName = null,
            string defauultSortDirection = "asc") where TEntity : class
        {
            if (propertyName == null || sortDirection == null)
            {
                return query.OrderBy(string.Format("{0} {1}", defaultSortPropertyName, defauultSortDirection));
            }

            if (!_availableSortOrders.Contains(sortDirection.ToLower()))
            {
                throw new ValidationException("Sort direction does not exist");
            }

            if (!EntityHasProperty<TEntity>(propertyName))
            {
                throw new ValidationException("Column name does not exist");
            }

            return query.OrderBy(string.Format("{0} {1}", propertyName, sortDirection));
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
