namespace Shrooms.Contracts.DataTransferObjects.Wall.Mentions
{
    public class MentionGroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// Members active today - the same rule the resolver uses when a tag of this
        /// group expands into the people to notify.
        /// </summary>
        public int MemberCount { get; set; }
    }
}
