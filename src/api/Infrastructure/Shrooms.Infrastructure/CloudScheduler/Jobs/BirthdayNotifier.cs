using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.WindowsAzure.Scheduler.Models;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.Infrastructure.CloudScheduler.Jobs
{
    public class BirthdayNotifier : AzureJob
    {
        private readonly DateTime _executionTime = DateTime.Today.AddHours(6);

        public BirthdayNotifier(string tenant, IApplicationSettings appSettings)
            : base("BirthdaysReminder", ConfigurationManager.AppSettings["BirthdayNotificationEndpoint"], tenant, appSettings)
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
                StartTime = _executionTime,
                Recurrence = new JobRecurrence
                {
                    Frequency = JobRecurrenceFrequency.Day
                }
            };

            return job;
        }
    }
}
