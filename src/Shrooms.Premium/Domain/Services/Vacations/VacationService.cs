using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Excel;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
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

        private readonly IDbSet<ApplicationUser> _applicationUserDbSet;
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
            _telemetryClient = new TelemetryClient();

            _applicationUserDbSet = unitOfWork2.GetDbSet<ApplicationUser>();
            _vacationDomainService = vacationDomainService;
        }

        public async Task<VacationImportStatusDTO> UploadVacationReportFileAsync(Stream fileStream)
        {
            var excelReader = ExcelReaderFactory.CreateBinaryReader(fileStream);

            var sheets = GetWorksheetNames(excelReader);
            var workSheet = GetWorksheetData(excelReader, sheets.First());

            var importStatus = new VacationImportStatusDTO
            {
                Imported = new List<VacationImportEntryDTO>(),
                Skipped = new List<VacationImportEntryDTO>()
            };

            foreach (var row in workSheet)
            {
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
                    var fullTime = (double)row[VacationTotalTimeColIndex];
                    var usedTime = (double)row[VacationUsedTimeColIndex];
                    var unusedTime = (double)row[VacationUnusedTimeColIndex];

                    userToUpdate.VacationTotalTime = fullTime;
                    userToUpdate.VacationUsedTime = usedTime;
                    userToUpdate.VacationUnusedTime = unusedTime;
                    userToUpdate.VacationLastTimeUpdated = DateTime.UtcNow;

                    importStatus.Imported.Add(new VacationImportEntryDTO { Code = code, FullName = fullName });
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

                    importStatus.Skipped.Add(new VacationImportEntryDTO { Code = code, FullName = fullName });
                }
            }

            await _uow.SaveChangesAsync();
            excelReader.Close();

            return importStatus;
        }

        public async Task<VacationAvailableDaysDTO> GetAvailableDaysAsync(UserAndOrganizationDTO userOrgDto)
        {
            var user = await _applicationUserDbSet
                .FirstAsync(u => u.Id == userOrgDto.UserId);

            var availableDaysModel = new VacationAvailableDaysDTO
            {
                AvailableDays = Math.Truncate(user.VacationUnusedTime ?? 0),
                LastTimeUpdated = user.VacationLastTimeUpdated
            };

            return availableDaysModel;
        }

        private static IEnumerable<string> GetWorksheetNames(IExcelDataReader excelReader)
        {
            var workbook = excelReader.AsDataSet();
            var sheets = from DataTable sheet in workbook.Tables select sheet.TableName;
            return sheets;
        }

        private static IEnumerable<DataRow> GetWorksheetData(IExcelDataReader excelReader, string sheet, bool firstRowIsColumnNames = false)
        {
            excelReader.IsFirstRowAsColumnNames = firstRowIsColumnNames;
            var workSheet = excelReader.AsDataSet().Tables[sheet];
            var rows = from DataRow a in workSheet.Rows select a;
            return rows;
        }
    }
}