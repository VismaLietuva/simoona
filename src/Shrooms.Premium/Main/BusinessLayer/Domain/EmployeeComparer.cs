using System.Collections.Generic;
using Shrooms.EntityModels.Models;

namespace Shrooms.Premium.Main.BusinessLayer.Domain
{
    internal class EmployeeComparer : IEqualityComparer<ApplicationUser>
    {
        public bool Equals(ApplicationUser x,
            ApplicationUser y)
        {
            if (x == null || y == null)
                return false;

            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(ApplicationUser obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
