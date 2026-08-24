using System;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    public class VacationRequestArgsDto : UserAndOrganizationDto, IPageable
    {
        public string Search { get; set; }

        public string Status { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// Period-overlap bounds: a request matches when its own period
        /// intersects [From, To], not when it starts inside it.
        /// </summary>
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        /// <summary>employee | dateFrom | type | workingDays | status | createdAt.</summary>
        public string Sort { get; set; }

        public string Dir { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }

    public class VacationLogArgsDto : UserAndOrganizationDto, IPageable
    {
        public string Search { get; set; }

        public string Kind { get; set; }

        public string Type { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        /// <summary>at | employee | kind | actor.</summary>
        public string Sort { get; set; }

        public string Dir { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }
    }

    public class VacationStatisticsArgsDto : UserAndOrganizationDto
    {
        public string Search { get; set; }

        /// <summary>employee | accrued | booked | remaining | taken | upcoming | pendingCount.</summary>
        public string Sort { get; set; }

        public string Dir { get; set; }
    }
}
