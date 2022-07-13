using System;

namespace Shrooms.Contracts.Infrastructure
{
    public interface IFilterableByDate
    {
        DateTime? StartDate { get; set; }

        DateTime? EndDate { get; set; }
    }
}
