using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Infrastructure;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace Shrooms.Domain.Extensions
{
    public static class QueryableExtensions
    {
        private const string DefaultSortDirection = SortDirectionConstants.Ascending;

        private const char SortablePropertiesSeparator = ';';
        private const char SortablePropertySeparator = ' ';
        private const char PropertyNamePartsSeparator = '.';

        private const BindingFlags Flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

        public static IQueryable<TEntity> OrderByPropertyNames<TEntity>(
            this IQueryable<TEntity> query,
            ISortable sortableProperties) where TEntity : class
        {
            if (sortableProperties == null)
            {
                return query.OrderByFirstPropertyName(DefaultSortDirection);
            }

            return query.OrderByPropertyNames(sortableProperties.SortByProperties);
        }
        
        private static IQueryable<TEntity> OrderByPropertyNames<TEntity>(
            this IQueryable<TEntity> query,
            string sortByProperties) where TEntity : class
        {
            if (sortByProperties == null)
            {
                return query.OrderByFirstPropertyName(DefaultSortDirection);
            }

            var properties = sortByProperties.Split(new char[] { SortablePropertiesSeparator },
                StringSplitOptions.RemoveEmptyEntries);

            if (!properties.Any())
            {
                return query.OrderByFirstPropertyName(DefaultSortDirection);
            }

            var orderString = string.Empty;

            foreach (var property in properties)
            {
                var propertyParts = property.Split(SortablePropertySeparator);
                var propertyName = propertyParts.FirstOrDefault();

                if (propertyName == null)
                {
                    return query.OrderByFirstPropertyName(DefaultSortDirection);
                }

                var sortDirection = propertyParts.LastOrDefault();

                if (!IsValidSortDirection(sortDirection))
                {
                    sortDirection = DefaultSortDirection;
                }

                if (!EntityHasProperty<TEntity>(propertyName))
                {
                    return query.OrderByFirstPropertyName(DefaultSortDirection);
                }

                orderString += $"{propertyName} {sortDirection},";
            }

            // Removing last comma
            orderString = orderString.Substring(0, orderString.Length - 1);

            return query.OrderBy(orderString);
        }

        private static IQueryable<TEntity> OrderByFirstPropertyName<TEntity>(this IQueryable<TEntity> query, string sortDirection)
        {
            var firstProperty = typeof(TEntity)
                    .GetProperties(Flags)
                    .FirstOrDefault();

            return query.OrderBy($"{firstProperty.Name} {sortDirection}");
        }

        private static bool EntityHasProperty<TEntity>(string propertyName) where TEntity : class
        {
            var propertyNameParts = propertyName.Split(PropertyNamePartsSeparator);
            
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

        private static bool IsValidSortDirection(string sortDirection)
        {
            if (sortDirection == null)
            {
                return false;
            }

            return sortDirection == SortDirectionConstants.Ascending || sortDirection == SortDirectionConstants.Descending;
        }
    }
}
