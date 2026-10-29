using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Shrooms.Presentation.Api.Filters;

namespace Shrooms.Tests.Filters
{
    [TestFixture]
    public class RequestAbortedExceptionFilterTests
    {
        private RequestAbortedExceptionFilter _filter;

        [SetUp]
        public void SetUp()
        {
            _filter = new RequestAbortedExceptionFilter(NullLogger<RequestAbortedExceptionFilter>.Instance);
        }

        [Test]
        public void OnException_CallerAborted_Returns499()
        {
            var context = BuildContext(new OperationCanceledException(), requestAborted: true);

            _filter.OnException(context);

            Assert.That(context.ExceptionHandled, Is.True);
            Assert.That(context.Result, Is.InstanceOf<StatusCodeResult>());
            Assert.That(((StatusCodeResult)context.Result).StatusCode, Is.EqualTo(RequestAbortedExceptionFilter.ClientClosedRequest));
        }

        [Test]
        public void OnException_TaskCanceledWhenCallerAborted_Returns499()
        {
            var context = BuildContext(new TaskCanceledException(), requestAborted: true);

            _filter.OnException(context);

            Assert.That(context.ExceptionHandled, Is.True);
        }

        [Test]
        public void OnException_CanceledWhileCallerStillConnected_LeavesExceptionUnhandled()
        {
            var context = BuildContext(new OperationCanceledException(), requestAborted: false);

            _filter.OnException(context);

            Assert.That(context.ExceptionHandled, Is.False);
            Assert.That(context.Result, Is.Null);
        }

        [Test]
        public void OnException_OtherExceptionAfterCallerAborted_LeavesExceptionUnhandled()
        {
            var context = BuildContext(new InvalidOperationException(), requestAborted: true);

            _filter.OnException(context);

            Assert.That(context.ExceptionHandled, Is.False);
            Assert.That(context.Result, Is.Null);
        }

        private static ExceptionContext BuildContext(Exception exception, bool requestAborted)
        {
            var httpContext = new DefaultHttpContext
            {
                RequestAborted = new CancellationToken(requestAborted)
            };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new ExceptionContext(actionContext, new List<IFilterMetadata>())
            {
                Exception = exception
            };
        }
    }
}
