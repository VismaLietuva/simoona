namespace Shrooms.DataLayer.EntityModels.Models.TimeRegistration
{
    public class TimeTask : SoftDeletableModelWithOrg
    {
        public const int MaxNameLength = 100;
        public const int MaxCommentLength = 500;

        public int ProjectId { get; set; }

        public virtual TimeProject Project { get; set; }

        public string Name { get; set; }

        public string Comment { get; set; }

        public bool IsDefault { get; set; }
    }
}
