using System;

namespace Shrooms.Premium.Main.BusinessLayer.DomainExceptions.Lotteries
{
    public class LotteryException : Exception
    {
        public LotteryException(string message) : base (message)
        {
        }
    }
}
