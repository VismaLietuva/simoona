using PagedList;

namespace Shrooms.WebViewModels.Models.Wall
{
    public class PagedWallViewModel<T>
    {
        public IPagedList<T> PagedList { get; set; }

        public int PageSize { get; set; }
    }
}