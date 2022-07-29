using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Contracts.DataTransferObjects.Employees
{
    public class EmployeeListingArgsDto : ISortable, IPageable
    {
        public string Search { get; set; }

        public string SortByProperties { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public bool ShowOnlyBlacklisted { get; set; }
    }
}
