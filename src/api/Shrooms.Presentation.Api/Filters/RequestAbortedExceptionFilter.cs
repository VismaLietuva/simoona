using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Shrooms.Presentation.Api.Filters
{
    /// <summary>
    /// GET actions forward HttpContext.RequestAborted to their queries, so a caller that
    /// disconnects surfaces as an OperationCanceledException. Answer it with 499 and a debug
    /// log instead of letting it count as a server error.
    /// </summary>
    public class RequestAbortedExceptionFilter : IExceptionFilter
    {
        // Not an official status code; nginx uses it for "client closed request".
        public const int ClientClosedRequest = 499;

        private readonly ILogger<RequestAbortedExceptionFilter> _logger;

        public RequestAbortedExceptionFilter(ILogger<RequestAbortedExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            // A timeout or a token of the action's own is a real failure; only the caller
            // going away is expected.
            if (context.Exception is not OperationCanceledException ||
                !context.HttpContext.RequestAborted.IsCancellationRequested)
            {
                return;
            }

            _logger.LogDebug("Request {Method} {Path} was aborted by the caller", context.HttpContext.Request.Method, context.HttpContext.Request.Path);

            context.Result = new StatusCodeResult(ClientClosedRequest);
            context.ExceptionHandled = true;
        }
    }
}
