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
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Vacations;
using Shrooms.Premium.Main.BusinessLayer.DomainExceptions.Vacation;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Vacations
{
    public class VacationService : IVacationService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly TelemetryClient _telemetryClient;

        private readonly IDbSet<ApplicationUser> _applicationUserDbSet;
        private readonly IVacationDomainService _vacationDomainService;

        private readonly int _codeColIndex = 0;
        private readonly int _fullnameColIndex = 1;
        private readonly int _operationColIndex = 3;
        private readonly int _officeColIndex = 4;
        private readonly int _jobTitleColIndex = 5;
        private readonly int _vacationTotalTimeColIndex = 6;
        private readonly int _vacationUsedTimeColIndex = 7;
        private readonly int _vacationUnusedTimeColIndex = 8;

        public VacationService(IUnitOfWork2 unitOfWork2, IVacationDomainService vacationDomainService)
        {
            _uow = unitOfWork2;
            _telemetryClient = new TelemetryClient();

            _applicationUserDbSet = unitOfWork2.GetDbSet<ApplicationUser>();
            _vacationDomainService = vacationDomainService;
        }

        public VacationImportStatusDTO UploadVacationReportFile(Stream fileStream)
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
                var acceptableData = row[_codeColIndex] is string && row[_fullnameColIndex] is string
                                          && row[_operationColIndex] is string && row[_officeColIndex] is string
                                          && row[_jobTitleColIndex] is string
                                          && (row[_vacationTotalTimeColIndex] is double || row[_vacationTotalTimeColIndex] is int)
                                          && (row[_vacationUsedTimeColIndex] is double || row[_vacationUsedTimeColIndex] is int)
                                          && (row[_vacationUnusedTimeColIndex] is double || row[_vacationUnusedTimeColIndex] is int);

                if (!acceptableData)
                {
                    continue;
                }

                var fullName = row[_fullnameColIndex].ToString();
                var code = row[_codeColIndex].ToString();
                var users = _applicationUserDbSet.Where(_vacationDomainService.UsersByNamesFilter(fullName).Compile()).ToList();
                var userToUpdate = _vacationDomainService.FindUser(users, fullName);

                if (userToUpdate != null)
                {
                    var fullTime = (double)row[_vacationTotalTimeColIndex];
                    var usedTime = (double)row[_vacationUsedTimeColIndex];
                    var unusedTime = (double)row[_vacationUnusedTimeColIndex];

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

            _uow.SaveChanges();
            excelReader.Close();

            return importStatus;
        }

        public async Task<VacationAvailableDaysDTO> GetAvailableDays(UserAndOrganizationDTO userOrgDto)
        {
            var user = await _applicationUserDbSet
                .FirstAsync(u => u.Id == userOrgDto.UserId);

            var availableDaysModel = new VacationAvailableDaysDTO()
            {
                AvailableDays = Math.Truncate(user.VacationUnusedTime ?? 0),
                LastTimeUpdated = user.VacationLastTimeUpdated,
            };

            return availableDaysModel;
        }

        private IEnumerable<string> GetWorksheetNames(IExcelDataReader excelReader)
        {
            var workbook = excelReader.AsDataSet();
            var sheets = from DataTable sheet in workbook.Tables select sheet.TableName;
            return sheets;
        }

        private IEnumerable<DataRow> GetWorksheetData(IExcelDataReader excelReader, string sheet, bool firstRowIsColumnNames = false)
        {
            excelReader.IsFirstRowAsColumnNames = firstRowIsColumnNames;
            var workSheet = excelReader.AsDataSet().Tables[sheet];
            var rows = from DataRow a in workSheet.Rows select a;
            return rows;
        }
    }
}