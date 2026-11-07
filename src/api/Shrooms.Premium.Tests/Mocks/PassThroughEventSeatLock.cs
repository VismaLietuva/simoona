using System;
using System.Threading.Tasks;
using Shrooms.Premium.Domain.Services.Events.Participation;

namespace Shrooms.Premium.Tests.Mocks
{
    public class PassThroughEventSeatLock : IEventSeatLock
    {
        public bool Inside { get; private set; }

        public int Runs { get; private set; }

        public async Task RunAsync(Guid eventId, Func<Task> work)
        {
            Runs++;
            Inside = true;
            try
            {
                await work();
            }
            finally
            {
                Inside = false;
            }
        }

        public void AfterCommit(Action action) => action();
    }
}
