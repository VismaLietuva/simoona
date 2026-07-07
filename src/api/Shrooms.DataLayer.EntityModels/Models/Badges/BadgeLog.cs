namespace Shrooms.DataLayer.EntityModels.Models.Badges
{
    public class BadgeLog : SoftDeletableModelWithOrg
    {
        public string EmployeeId { get; set; }
        public int BadgeTypeId { get; set; }

        public virtual ApplicationUser Employee { get; set; }
        public virtual BadgeType BadgeType { get; set; }
    }
}
