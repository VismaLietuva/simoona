using System;

namespace Shrooms.EntityModels.Models
{
    public interface ITrackable
    {
        DateTime Created { get; set; }

        string CreatedBy { get; set; }

        DateTime Modified { get; set; }

        string ModifiedBy { get; set; }
    }
}
