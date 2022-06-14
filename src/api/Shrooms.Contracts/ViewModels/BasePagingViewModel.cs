using Shrooms.Contracts.Constants;

namespace Shrooms.Contracts.ViewModels
{
    public class BasePagingViewModel
    {
        public virtual int PageSize { get; set; } = WebApiConstants.DefaultPageSize;

        public virtual int Page { get; set; } = 1;
    }
}