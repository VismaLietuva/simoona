using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Domain.DomainExceptions.Vacation;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationService : IVacationService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly TelemetryClient _telemetryClient;

        private readonly DbSet<ApplicationUser> _applicationUserDbSet;
        private readonly IVacationDomainService _vacationDomainService;

        private const int CodeColIndex = 0;
        private const int FullnameColIndex = 1;
        private const int OperationColIndex = 3;
        private const int OfficeColIndex = 4;
        private const int JobTitleColIndex = 5;
        private const int VacationTotalTimeColIndex = 6;
        private const int VacationUsedTimeColIndex = 7;
        private const int VacationUnusedTimeColIndex = 8;

        public VacationService(IUnitOfWork2 unitOfWork2, IVacationDomainService vacationDomainService)
        {
            _uow = unitOfWork2;
            _telemetryClient = new TelemetryClient(Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.CreateDefault());

            _applicationUserDbSet = unitOfWork2.GetDbSet<ApplicationUser>();
            _vacationDomainService = vacationDomainService;
        }

        public async Task<VacationImportStatusDto> UploadVacationReportFileAsync(Stream fileStream)
        {
            using var excelReader = ExcelReaderFactory.CreateReader(fileStream);

            var rows = ReadFirstSheetRows(excelReader);

            var importStatus = new VacationImportStatusDto
            {
                Imported = new List<VacationImportEntryDto>(),
                Skipped = new List<VacationImportEntryDto>()
            };

            foreach (var row in rows)
            {
                if (row.Length <= VacationUnusedTimeColIndex)
                {
                    continue;
                }

                var acceptableData = row[CodeColIndex] is string && row[FullnameColIndex] is string
                                          && row[OperationColIndex] is string && row[OfficeColIndex] is string
                                          && row[JobTitleColIndex] is string
                                          && (row[VacationTotalTimeColIndex] is double || row[VacationTotalTimeColIndex] is int)
                                          && (row[VacationUsedTimeColIndex] is double || row[VacationUsedTimeColIndex] is int)
                                          && (row[VacationUnusedTimeColIndex] is double || row[VacationUnusedTimeColIndex] is int);

                if (!acceptableData)
                {
                    continue;
                }

                var fullName = row[FullnameColIndex].ToString();
                var code = row[CodeColIndex].ToString();
                var users = _applicationUserDbSet.Where(_vacationDomainService.UsersByNamesFilter(fullName).Compile()).ToList();
                var userToUpdate = _vacationDomainService.FindUser(users, fullName);

                if (userToUpdate != null)
                {
                    var fullTime = Convert.ToDouble(row[VacationTotalTimeColIndex]);
                    var usedTime = Convert.ToDouble(row[VacationUsedTimeColIndex]);
                    var unusedTime = Convert.ToDouble(row[VacationUnusedTimeColIndex]);

                    userToUpdate.VacationTotalTime = fullTime;
                    userToUpdate.VacationUsedTime = usedTime;
                    userToUpdate.VacationUnusedTime = unusedTime;
                    userToUpdate.VacationLastTimeUpdated = DateTime.UtcNow;

                    importStatus.Imported.Add(new VacationImportEntryDto { Code = code, FullName = fullName });
                }
                else
                {
                    var exception = new VacationImportException($"User wasn't found during import - entry code: {code}, fullname: {fullName}");

                    var exceptionTelemetry = new ExceptionTelemetry
                    {
                        Message = exception.Message,
                        Exception = exception
                    };

                    exceptionTelemetry.Properties.Add("Entry code", code);
                    exceptionTelemetry.Properties.Add("Entry last name, first name", fullName);
                    _telemetryClient.TrackException(exceptionTelemetry);

                    importStatus.Skipped.Add(new VacationImportEntryDto { Code = code, FullName = fullName });
                }
            }

            await _uow.SaveChangesAsync();

            return importStatus;
        }

        public async Task<VacationAvailableDaysDto> GetAvailableDaysAsync(UserAndOrganizationDto userOrgDto)
        {
            var user = await _applicationUserDbSet
                .FirstAsync(u => u.Id == userOrgDto.UserId);

            var availableDaysModel = new VacationAvailableDaysDto
            {
                AvailableDays = Math.Truncate(user.VacationUnusedTime ?? 0),
                LastTimeUpdated = user.VacationLastTimeUpdated
            };

            return availableDaysModel;
        }

        private static IEnumerable<object[]> ReadFirstSheetRows(IExcelDataReader reader)
        {
            var rows = new List<object[]>();
            while (reader.Read())
            {
                var row = new object[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[i] = reader.GetValue(i);
                }
                rows.Add(row);
            }
            return rows;
        }
    }
}