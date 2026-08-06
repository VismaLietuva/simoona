using System.ComponentModel.DataAnnotations.Schema;

namespace Shrooms.DataLayer.EntityModels.Models.Group
{
    /// <summary>
    /// A link belonging to a group. Replaces the single Website field.
    /// </summary>
    public class GroupReference : BaseModel
    {
        [ForeignKey("Group")]
        public int GroupId { get; set; }

        public virtual Group Group { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// Label shown instead of the raw URL.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// When false the reference is only shown to members of the group.
        /// </summary>
        public bool IsPubliclyVisible { get; set; }
    }
}
