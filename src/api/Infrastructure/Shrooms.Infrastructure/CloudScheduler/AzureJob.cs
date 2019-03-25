using System;
using Microsoft.WindowsAzure.Scheduler.Models;
using Shrooms.Infrastructure.Configuration;

namespace Shrooms.Infrastructure.CloudScheduler
{
    public abstract class AzureJob
    {
        private readonly string _apiAddress;
        private readonly string _endpointPath;

        protected AzureJob(string name, string endpointPath, string tenant, IApplicationSettings appSettings)
        {
            _apiAddress = appSettings.ApiUrl;
            var username = appSettings.BasicUsername; // ConfigurationManager.AppSettings["BasicUsername"];
            var password = appSettings.BasicPassword; //ConfigurationManager.AppSettings["BasicPassword"];

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrEmpty(endpointPath))
            {
                throw new ArgumentNullException(nameof(endpointPath));
            }

            if (string.IsNullOrEmpty(tenant))
            {
                throw new ArgumentNullException(nameof(tenant));
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            if (string.IsNullOrEmpty(_apiAddress))
            {
                throw new ArgumentNullException(nameof(_apiAddress));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            Name = name;
            _endpointPath = endpointPath;
            Tenant = tenant;
            Username = username;
            Password = password;
        }

        public string Identifier => $"{Name}-{Tenant}";

        public string Name { get; }

        public string ActionUrl => $"{_apiAddress}{_endpointPath}";

        public string Tenant { get; }

        public string Username { get; }

        public string Password { get; }

        public abstract JobCreateOrUpdateParameters GenerateAzureJobParameters();
    }
}
