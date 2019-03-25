using System;
using System.Net;
using Microsoft.Azure;

namespace Shrooms.Infrastructure.CloudScheduler
{
    public class JobSchedulerException : Exception
    {
        public JobSchedulerException(AzureOperationResponse response)
            : base("Scheduled job was not created/updated")
        {
            Response = response.StatusCode;
            RequestId = response.RequestId;
        }

        public HttpStatusCode Response { get; }

        public string RequestId { get; }
    }
}
