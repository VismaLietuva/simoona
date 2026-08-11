namespace Shrooms.Contracts.Enums
{
    /// <summary>
    /// Who may create groups of a given type. The restrictive value is the default,
    /// so a type created without an explicit choice does not open group creation up.
    /// </summary>
    public enum GroupCreationPolicy
    {
        AdminOnly = 0,
        Open = 1,
        RequiresApproval = 2
    }
}
