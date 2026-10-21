using System;
using System.Collections.Generic;
using Shrooms.Contracts.Enums;

namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeCalendar
    {
        public TimeCalendar(
            DateTime month,
            IReadOnlyDictionary<DateTime, string> holidays,
            IReadOnlyDictionary<DateTime, TimeAbsence> absences,
            int dailyMinutes,
            DateTime? employedFrom = null)
        {
            Month = month;
            EmployedFrom = employedFrom?.Date;
            Days = TimeMonth.Days(month);
            Holidays = holidays;
            Absences = absences;
            DailyMinutes = dailyMinutes;
        }

        public DateTime Month { get; }

        public IReadOnlyList<DateTime> Days { get; }

        public IReadOnlyDictionary<DateTime, string> Holidays { get; }

        public IReadOnlyDictionary<DateTime, TimeAbsence> Absences { get; }

        public int DailyMinutes { get; }

        public DateTime? EmployedFrom { get; }

        public bool IsWorkingDay(DateTime day)
        {
            return !TimeMonth.IsWeekend(day) && !Holidays.ContainsKey(day) && (EmployedFrom == null || day >= EmployedFrom);
        }

        public int ExpectedMinutes(DateTime day)
        {
            return IsWorkingDay(day) ? DailyMinutes : 0;
        }
    }

    public class TimeAbsence
    {
        public TimeAbsence(DateTime date, VacationRequestType type, VacationRequestStatus status)
        {
            Date = date;
            Type = type;
            Status = status;
        }

        public DateTime Date { get; }

        public VacationRequestType Type { get; }

        public VacationRequestStatus Status { get; }
    }
}
