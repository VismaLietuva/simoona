using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.LazyPaged
{
    public class LazyPaged<T> : ILazyPaged<T>
        where T : class
    {
        public IEnumerable<T> Entries { get; set; }
        public int PageCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int ItemCount { get; set; }

        public LazyPaged(IEnumerable<T> entries, int page, int pageSize, int itemCount)
        {
            Entries = entries;
            Page = page;
            PageSize = pageSize;
            ItemCount = itemCount;
            PageCount = (itemCount + pageSize - 1) / pageSize;
        }
    }
}
