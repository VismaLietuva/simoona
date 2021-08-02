using System;
using System.Collections.Generic;

namespace Shrooms.Domain.ServiceValidators.Validators.UserAdministration
{
    public interface IUserAdministrationValidator
    {
        void CheckIfEmploymentDateIsSet(DateTime? employmentDate);
        void CheckIfUserHasFirstLoginRole(bool hasRole);
        void CheckForAddingRemovingRoleErrors(IList<string> addRoleErrors, IList<string> removeRoleErrors);
    }
}
