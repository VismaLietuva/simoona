using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public interface IHolidayService
    {
        /// <summary>The whole calendar, ready to hand to <see cref="VacationCalculator"/>.</summary>
        Task<HolidayCalendar> GetCalendarAsync();

        /// <summary>
        /// The holidays themselves, for the client to grey out. Both bounds are
        /// optional and inclusive; omitting them returns everything on file.
        /// </summary>
        Task<IEnumerable<Holiday>> GetAsync(DateTime? from, DateTime? to);
    }
}
