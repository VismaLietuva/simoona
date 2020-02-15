namespace Shrooms.DataLayer.EntityModels.Models.Badges
{
    public class BadgeType : BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string ImageSmallUrl { get; set; }
        public int Value { get; set; }

        public int BadgeCategoryId { get; set; }
        public virtual BadgeCategory BadgeCategory { get; set; }

        public bool IsActive { get; set; }
    }
}
