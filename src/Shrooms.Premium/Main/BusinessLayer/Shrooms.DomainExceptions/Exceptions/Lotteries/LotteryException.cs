using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DomainExceptions.Exceptions.Lotteries
{
    public class LotteryException : Exception
    {
        public LotteryException(string message) : base (message)
        {
            
        }
    }
}
