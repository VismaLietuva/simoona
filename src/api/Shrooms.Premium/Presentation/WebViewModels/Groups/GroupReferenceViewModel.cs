using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.Groups
{
    public class GroupReferenceViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Url { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        public bool IsPubliclyVisible { get; set; }
    }
}
