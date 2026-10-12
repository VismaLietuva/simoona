using System;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    public class ParentalEntitlementDto
    {
        public VacationPersonDto Employee { get; set; }

        public string Type { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class ParentalBalanceDto
    {
        public string Type { get; set; }

        public string Unit { get; set; }

        public int? Amount { get; set; }

        public string Period { get; set; }

        public string PeriodFrom { get; set; }

        public string PeriodTo { get; set; }

        public double? Booked { get; set; }

        public double? Left { get; set; }
    }

    public class ParentalEntitlementListArgsDto
    {
        public int OrganizationId { get; set; }

        public string Search { get; set; }

        public string Dir { get; set; }
    }
}
