using System;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class Banner : BaseModelWithOrg
    {
        public string Url { get; set; }
        public string PictureId { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
