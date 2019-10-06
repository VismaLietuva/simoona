using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Shrooms.Constants.BusinessLayer;
using Shrooms.EntityModels.Attributes;

namespace Shrooms.EntityModels.Models
{
    public class Organization : BaseModel
    {
        [Required]
        [StringLength(ConstBusinessLayer.MaxOrganizationNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(ConstBusinessLayer.MaxOrganizationShortNameLength)]
        public string ShortName { get; set; }

        [StringLength(50)]
        public string HostName { get; set; }

        [Required]
        public bool HasRestrictedAccess { get; set; }

        public virtual ICollection<Module> ShroomsModules { get; set; }

        [Required]
        [StringLength(ConstBusinessLayer.WelcomeEmailLength)]
        public string WelcomeEmail { get; set; }

        public bool RequiresUserConfirmation { get; set; }

        public string CalendarId { get; set; }

        public string TimeZone { get; set; }

        public string CultureCode { get; set; }

        public string BookAppAuthorizationGuid { get; set; }

        public string AuthenticationProviders { get; set; }

        public string KudosYearlyMultipliers { get; set; }


        [NotMapped]
        public int[] KudosYearlyMultipliersArray
        {
            get
            {
                if (string.IsNullOrEmpty(KudosYearlyMultipliers))
                {
                    return null;
                }

                try
                {
                    return Array.ConvertAll(KudosYearlyMultipliers.Split(';'), int.Parse);
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
