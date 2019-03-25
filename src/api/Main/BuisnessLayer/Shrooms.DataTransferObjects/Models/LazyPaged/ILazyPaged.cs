using System.Collections.Generic;

namespace Shrooms.DataTransferObjects.Models.LazyPaged
{
    public interface ILazyPaged<T> 
        where T : class
    {
        IEnumerable<T> Entries { get; set; }
        int PageCount { get; set; }
        int Page { get; set; }
        int PageSize { get; set; }
        int ItemCount { get; set; }
    }
}
