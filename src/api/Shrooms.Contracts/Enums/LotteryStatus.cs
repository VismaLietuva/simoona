namespace Shrooms.Contracts.Enums
{
    public enum LotteryStatus
    {
        Drafted = 1,
        Started,
        Deleted,
        Finished, // User finished the lottery
        RefundStarted,
        RefundLogsCreated,
        Refunded,
        Ended // Time ran out
    }
}