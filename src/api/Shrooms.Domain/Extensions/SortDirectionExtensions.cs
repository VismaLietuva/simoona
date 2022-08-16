using Shrooms.Contracts.Constants;
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
                SortDirection.Ascending => SortDirectionConstants.Ascending,
                SortDirection.Descending => SortDirectionConstants.Descending,
                _ => throw new ArgumentException($"Sort direction {sortDirection} is invalid"),
            };
        }
    }
}
