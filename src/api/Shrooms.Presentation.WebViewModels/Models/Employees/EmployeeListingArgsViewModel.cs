using Shrooms.Contracts.Constants;
using Shrooms.Contracts.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Presentation.WebViewModels.Models.Employees
{
    public class EmployeeListingArgsViewModel : ISortable, IPageable
    {
        public string Search { get; set; }

        public string SortByProperties { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, int.MaxValue)]
        public int PageSize { get; set; } = WebApiConstants.DefaultPageSize;

        public bool ShowOnlyBlacklisted { get; set; }
    }
}
