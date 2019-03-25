using System;
using System.Collections.Generic;
using System.Linq;
using Shrooms.DomainExceptions.Exceptions.UserAdministration;

namespace DomainServiceValidators.Validators.UserAdministration
{
    public class UserAdministrationValidator : IUserAdministrationValidator
    {
        public void CheckIfEmploymentDateIsSet(DateTime? employmentDate)
        {
            if (!employmentDate.HasValue)
            {
                throw new UserAdministrationException("Employment date is not valid");
            }
        }

        public void CheckIfUserHasFirstLoginRole(bool hasRole)
        {
            if (hasRole)
            {
                throw new UserAdministrationException("User has not filled info yet");
            }
        }

        public void CheckForAddingRemovingRoleErrors(IEnumerable<string> addRoleErrors, IEnumerable<string> removeRoleErrors)
        {
            if (addRoleErrors.Any() || removeRoleErrors.Any())
            {
                var errors = addRoleErrors.Concat(removeRoleErrors).Distinct();
                throw new UserAdministrationException(errors);
            }
        }
    }
}
