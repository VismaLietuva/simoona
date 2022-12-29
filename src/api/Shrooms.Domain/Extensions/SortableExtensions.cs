using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Infrastructure.Sorting;
using System.Linq;

namespace Shrooms.Domain.Extensions
{
    public static class SortableExtensions
    {
        public static ISortable AddSortablePropertiesToStart(this ISortable sortable, params (string, SortDirection)[] sortableProperties)
        {
            return new Sortable
            {
                SortByProperties = $"{SortablePropertiesToOrderString(sortableProperties)}{sortable.SortByProperties}"
            };
        }

        public static ISortable AddSortablePropertiesToEnd(this ISortable sortable, params (string, SortDirection)[] sortableProperties)
        {
            if (string.IsNullOrEmpty(sortable.SortByProperties))
            {
                return AddSortablePropertiesToStart(sortable, sortableProperties);
            }

            return new Sortable
            {
                SortByProperties = $"{sortable.SortByProperties};{SortablePropertiesToOrderString(sortableProperties)}"
            };
        }

        private static string SortablePropertiesToOrderString((string, SortDirection)[] sortableProperties) =>
            string.Join("", sortableProperties.Select(property => $"{property.Item1} {property.Item2.GetString()};"));
    }
}
