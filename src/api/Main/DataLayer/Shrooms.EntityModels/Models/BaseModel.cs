using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Shrooms.EntityModels.Models.Multiwall;

namespace Shrooms.EntityModels.Models
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

    public abstract class LikeBaseModel : BaseModel, ILikeable
    {
        public virtual LikesCollection Likes { get; set; }

        public bool IsLikedByUser(string userId)
        {
            return Likes.Any(a => a.UserId == userId);
        }
    }
}