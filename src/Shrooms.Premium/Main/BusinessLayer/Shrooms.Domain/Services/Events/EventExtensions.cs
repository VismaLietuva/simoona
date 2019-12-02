using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Events;

namespace Shrooms.Domain.Services.Events
{
    public static class EventExtensions
    {
        public static IQueryable<Event> HappeningThisWeek(this IQueryable<Event> events) =>
            events.Where(IsCurrentWeek);

        public static Expression<Func<Event, bool>> IsCurrentWeek =>
            e => SqlFunctions.DatePart("wk", e.StartDate) == SqlFunctions.DatePart("wk", DateTime.UtcNow);
    }
}
