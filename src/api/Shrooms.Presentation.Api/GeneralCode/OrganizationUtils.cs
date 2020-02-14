using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;

namespace Shrooms.Presentation.Api.GeneralCode
{
    public static class OrganizationUtils
    {
        public static readonly Dictionary<string, string> AvailableOrganizations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly NameValueCollection _organizationsDicts = ConfigurationManager.GetSection("RegisteredOrganizations") as NameValueCollection;

        static OrganizationUtils()
        {
            foreach (string org in _organizationsDicts)
            {
                var value = _organizationsDicts[org];
                AvailableOrganizations.Add(org, value);
            }
        }
    }
}