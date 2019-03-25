using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Shrooms.API.Handlers
{
    public class CancelledTaskBugWorkaroundMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }

            return response;
        }
    }
}