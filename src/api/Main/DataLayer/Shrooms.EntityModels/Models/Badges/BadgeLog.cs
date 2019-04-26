namespace Shrooms.EntityModels.Models.Badges
{
    public class BadgeLog : BaseModelWithOrg
    {
        public string EmployeeId { get; set; }
        public int BadgeTypeId { get; set; }

        public virtual ApplicationUser Employee { get; set; }
        public virtual BadgeType BadgeType { get; set; }
    }
}
