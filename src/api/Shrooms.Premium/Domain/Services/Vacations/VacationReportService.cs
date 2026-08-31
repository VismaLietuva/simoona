using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Resx = Shrooms.Resources.Models.Vacations.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationReportService : IVacationReportService
    {
        private const string MonthFormat = "yyyy-MM";
        private const char Separator = ';';

        private readonly IUnitOfWork2 _uow;
        private readonly IHolidayService _holidayService;
        private readonly DbSet<VacationRequest> _requestDbSet;
        private readonly DbSet<VacationRequestEvent> _eventDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly DbSet<Organization> _organizationDbSet;

        public VacationReportService(IUnitOfWork2 uow, IHolidayService holidayService)
        {
            _uow = uow;
            _holidayService = holidayService;
            _requestDbSet = uow.GetDbSet<VacationRequest>();
            _eventDbSet = uow.GetDbSet<VacationRequestEvent>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<VacationDocumentDto> GetReportAsync(string from, string to, UserAndOrganizationDto userOrg)
        {
            var (start, end) = ResolveRange(from, to, await _organizationDbSet.TimeZoneAsync(userOrg.OrganizationId));

            // Overlap, not containment: leave starting in July and ending in
            // August belongs on the August report too.
            var requests = await _requestDbSet
                .AsNoTracking()
                // Users are soft-deleted and the employee navigation is
                // required, so the join drops the row once somebody leaves —
                // and leave already granted still has to be paid.
                .IgnoreQueryFilters()
                .Include(request => request.Employee)
                .Where(request => request.OrganizationId == userOrg.OrganizationId
                                  && request.Status == VacationRequestStatus.Approved
                                  && request.DateTo >= start
                                  && request.DateFrom <= end)
                .OrderBy(request => request.DateFrom)
                .ThenBy(request => request.Employee.FirstName)
                .ThenBy(request => request.Employee.LastName)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.Append("name").Append(Separator)
                .Append("dateFrom").Append(Separator)
                .Append("dateTo").Append(Separator)
                .Append("type").Append('\n');

            foreach (var request in requests)
            {
                builder
                    .Append(Escape($"{request.Employee?.FirstName} {request.Employee?.LastName}".Trim())).Append(Separator)
                    .Append(VacationWireFormat.ToDay(request.DateFrom)).Append(Separator)
                    .Append(VacationWireFormat.ToDay(request.DateTo)).Append(Separator)
                    .Append(VacationWireFormat.TypeToReportLetter(request.Type))
                    .Append('\n');
            }

            return new VacationDocumentDto
            {
                FileName = FileNameFor(start, end),
                ContentType = "text/csv",
                // A BOM, or Excel reads the Lithuanian names as the local ANSI
                // code page and mangles every diacritic.
                Content = new UTF8Encoding(true).GetBytes(builder.ToString())
            };
        }

        public async Task<VacationReportImportDto> ImportAsync(
            Stream fileStream,
            string fileName,
            UserAndOrganizationDto userOrg)
        {
            ReportParseResult parsed;
            try
            {
                parsed = VacationReportParser.ParseCsv(await ReadAllTextAsync(fileStream));
            }
            catch (Exception ex) when (ex is not VacationValidationException)
            {
                throw Unreadable();
            }

            var report = new VacationReportImportDto
            {
                FileName = fileName,
                Imported = new List<VacationReportRowDto>(),
                Duplicates = new List<VacationReportRowDto>(),
                Errors = parsed.UnreadableLines
                    .Select(line => new VacationReportRowErrorDto { Line = line, Reason = "badColumns" })
                    .ToList()
            };

            if (parsed.Rows.Count == 0 && report.Errors.Count == 0)
            {
                throw Unreadable();
            }

            var byName = VacationNameIndex.Build(await _userDbSet
                .Where(user => user.OrganizationId == userOrg.OrganizationId)
                .ToListAsync());

            // Only the active ones: a cancelled or rejected period is not leave
            // anybody has, so payroll listing it again is a fresh grant.
            var existing = await _requestDbSet
                .Where(request => request.OrganizationId == userOrg.OrganizationId
                                  && (request.Status == VacationRequestStatus.Pending
                                      || request.Status == VacationRequestStatus.Approved))
                .ToListAsync();

            var byEmployee = existing
                .GroupBy(request => request.EmployeeId)
                .ToDictionary(group => group.Key, group => group.ToList());

            var holidays = await _holidayService.GetCalendarAsync();

            var now = DateTime.UtcNow;

            foreach (var row in parsed.Rows)
            {
                var type = VacationWireFormat.ParseReportLetter(row.Type);
                var from = VacationWireFormat.ParseDay(row.DateFrom);
                var to = VacationWireFormat.ParseDay(row.DateTo);

                if (type == null)
                {
                    report.Errors.Add(Error(row, "badType"));
                    continue;
                }

                if (from == null || to == null || to < from)
                {
                    report.Errors.Add(Error(row, "badDates"));
                    continue;
                }

                if (!VacationNameIndex.TryFind(byName, row.Name, out var employee))
                {
                    report.Errors.Add(Error(row, "unknownEmployee"));
                    continue;
                }

                var workingDays = VacationCalculator.CountWorkingDays(from.Value, to.Value, holidays);
                if (workingDays == 0)
                {
                    report.Errors.Add(Error(row, "noWorkingDays"));
                    continue;
                }

                if (!byEmployee.TryGetValue(employee.Id, out var own))
                {
                    own = new List<VacationRequest>();
                    byEmployee[employee.Id] = own;
                }

                var duplicate = own.FirstOrDefault(request => request.Type == type
                                                              && request.DateFrom.Date == from.Value
                                                              && request.DateTo.Date == to.Value);
                if (duplicate != null)
                {
                    report.Duplicates.Add(Describe(row, employee, from.Value, to.Value, type.Value, duplicate.WorkingDays));
                    continue;
                }

                // Any type, matching what the app refuses on submission: two
                // periods over the same day are a contradiction to resolve by
                // hand, not something an import should guess at.
                var clash = own.FirstOrDefault(request =>
                    VacationCalculator.RangesOverlap(from.Value, to.Value, request.DateFrom, request.DateTo));
                if (clash != null)
                {
                    var error = Error(row, "overlap");
                    error.DateFrom = VacationWireFormat.ToDay(clash.DateFrom);
                    error.DateTo = VacationWireFormat.ToDay(clash.DateTo);
                    report.Errors.Add(error);
                    continue;
                }

                var created = new VacationRequest
                {
                    OrganizationId = userOrg.OrganizationId,
                    EmployeeId = employee.Id,
                    Type = type.Value,
                    // Payroll's report is the record of leave already granted, so
                    // it arrives decided rather than waiting for an approval.
                    Status = VacationRequestStatus.Approved,
                    DateFrom = from.Value,
                    DateTo = to.Value,
                    WorkingDays = workingDays,
                    ReviewedAt = now,
                    ReviewedById = userOrg.UserId,
                    Created = now,
                    Modified = now
                };

                _requestDbSet.Add(created);
                _eventDbSet.Add(new VacationRequestEvent
                {
                    OrganizationId = userOrg.OrganizationId,
                    VacationRequest = created,
                    Kind = VacationEventKind.Approved,
                    ActorId = userOrg.UserId,
                    OccurredAt = now,
                    EmployeeId = employee.Id,
                    Type = type.Value,
                    DateFrom = from.Value,
                    DateTo = to.Value,
                    WorkingDays = workingDays,
                    // The file name is the provenance: the log is where somebody
                    // asks why a period nobody requested exists.
                    Comment = Truncate(fileName, VacationRequestEvent.MaxCommentLength),
                    Created = now,
                    Modified = now
                });

                // Added to the in-memory list too, so the same period twice in
                // one file is the second row's duplicate.
                own.Add(created);
                report.Imported.Add(Describe(row, employee, from.Value, to.Value, type.Value, workingDays));
            }

            if (report.Imported.Count > 0)
            {
                await _uow.SaveChangesAsync(userOrg.UserId);
            }

            return report;
        }

        private static VacationReportRowDto Describe(
            ReportRow row,
            ApplicationUser employee,
            DateTime from,
            DateTime to,
            VacationRequestType type,
            double workingDays)
        {
            return new VacationReportRowDto
            {
                Line = row.Line,
                Name = $"{employee.FirstName} {employee.LastName}".Trim(),
                EmployeeId = employee.Id,
                DateFrom = VacationWireFormat.ToDay(from),
                DateTo = VacationWireFormat.ToDay(to),
                Type = VacationWireFormat.TypeToWire(type),
                WorkingDays = workingDays
            };
        }

        private static VacationReportRowErrorDto Error(ReportRow row, string reason)
        {
            return new VacationReportRowErrorDto { Line = row.Line, Name = row.Name, Reason = reason };
        }

        private static VacationValidationException Unreadable()
        {
            return new VacationValidationException(
                ErrorCodes.VacationImportUnreadable,
                "importUnreadable",
                Resx.GetResourceString("importUnreadable"));
        }

        private static string Truncate(string value, int max)
        {
            var text = value ?? string.Empty;
            return text.Length <= max ? text : text.Substring(0, max);
        }

        private static async Task<string> ReadAllTextAsync(Stream stream)
        {
            // detectEncodingFromByteOrderMarks: the export carries a BOM, and
            // reading it as the default code page mangles every Lithuanian name —
            // which then matches nobody.
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return await reader.ReadToEndAsync();
        }

        private static (DateTime From, DateTime To) ResolveRange(string from, string to, string timeZoneId)
        {
            if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
            {
                var today = VacationCalculator.TodayIn(timeZoneId);
                var first = new DateTime(today.Year, today.Month, 1);
                return (first, first.AddMonths(1).AddDays(-1));
            }

            var start = VacationWireFormat.ParseDay(from);
            var end = VacationWireFormat.ParseDay(to);

            if (start == null || end == null || end < start)
            {
                throw new VacationValidationException(
                    ErrorCodes.VacationDatesRequired,
                    "datesRequired",
                    Resx.GetResourceString("notEnoughData"));
            }

            return (start.Value, end.Value);
        }

        /// <summary>
        /// A whole calendar month keeps the "report_2026-08" name payroll already
        /// knows; anything else spells both ends out.
        /// </summary>
        private static string FileNameFor(DateTime start, DateTime end)
        {
            var wholeMonth = start.Day == 1 && end == start.AddMonths(1).AddDays(-1);

            return wholeMonth
                ? $"report_{start.ToString(MonthFormat, CultureInfo.InvariantCulture)}.csv"
                : $"report_{VacationWireFormat.ToDay(start)}_{VacationWireFormat.ToDay(end)}.csv";
        }

        /// <summary>Characters that make a spreadsheet treat a cell as a formula.</summary>
        private static readonly char[] FormulaTriggers = { '=', '+', '-', '@', '\t', '\r' };

        /// <summary>
        /// CSV quoting, plus a guard against formula injection: a spreadsheet
        /// evaluates a cell starting with "=" even inside quotes, so the leading
        /// apostrophe is what forces it to be read as text. A real name never
        /// starts with one of these.
        /// </summary>
        private static string Escape(string value)
        {
            var text = value ?? string.Empty;

            if (text.Length > 0 && Array.IndexOf(FormulaTriggers, text[0]) >= 0)
            {
                text = "'" + text;
            }

            return text.IndexOfAny(new[] { Separator, '"', '\n', '\r' }) >= 0
                ? $"\"{text.Replace("\"", "\"\"")}\""
                : text;
        }
    }
}
