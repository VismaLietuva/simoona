using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.WindowsAzure.Scheduler.Models;
using Shrooms.Host.Contracts.Infrastructure;

namespace Shrooms.Infrastructure.CloudScheduler.Jobs
{
    public class RecurringEvents : AzureJob
    {
        public RecurringEvents(string tenant, IApplicationSettings appSettings)
            : base("EventRecurrence", ConfigurationManager.AppSettings["RecurringEventEndpoint"], tenant, appSettings)
        {
        }

        public override JobCreateOrUpdateParameters GenerateAzureJobParameters()
        {
            var requestHeaders = new Dictionary<string, string>
            {
                { "Organization", Tenant }
            };

            var job = new JobCreateOrUpdateParameters
            {
                Action = new JobAction
                {
                    Type = JobActionType.Https,
                    Request = new JobHttpRequest
                    {
                        Authentication = new BasicAuthentication
                        {
                            Type = HttpAuthenticationType.Basic,
                            Password = Password,
                            Username = Username
                        },
                        Headers = requestHeaders,
                        Method = "POST",
                        Uri = new Uri(ActionUrl)
                    }
                },
                StartTime = DateTime.UtcNow,
                Recurrence = new JobRecurrence
                {
                    Frequency = JobRecurrenceFrequency.Minute,
                    Interval = 5
                }
            };

            return job;
        }
    }
}
