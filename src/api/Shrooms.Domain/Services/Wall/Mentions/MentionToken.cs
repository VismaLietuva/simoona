namespace Shrooms.Domain.Services.Wall.Mentions
{
    public enum MentionKind
    {
        User,
        Group
    }

    /// <summary>
    /// One mention as it appears in a post or comment body. The body is the only
    /// durable record of a mention - nothing is persisted alongside it - so the
    /// token carries the identity and the label is display only.
    /// </summary>
    public class MentionToken
    {
        public MentionKind Kind { get; set; }

        public string Label { get; set; }

        public string UserId { get; set; }

        public int GroupId { get; set; }
    }
}
