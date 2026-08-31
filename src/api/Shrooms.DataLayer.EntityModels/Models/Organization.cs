using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Constants;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Organization : SoftDeletableModel
    {
        [Required]
        [StringLength(BusinessLayerConstants.MaxOrganizationNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(BusinessLayerConstants.MaxOrganizationShortNameLength)]
        public string ShortName { get; set; }

        [StringLength(50)]
        public string HostName { get; set; }

        [Required]
        public bool HasRestrictedAccess { get; set; }

        public virtual ICollection<Module> ShroomsModules { get; set; }

        [Required]
        [StringLength(BusinessLayerConstants.WelcomeEmailLength)]
        public string WelcomeEmail { get; set; }

        public bool RequiresUserConfirmation { get; set; }

        public string CalendarId { get; set; }

        public string TimeZone { get; set; }

        public string CultureCode { get; set; }

        public string BookAppAuthorizationGuid { get; set; }

        public string AuthenticationProviders { get; set; }

        public string KudosYearlyMultipliers { get; set; }

        /// <summary>Prefix printed before a leave order's number, e.g. "AT-".</summary>
        [StringLength(10)]
        public string VacationOrderPrefix { get; set; }

        /// <summary>
        /// Where the order sequence starts. Set it to continue from a paper trail
        /// that began outside the application; the allocator never goes below it.
        /// </summary>
        public int? VacationOrderStartNumber { get; set; }

        /// <summary>
        /// The block printed at the top of a leave order — company name, address,
        /// registration code, the signatory's title and name — one line each.
        /// Free text rather than columns because it is letterhead, not data.
        /// </summary>
        public string VacationOrderLetterhead { get; set; }

        /// <summary>City printed under the order number, e.g. "Vilnius".</summary>
        [StringLength(100)]
        public string VacationOrderCity { get; set; }

        /// <summary>
        /// The signature line, title and name separated by a tab, e.g.
        /// "Finansų vadovė\tRaminta Pakalnienė".
        /// </summary>
        [StringLength(200)]
        public string VacationOrderSignature { get; set; }
    }
}
