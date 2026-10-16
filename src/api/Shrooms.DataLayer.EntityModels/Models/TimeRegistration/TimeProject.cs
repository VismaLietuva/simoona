using System.Collections.Generic;

namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimeProject : SoftDeletableModelWithOrg
    {
        public const int MaxNameLength = 100;

        public int CustomerId { get; set; }

        public virtual TimeCustomer Customer { get; set; }

        public string Name { get; set; }

        public bool IsArchived { get; set; }

        public virtual ICollection<TimeTask> Tasks { get; set; }

        public virtual ICollection<TimeProjectMember> Members { get; set; }
    }
}
