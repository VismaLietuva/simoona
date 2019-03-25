using System.ComponentModel.DataAnnotations;

namespace Shrooms.EntityModels.Models
{
    public class Page : BaseModelWithOrg
    {
        [Required]
        public string Name { get; set; }

        public int? ParentPageId { get; set; }

        public virtual Page ParentPage { get; set; }
    }
}