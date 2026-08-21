namespace Shrooms.Contracts.Enums
{
    /// <summary>
    /// What an audit-log row records. One row per action, never updated.
    /// </summary>
    public enum VacationEventKind
    {
        Submitted = 0,
        Edited = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }
}
