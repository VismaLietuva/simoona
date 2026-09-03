using Microsoft.EntityFrameworkCore;
﻿using System;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
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
        private readonly DbSet<KudosLog> _kudosLogsDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly IExcelBuilderFactory _excelBuilderFactory;
        private readonly ISystemClock _systemClock;

        public KudosExportService(IUnitOfWork2 uow, IExcelBuilderFactory excelBuilderFactory, ISystemClock systemClock)
        {
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _excelBuilderFactory = excelBuilderFactory;
            _systemClock = systemClock;
        }

        public async Task<FileExportDto> ExportToExcelAsync(KudosLogsFilterDto filter)
        {
            var logsQuery = _kudosLogsDbSet
                .Include(log => log.Employee)
                .Where(log =>
                    log.OrganizationId == filter.OrganizationId &&
                    log.KudosBasketId == null)
                .Where(KudosServiceHelper.StatusFilter(filter.Status))
                .Where(KudosServiceHelper.UserFilter(filter.SearchUserId))
                .Where(KudosServiceHelper.TypeFilter(filter.FilteringType))
                .SelectMany(log => _userDbSet.Where(user => user.Id == log.CreatedBy).DefaultIfEmpty(), KudosServiceHelper.MapKudosLogsToDto());

            var sortedLogs = System.Linq.Dynamic.Core.DynamicQueryableExtensions.OrderBy(
                logsQuery.AsQueryable(),
                string.Concat(filter.SortBy, " ", filter.SortOrder, ", Id ", filter.SortOrder));

            var kudos = await sortedLogs.ToListAsync();

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

            var fileName = FileExportName.Sanitize(
                $"kudos {_systemClock.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
                "kudos",
                ".xlsx");
            return new FileExportDto(excelBuilder.Build(), fileName);
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
                }
            };
        }
    }
}
