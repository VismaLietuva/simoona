using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqKit;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationDomainService : IVacationDomainService
    {
        public Expression<Func<ApplicationUser, bool>> UsersByNamesFilter(string fullName)
        {
            var predicate = PredicateBuilder.False<ApplicationUser>();

            foreach (var name in fullName.Split(' '))
            {
                var temp = name;
                predicate = predicate.Or(p => p.FirstName == temp || p.LastName == temp);
            }

            return predicate;
        }

        public ApplicationUser FindUser(IList<ApplicationUser> users, string fullName)
        {
            fullName = OrderStringAlphabetically(fullName);

            return users.FirstOrDefault(f => OrderStringAlphabetically(f.FirstName + f.LastName) == fullName);
        }

        private static string OrderStringAlphabetically(string input)
        {
            return new string(input.OrderBy(o => o).ToArray()).Trim();
        }
    }
}