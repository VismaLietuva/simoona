using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    public class VacationEventDto
    {
        public int Id { get; set; }

        public int RequestId { get; set; }

        public string Kind { get; set; }

        public DateTime At { get; set; }

        public VacationPersonDto Actor { get; set; }

        public VacationPersonDto Employee { get; set; }

        public string Type { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public double WorkingDays { get; set; }

        public string Comment { get; set; }

        public IList<VacationFieldChangeDto> Changes { get; set; }

        /// <summary>
        /// The request as it stands *now*, so the log can offer the same
        /// administrator override the register does. Null when the request has
        /// since been removed — the audit row outlives it.
        ///
        /// Deliberately separate from the snapshot fields above: those describe
        /// the request at the moment of the action and must never be refreshed.
        /// </summary>
        public VacationRequestDto Request { get; set; }
    }

    /// <summary>
    /// One field's before/after. <see cref="Field"/> is one of type, status,
    /// dateFrom, dateTo, note — the wire names the client switches on.
    /// </summary>
    public class VacationFieldChangeDto
    {
        public string Field { get; set; }

        public string From { get; set; }

        public string To { get; set; }
    }
}
