using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    public class VacationOrderDto
    {
        public int Id { get; set; }

        public string Reference { get; set; }

        public int Number { get; set; }

        public string IssuedOn { get; set; }

        /// <summary>The leave type and start day the order covers; null on a hand-assembled one.</summary>
        public string Type { get; set; }

        public string PeriodStart { get; set; }

        public VacationPersonDto IssuedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public IList<VacationOrderItemDto> Items { get; set; }
    }

    public class VacationOrderItemDto
    {
        public int RequestId { get; set; }

        public string EmployeeName { get; set; }

        public string Type { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }
    }

    public class VacationOrderGenerationDto
    {
        public string From { get; set; }

        public string To { get; set; }

        public int Created { get; set; }

        public int Updated { get; set; }

        public int Unchanged { get; set; }

        /// <summary>Existing orders whose leave is no longer approved. Left untouched.</summary>
        public int Stale { get; set; }

        public IList<VacationOrderDto> Orders { get; set; }
    }

    public class VacationDocumentDto
    {
        public string FileName { get; set; }

        public string ContentType { get; set; }

        public byte[] Content { get; set; }
    }
}
