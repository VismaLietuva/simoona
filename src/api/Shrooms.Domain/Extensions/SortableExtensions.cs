using Shrooms.Contracts.Enums;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Infrastructure.Sorting;
using System.Linq;

namespace Shrooms.Domain.Extensions
{
    public static class SortableExtensions
    {
        public static ISortable AddSortablePropertiesToStart(this ISortable sortable, params (string, SortDirection)[] args)
        {
            var formattedStrings = args.Select(arg => $"{arg.Item1} {arg.Item2.GetString()};");
            
            return new Sortable
            {
                SortByProperties = $"{string.Join("", formattedStrings)}{sortable.SortByProperties}"
            };
        }
    }
}
