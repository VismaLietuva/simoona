using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IVacationDomainService
    {
        Expression<Func<ApplicationUser, bool>> UsersByNamesFilter(string fullName);

        ApplicationUser FindUser(IList<ApplicationUser> users, string fullName);
    }
}