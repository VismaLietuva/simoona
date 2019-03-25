using System;
using System.Collections.Generic;
using System.Threading;

namespace Shrooms.Infrastructure.Retryer
{
    public static class Retry
    {
        public static void Do(Action action, TimeSpan delay, int retryCount)
        {
            var exceptions = new List<Exception>();

            for (var retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        Thread.Sleep(delay);
                    }

                    action();
                    return;
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
