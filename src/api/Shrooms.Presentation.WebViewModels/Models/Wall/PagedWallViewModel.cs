using X.PagedList;

namespace Shrooms.Presentation.WebViewModels.Models.Wall
{
    public class PagedWallViewModel<T>
    {
        public IPagedList<T> PagedList { get; set; }

        public int PageSize { get; set; }
    }
}