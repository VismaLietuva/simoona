using System.ComponentModel.DataAnnotations;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Page : SoftDeletableModelWithOrg
    {
        [Required]
        public string Name { get; set; }

        public int? ParentPageId { get; set; }

        public virtual Page ParentPage { get; set; }
    }
}