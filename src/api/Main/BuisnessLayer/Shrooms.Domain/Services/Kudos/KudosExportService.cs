using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models.Kudos;
using Shrooms.Domain.Helpers;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Infrastructure.ExcelGenerator;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.Kudos
{
    public class KudosExportService : IKudosExportService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        private readonly IDbSet<ApplicationUser> _userDbSet;
        private readonly IExcelBuilder _excelBuilder;

        public KudosExportService(
            IUnitOfWork2 uow,
            IExcelBuilder excelBuilder)
        {
            _uow = uow;
            _kudosLogsDbSet = _uow.GetDbSet<KudosLog>();
            _userDbSet = _uow.GetDbSet<ApplicationUser>();
            _excelBuilder = excelBuilder;
        }

        public byte[] ExportToExcel(KudosLogsFilterDTO filter)
        {
            var kudosLogs = _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log =>
                    log.OrganizationId == filter.OrganizationId &&
                    log.KudosBasketId == null)
                .Where(KudosServiceHelper.StatusFilter(filter.Status))
                .Where(KudosServiceHelper.UserFilter(filter.SearchUserId))
                .GroupJoin(_userDbSet, log => log.CreatedBy, u => u.Id, KudosServiceHelper.MapKudosLogsToDto())
                .OrderBy(string.Concat(filter.SortBy, " ", filter.SortOrder))
                .AsEnumerable()
                .Select(x => new List<object>
                {
                    x.Sender.FullName,
                    x.Receiver.FullName,
                    x.Type.Name,
                    x.Multiplier,
                    x.Points,
                    x.Created,
                    x.Comment,
                    x.Status,
                    x.Type.Value,
                    x.RejectionMessage
                });

            var header = new List<string>
            {
                Resources.Models.Kudos.Kudos.ExportColumnSender,
                Resources.Models.Kudos.Kudos.ExportColumnReceiver,
                Resources.Models.Kudos.Kudos.ExportColumnKudosType,
                Resources.Models.Kudos.Kudos.ExportColumnMultiplyBy,
                Resources.Models.Kudos.Kudos.ExportColumnPointsInTotal,
                Resources.Models.Kudos.Kudos.ExportColumnCreated,
                Resources.Models.Kudos.Kudos.ExportColumnComment,
                Resources.Models.Kudos.Kudos.ExportColumnStatus,
                Resources.Models.Kudos.Kudos.ExportColumnKudosTypeValue,
                Resources.Models.Kudos.Kudos.ExportColumnRejectionMessage
            };

            _excelBuilder.AddNewWorksheet(
                ConstBusinessLayer.KudosLogExcelSheetName,
                header,
                kudosLogs);

            return _excelBuilder.GenerateByteArray();
        }
    }
}
