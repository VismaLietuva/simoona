using Shrooms.Contracts.Enums;
using System;

namespace Shrooms.Domain.Extensions
{
    public static class SortDirectionExtensions
    {
        public static string GetString(this SortDirection sortDirection)
        {
            return sortDirection switch
            {
                SortDirection.Ascending => "asc",
                SortDirection.Descending => "desc",
                _ => throw new ArgumentException($"Sort direction {sortDirection} is not supported"),
            };
        }
    }
}
