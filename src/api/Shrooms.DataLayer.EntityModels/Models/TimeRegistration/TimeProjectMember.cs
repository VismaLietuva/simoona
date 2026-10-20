namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimeProjectMember : BaseModelWithOrg
    {
        public int ProjectId { get; set; }

        public virtual TimeProject Project { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public bool IsAutoFill { get; set; }
    }
}
