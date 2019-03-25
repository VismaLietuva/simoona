using System;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.Azure;

namespace Shrooms.Infrastructure.CloudScheduler
{
    internal static class AzureCredentialsFactory
    {
        private static readonly string AzureSubscriptionId = ConfigurationManager.AppSettings["AzureSubscriptionId"];
        private static readonly string AzureSubscriptionCertThumb = ConfigurationManager.AppSettings["AzureSubscriptionCertThumb"];

        public static CertificateCloudCredentials FromCertificateStore()
        {
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            var cert = store.Certificates.Find(X509FindType.FindByThumbprint, AzureSubscriptionCertThumb, false);
            store.Close();
            return new CertificateCloudCredentials(AzureSubscriptionId, cert[0]);
        }

        public static CertificateCloudCredentials FromPublishSettingsFile(string path, string subscriptionName)
        {
            var profile = XDocument.Load(path);
            var subscriptionId = profile.Descendants("Subscription")
                .First(element => element.Attribute("Name").Value == subscriptionName)
                .Attribute("Id").Value;
            var certificate = new X509Certificate2(
                Convert.FromBase64String(profile.Descendants("PublishProfile").Descendants("Subscription").Single().Attribute("ManagementCertificate").Value));
            return new CertificateCloudCredentials(subscriptionId, certificate);
        }
    }
}
