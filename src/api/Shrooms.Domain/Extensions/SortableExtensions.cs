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
            var formattedStrings = sortableProperties.Select(property => $"{property.Item1} {property.Item2.GetString()};");

            return new Sortable
            {
                SortByProperties = $"{string.Join("", formattedStrings)}{sortable.SortByProperties}"
            };
        }
    }
}
