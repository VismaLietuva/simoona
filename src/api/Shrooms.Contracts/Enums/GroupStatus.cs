namespace Shrooms.Contracts.Enums
{
    /// <summary>
    /// Pending is the default so a group is never visible by accident - it has to be
    /// approved, or created under a policy that approves it outright.
    /// </summary>
    public enum GroupStatus
    {
        Pending = 0,
        Approved = 1
    }
}
