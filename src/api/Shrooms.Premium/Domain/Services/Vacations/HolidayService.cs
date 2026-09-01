using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    /// <summary>
    /// Reads the holiday calendar. Not cached: the table holds roughly ten rows a
    /// year, which is cheaper to read than it would be to invalidate.
    /// </summary>
    public class HolidayService : IHolidayService
    {
        private readonly DbSet<Holiday> _holidayDbSet;

        public HolidayService(IUnitOfWork2 uow)
        {
            _holidayDbSet = uow.GetDbSet<Holiday>();
        }

        public async Task<HolidayCalendar> GetCalendarAsync()
        {
            var days = await _holidayDbSet
                .AsNoTracking()
                .Select(holiday => holiday.Date)
                .ToListAsync();

            return new HolidayCalendar(days);
        }

        public async Task<IEnumerable<Holiday>> GetAsync(DateTime? from, DateTime? to)
        {
            var query = _holidayDbSet.AsNoTracking();

            if (from.HasValue)
            {
                var start = from.Value.Date;
                query = query.Where(holiday => holiday.Date >= start);
            }

            if (to.HasValue)
            {
                var end = to.Value.Date;
                query = query.Where(holiday => holiday.Date <= end);
            }

            return await query
                .OrderBy(holiday => holiday.Date)
                .ToListAsync();
        }
    }
}
