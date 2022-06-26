using Shrooms.Contracts.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace Shrooms.Domain.Extensions
{
    public static class DbSetExtensions
    {
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
            if (propertyName == null || sortDirection == null)
            {
                var firstProperty = typeof(TEntity)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault();

                if (firstProperty == null)
                {
                    throw new ValidationException($"Entity has to have at least one property");
                }

                return query.OrderBy(string.Format("{0} {1}", firstProperty.Name, "desc"));
            }

            sortDirection = sortDirection.ToLower();

            if (sortDirection != "asc" && sortDirection != "desc")
            {
                throw new ValidationException("Sort direction does not exist");
            }

            if (!EntityHasProperty<TEntity>(propertyName))
            {
                throw new ValidationException("Property name does not exist");
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
