using System;

namespace Shrooms.Premium.Domain.DomainExceptions.Lotteries
{
    public class LotteryException : Exception
    {
        public LotteryException(string message) : base(message)
        {
        }
    }
}
