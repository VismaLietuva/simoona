using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects.Models.Kudos;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.ExcelGenerator;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Helpers;
using Shrooms.Infrastructure.ExcelGenerator;

namespace Shrooms.Domain.Services.Kudos
{
    public class KudosExportService : IKudosExportService
    {
        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        private readonly IDbSet<ApplicationUser> _userDbSet;
        private readonly IExcelBuilderFactory _excelBuilderFactory;

        public KudosExportService(IUnitOfWork2 uow, IExcelBuilderFactory excelBuilderFactory)
        {
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _excelBuilderFactory = excelBuilderFactory;
        }

        public async Task<ByteArrayContent> ExportToExcelAsync(KudosLogsFilterDto filter)
        {
            var kudos = await _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log =>
                    log.OrganizationId == filter.OrganizationId &&
                    log.KudosBasketId == null)
                .Where(KudosServiceHelper.StatusFilter(filter.Status))
                .Where(KudosServiceHelper.UserFilter(filter.SearchUserId))
                .GroupJoin(_userDbSet, log => log.CreatedBy, u => u.Id, KudosServiceHelper.MapKudosLogsToDto())
                .OrderBy(string.Concat(filter.SortBy, " ", filter.SortOrder))
                .ToListAsync();

            var excelBuilder = _excelBuilderFactory.GetBuilder();

            excelBuilder
                .AddWorksheet(BusinessLayerConstants.KudosLogExcelSheetName)
                .AddHeader(
                    Resources.Models.Kudos.Kudos.ExportColumnSender,
                    Resources.Models.Kudos.Kudos.ExportColumnReceiver,
                    Resources.Models.Kudos.Kudos.ExportColumnKudosType,
                    Resources.Models.Kudos.Kudos.ExportColumnMultiplyBy,
                    Resources.Models.Kudos.Kudos.ExportColumnPointsInTotal,
                    Resources.Models.Kudos.Kudos.ExportColumnCreated,
                    Resources.Models.Kudos.Kudos.ExportColumnComment,
                    Resources.Models.Kudos.Kudos.ExportColumnStatus,
                    Resources.Models.Kudos.Kudos.ExportColumnKudosTypeValue,
                    Resources.Models.Kudos.Kudos.ExportColumnRejectionMessage)
                .AddRows(kudos.AsQueryable(), MapKudosLogToExcelCell());

            return new ByteArrayContent(excelBuilder.Build());
        }

        private static Expression<Func<MainKudosLogDto, IExcelRow>> MapKudosLogToExcelCell()
        {
            return log => new ExcelRow
            {
                new ExcelColumn 
                { 
                    Value = log.Sender.FullName 
                },

                new ExcelColumn 
                { 
                    Value = log.Receiver.FullName 
                },
                
                new ExcelColumn 
                { 
                    Value = log.Type.Name 
                },
                
                new ExcelColumn 
                {
                    Value = log.Multiplier, Format = ExcelWorksheetBuilderConstants.DecimalFormat
                },

                new ExcelColumn 
                {
                    Value = log.Points, Format = ExcelWorksheetBuilderConstants.DecimalFormat 
                },
                
                new ExcelColumn 
                {
                    Value = log.Created, Format = ExcelWorksheetBuilderConstants.DateFormat 
                },
                
                new ExcelColumn 
                { 
                    Value = log.Comment 
                },
                
                new ExcelColumn 
                {
                    Value = log.Status 
                },
                
                new ExcelColumn 
                { 
                    Value = log.Type.Value 
                },
                
                new ExcelColumn 
                {
                    Value = log.RejectionMessage 
                },
            };
        }
    }
}
