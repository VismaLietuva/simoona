using Shrooms.Contracts.Constants;

namespace Shrooms.Contracts.ViewModels
{
    public class BasePagingViewModel
    {
        public int PageSize { get; set; } = WebApiConstants.DefaultPageSize;

        public int Page { get; set; } = 1;
    }
}