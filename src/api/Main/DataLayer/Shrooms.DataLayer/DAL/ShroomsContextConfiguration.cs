using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Shrooms.DataLayer.DAL
{
    public class ShroomsContextConfiguration : DbConfiguration
    {
        public ShroomsContextConfiguration()
        {
            var retryCount = Convert.ToInt16(ConfigurationManager.AppSettings["AzureSqlExecutionPolicyRetryCount"]);
            var maxDelay = Convert.ToInt16(ConfigurationManager.AppSettings["AzureSqlExecutionPolicyMaxDelayInSeconds"]);

            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy(retryCount, TimeSpan.FromSeconds(maxDelay)));
        }
    }
}
