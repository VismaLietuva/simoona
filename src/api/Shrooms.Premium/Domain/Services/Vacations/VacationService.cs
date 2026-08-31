using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;
using Resx = Shrooms.Resources.Models.Vacations.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationService : IVacationService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly TelemetryClient _telemetryClient;

        private readonly DbSet<ApplicationUser> _applicationUserDbSet;

        public VacationService(IUnitOfWork2 unitOfWork2, TelemetryClient telemetryClient)
        {
            _uow = unitOfWork2;
            _telemetryClient = telemetryClient;

            _applicationUserDbSet = unitOfWork2.GetDbSet<ApplicationUser>();
        }

        public async Task<VacationEntitlementImportDto> ImportEntitlementsAsync(
            Stream fileStream,
            string fileName,
            string asOf,
            UserAndOrganizationDto userOrg)
        {
            EntitlementParseResult parsed;
            try
            {
                parsed = IsCsv(fileName)
                    ? VacationEntitlementParser.ParseCsv(await ReadAllTextAsync(fileStream))
                    : VacationEntitlementParser.ParseExcel(fileStream);
            }
            catch (Exception ex) when (ex is not VacationValidationException)
            {
                // A file that is not a workbook at all makes ExcelDataReader
                // throw on the first read. That is a bad upload, not a server
                // fault, so it comes back as a refusal the dialog can show.
                throw new VacationValidationException(
                    ErrorCodes.VacationImportUnreadable,
                    "importUnreadable",
                    Resx.GetResourceString("importUnreadable"));
            }

            // The supplied date wins; the export's own preamble is only a
            // fallback, because getting this wrong silently mis-dates every
            // balance in the organisation.
            var measuredAt = VacationWireFormat.ParseDay(asOf) ?? parsed.DetectedAsOf;
            if (measuredAt == null)
            {
                throw new VacationValidationException(
                    ErrorCodes.VacationImportDateRequired,
                    "importDateRequired",
                    Resx.GetResourceString("importDateRequired"));
            }

            var users = await _applicationUserDbSet
                .Where(user => user.OrganizationId == userOrg.OrganizationId)
                .ToListAsync();

            var byName = VacationNameIndex.Build(users);

            var report = new VacationEntitlementImportDto
            {
                AsOf = VacationWireFormat.ToDay(measuredAt.Value),
                FileName = fileName,
                Imported = new List<VacationEntitlementEntryDto>(),
                Skipped = new List<VacationEntitlementSkipDto>(),
                Unreadable = parsed.Unreadable
            };

            foreach (var row in parsed.Rows)
            {
                if (!byName.TryGetValue(VacationEntitlementParser.Normalize(row.Name), out var user))
                {
                    TrackMissingUser(row);
                    report.Skipped.Add(new VacationEntitlementSkipDto { Code = row.Code, Name = row.Name });
                    continue;
                }

                var previous = user.VacationUnusedTime ?? 0;

                // Total and used are imported but displayed nowhere yet; only
                // when the export actually carries them, so a leaner CSV cannot
                // blank figures a fuller import had already set.
                if (row.Total.HasValue)
                {
                    user.VacationTotalTime = row.Total.Value;
                }

                if (row.Used.HasValue)
                {
                    user.VacationUsedTime = row.Used.Value;
                }

                user.VacationUnusedTime = row.Unused;
                user.VacationLastTimeUpdated = measuredAt.Value;

                report.Imported.Add(new VacationEntitlementEntryDto
                {
                    Code = row.Code,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    EmployeeId = user.Id,
                    From = previous,
                    To = row.Unused
                });
            }

            await _uow.SaveChangesAsync(userOrg.UserId);

            return report;
        }

        public async Task<VacationAvailableDaysDto> GetAvailableDaysAsync(UserAndOrganizationDto userOrgDto)
        {
            var user = await _applicationUserDbSet
                .AsNoTracking()
                .FirstAsync(u => u.Id == userOrgDto.UserId);

            return new VacationAvailableDaysDto
            {
                AvailableDays = Math.Truncate(user.VacationUnusedTime ?? 0),
                LastTimeUpdated = user.VacationLastTimeUpdated
            };
        }

        private void TrackMissingUser(EntitlementRow row)
        {
            var exception = new VacationImportException(
                $"User wasn't found during import - entry code: {row.Code}, fullname: {row.Name}");

            var telemetry = new ExceptionTelemetry
            {
                Message = exception.Message,
                Exception = exception
            };

            telemetry.Properties.Add("Entry code", row.Code ?? string.Empty);
            telemetry.Properties.Add("Entry last name, first name", row.Name ?? string.Empty);
            _telemetryClient.TrackException(telemetry);
        }

        private static bool IsCsv(string fileName)
        {
            var name = fileName ?? string.Empty;
            return name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                   || name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task<string> ReadAllTextAsync(Stream stream)
        {
            // detectEncodingFromByteOrderMarks: the payroll export is UTF-8 with
            // a BOM, and reading it as the default code page mangles every
            // Lithuanian name — which then matches nobody.
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return await reader.ReadToEndAsync();
        }
    }
}
