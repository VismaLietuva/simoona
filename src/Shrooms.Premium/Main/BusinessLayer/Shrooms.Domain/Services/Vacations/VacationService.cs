using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using Excel;
using Shrooms.DataLayer;
using Shrooms.DataLayer.DAL;
using Shrooms.DataTransferObjects.Models;
using Shrooms.EntityModels.Models;
using Shrooms.DataTransferObjects.Models.Vacations;
using System.Threading.Tasks;

namespace Shrooms.Domain.Services.Vacations
{
    public class VacationService : IVacationService
    {
        private readonly IUnitOfWork2 _uow;

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

            _applicationUserDbSet = unitOfWork2.GetDbSet<ApplicationUser>();

            _vacationDomainService = vacationDomainService;
        }

        public void UploadVacationReportFile(Stream fileStream)
        {
            IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(fileStream);

            var sheets = GetWorksheetNames(excelReader);
            var workSheet = GetWorksheetData(excelReader, sheets.First());

            foreach (var row in workSheet)
            {
                var acceptableData = row[_codeColIndex].GetType() == typeof(string) &&
                    row[_fullnameColIndex].GetType() == typeof(string) &&
                    row[_operationColIndex].GetType() == typeof(string) &&
                    row[_officeColIndex].GetType() == typeof(string) &&
                    row[_jobTitleColIndex].GetType() == typeof(string) &&
                    (row[_vacationTotalTimeColIndex].GetType() == typeof(double) || row[_vacationTotalTimeColIndex].GetType() == typeof(int)) &&
                    (row[_vacationUsedTimeColIndex].GetType() == typeof(double) || row[_vacationUsedTimeColIndex].GetType() == typeof(int)) &&
                    (row[_vacationUnusedTimeColIndex].GetType() == typeof(double) || row[_vacationUnusedTimeColIndex].GetType() == typeof(int));

                if (acceptableData)
                {
                    var fullName = row[_fullnameColIndex].ToString();

                    var users = _applicationUserDbSet.Where(_vacationDomainService.UsersByNamesFilter(fullName).Compile()).ToList();

                    ApplicationUser userToUpdate = _vacationDomainService.FindUser(users, fullName);

                    if (userToUpdate != null)
                    {
                        var fullTime = (double)row[_vacationTotalTimeColIndex];
                        var usedTime = (double)row[_vacationUsedTimeColIndex];
                        var unusedTime = (double)row[_vacationUnusedTimeColIndex];

                        userToUpdate.VacationTotalTime = fullTime;
                        userToUpdate.VacationUsedTime = usedTime;
                        userToUpdate.VacationUnusedTime = unusedTime;
                        userToUpdate.VacationLastTimeUpdated = DateTime.UtcNow;
                    }
                }
            }

            _uow.SaveChanges();
            excelReader.Close();
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