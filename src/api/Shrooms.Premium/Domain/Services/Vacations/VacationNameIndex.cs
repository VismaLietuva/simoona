using System.Collections.Generic;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Matches a payroll file's names to directory users. Both name orders,
    /// because payroll exports "Surname Firstname" while the directory holds them
    /// the other way round. Diacritics are stripped, so "Rūta Trumpauskaitė" and
    /// "Ruta Trumpauskaite" are the same person.
    /// </summary>
    public static class VacationNameIndex
    {
        public static Dictionary<string, ApplicationUser> Build(IEnumerable<ApplicationUser> users)
        {
            var index = new Dictionary<string, ApplicationUser>();
            var ambiguous = new HashSet<string>();

            foreach (var user in users)
            {
                var first = VacationEntitlementParser.Normalize(user.FirstName);
                var last = VacationEntitlementParser.Normalize(user.LastName);

                if (first.Length == 0 && last.Length == 0)
                {
                    continue;
                }

                foreach (var key in new[] { $"{first} {last}".Trim(), $"{last} {first}".Trim() })
                {
                    if (index.TryGetValue(key, out var claimed))
                    {
                        // Two people answer to the same written name — "Jonas
                        // Petras" and "Petras Jonas" claim each other's reversed
                        // form. Guessing would charge somebody else's leave, so
                        // the name resolves to nobody and the row is reported.
                        if (claimed.Id != user.Id)
                        {
                            ambiguous.Add(key);
                        }

                        continue;
                    }

                    index[key] = user;
                }
            }

            foreach (var key in ambiguous)
            {
                index.Remove(key);
            }

            return index;
        }

        public static bool TryFind(
            IReadOnlyDictionary<string, ApplicationUser> index,
            string name,
            out ApplicationUser user)
        {
            return index.TryGetValue(VacationEntitlementParser.Normalize(name), out user);
        }
    }
}
