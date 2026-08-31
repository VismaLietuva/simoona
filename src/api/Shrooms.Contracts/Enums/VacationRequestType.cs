namespace Shrooms.Contracts.Enums
{
    /// <summary>
    /// Leave types. Persisted as int; the API serialises them as the lower-case
    /// wire strings the client uses ("annual", "parental", "unpaid").
    /// </summary>
    public enum VacationRequestType
    {
        Annual = 0,
        Parental = 1,
        Unpaid = 2
    }
}
