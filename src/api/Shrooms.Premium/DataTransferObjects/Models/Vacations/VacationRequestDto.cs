using System;
using System.Collections.Generic;

namespace Shrooms.Premium.DataTransferObjects.Models.Vacations
{
    public class VacationPersonDto
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PictureId { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class VacationRequestDto
    {
        public int Id { get; set; }

        public string Type { get; set; }

        public string Status { get; set; }

        public VacationPersonDto Employee { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public double WorkingDays { get; set; }

        public string Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public VacationPersonDto ReviewedBy { get; set; }

        public string ReviewComment { get; set; }

        /// <summary>
        /// Server-computed per the transition rules, so the client does not have
        /// to duplicate them. It still computes them locally for instant
        /// feedback; where the two disagree, this one wins.
        /// </summary>
        public bool CanEdit { get; set; }

        public bool CanCancel { get; set; }

        public double? Entitlement { get; set; }

        /// <summary>
        /// The employee's days left, with this request excluded from the total
        /// when it is one of the types that charges the balance — an approver
        /// wants the balance as it stood *before* the decision in front of them.
        ///
        /// Computed here rather than left to the caller: subtracting the
        /// request's own days client-side gets parental and unpaid leave wrong,
        /// since neither of them touches the balance at all.
        /// </summary>
        public double? RemainingDays { get; set; }

        public VacationShortfallDto BalanceShortfall { get; set; }

        public IList<VacationOverlapDto> Overlaps { get; set; }

        public VacationLastEditDto LastEdit { get; set; }
    }

    public class VacationShortfallDto
    {
        public double Requested { get; set; }

        public double Remaining { get; set; }
    }

    public class VacationOverlapDto
    {
        public VacationPersonDto Employee { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }
    }

    public class VacationLastEditDto
    {
        public DateTime At { get; set; }

        public VacationPersonDto Actor { get; set; }

        public IList<VacationFieldChangeDto> Changes { get; set; }
    }
}
