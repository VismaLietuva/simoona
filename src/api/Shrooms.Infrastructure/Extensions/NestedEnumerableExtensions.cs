using System.Collections.Generic;

namespace Shrooms.Infrastructure.Extensions
{
    public static class NestedEnumerableExtensions
    {
        /// <summary>
        /// Loops through double nested enumerable
        /// </summary>
        /// <returns>Row index and column index</returns>
        public static IEnumerable<(int, int)> GetRowsPositionEnumerator<T>(this IEnumerable<IEnumerable<T>> enumerable)
        {
            var currentRowIndex = -1;

            foreach (var row in enumerable)
            {
                currentRowIndex++;

                var currentColumnIndex = 0;

                foreach (var column in row)
                {
                    yield return (currentRowIndex, currentColumnIndex++);
                }
            }
        }
    }
}
