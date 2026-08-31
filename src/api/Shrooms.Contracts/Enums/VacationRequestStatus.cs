namespace Shrooms.Contracts.Enums
{
    /// <summary>
    /// Request lifecycle. Pending and Approved are the "active" pair: only those
    /// two book someone out of the office or charge the balance.
    /// </summary>
    public enum VacationRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }
}
