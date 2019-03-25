namespace Shrooms.Azure
{
    public static class AzureSettings
    {
        public const int MaximumExecutionTimeInSeconds = 60;
        public const int ExponentialRetryDeltaBackoff = 2;
        public const int ExponentialRetryMaxAttempts = 10;
    }
}