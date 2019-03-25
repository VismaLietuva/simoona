using System;
using System.Collections.Generic;

namespace DomainServiceValidators.Validators.UserAdministration
{
    public interface IUserAdministrationValidator
    {
        void CheckIfEmploymentDateIsSet(DateTime? employmentDate);
        void CheckIfUserHasFirstLoginRole(bool hasRole);
        void CheckForAddingRemovingRoleErrors(IEnumerable<string> addRoleErrors, IEnumerable<string> removeRoleErrors);
    }
}
