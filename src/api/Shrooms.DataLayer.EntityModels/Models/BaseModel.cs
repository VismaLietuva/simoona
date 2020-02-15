using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class BaseModel : ITrackable, ISoftDelete, IValidatableObject
    {
        [Key]
        public virtual int Id { get; set; }

        public DateTime Created { get; set; }

        public string CreatedBy { get; set; }

        public DateTime Modified { get; set; }

        public string ModifiedBy { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}