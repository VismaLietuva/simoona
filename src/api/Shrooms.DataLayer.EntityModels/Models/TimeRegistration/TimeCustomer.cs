using System.Collections.Generic;

namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimeCustomer : SoftDeletableModelWithOrg
    {
        public const int MaxNameLength = 100;

        public string Name { get; set; }

        public virtual ICollection<TimeProject> Projects { get; set; }
    }
}
