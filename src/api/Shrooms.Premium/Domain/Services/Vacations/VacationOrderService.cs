using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Vacations;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;
using Resx = Shrooms.Resources.Models.Vacations.Vacations;

namespace Shrooms.Premium.Domain.Services.Vacations
{
    public class VacationOrderService : IVacationOrderService
    {
        private const string MonthFormat = "yyyy-MM";

        private const int MaxArchiveOrders = 200;

        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<VacationOrder> _orderDbSet;
        private readonly DbSet<VacationOrderItem> _itemDbSet;
        private readonly DbSet<VacationRequest> _requestDbSet;
        private readonly DbSet<Organization> _organizationDbSet;

        public VacationOrderService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _orderDbSet = uow.GetDbSet<VacationOrder>();
            _itemDbSet = uow.GetDbSet<VacationOrderItem>();
            _requestDbSet = uow.GetDbSet<VacationRequest>();
            _organizationDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<IList<VacationOrderDto>> GetOrdersAsync(string from, string to, UserAndOrganizationDto userOrg)
        {
            var orders = await PeriodQuery(from, to, userOrg.OrganizationId)
                .AsNoTracking()
                // The issuing administrator may since have left, soft-deleting
                // their orders out of a required join.
                .IgnoreQueryFilters()
                .Include(order => order.IssuedBy)
                .Include(order => order.Items)
                .OrderByDescending(order => order.IssuedOn)
                .ThenByDescending(order => order.Number)
                .ToListAsync();

            return orders.Select(VacationMapper.ToOrder).ToList();
        }

        public async Task<VacationOrderGenerationDto> GenerateAsync(string from, string to, UserAndOrganizationDto userOrg)
        {
            var (start, end) = ParseRange(from, to);

            var organization = await _organizationDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == userOrg.OrganizationId);

            var settings = VacationSettingsService.Resolve(organization);

            var requests = await _requestDbSet
                .AsNoTracking()
                // Leave granted before somebody left still needs its order.
                .IgnoreQueryFilters()
                .Include(request => request.Employee)
                .Where(request => request.OrganizationId == userOrg.OrganizationId
                                  && request.Status == VacationRequestStatus.Approved
                                  && request.DateFrom >= start
                                  && request.DateFrom <= end)
                .ToListAsync();

            // Tracked, not AsNoTracking: an existing order is updated in place so
            // that its number survives.
            var existing = await _orderDbSet
                .Include(order => order.Items)
                .Where(order => order.OrganizationId == userOrg.OrganizationId
                                && order.Type != null
                                && order.PeriodStart >= start
                                && order.PeriodStart <= end)
                .ToListAsync();

            var number = await VacationSettingsService.NextOrderNumberAsync(_orderDbSet, userOrg.OrganizationId, settings);
            var now = DateTime.UtcNow;

            var report = new VacationOrderGenerationDto
            {
                From = VacationWireFormat.ToDay(start),
                To = VacationWireFormat.ToDay(end),
                Orders = new List<VacationOrderDto>()
            };

            var touched = new List<VacationOrder>();

            var groups = requests
                .GroupBy(request => new { Day = request.DateFrom.Date, request.Type })
                .OrderBy(group => group.Key.Day)
                .ThenBy(group => group.Key.Type);

            foreach (var group in groups)
            {
                var lines = group
                    .OrderBy(request => request.Id)
                    .Select(request => NewItem(request, now))
                    .ToList();

                var order = existing.FirstOrDefault(candidate => candidate.Type == group.Key.Type
                                                                 && candidate.PeriodStart == group.Key.Day);

                if (order == null)
                {
                    order = new VacationOrder
                    {
                        OrganizationId = userOrg.OrganizationId,
                        Number = number++,
                        Prefix = settings.OrderPrefix,
                        Type = group.Key.Type,
                        PeriodStart = group.Key.Day,
                        // The document has to be signed before the leave starts.
                        IssuedOn = VacationCalculator.PreviousWorkingDay(group.Key.Day),
                        IssuedById = userOrg.UserId,
                        Created = now,
                        Modified = now,
                        Items = lines
                    };

                    _orderDbSet.Add(order);
                    report.Created++;
                }
                else if (SameLines(order.Items, lines))
                {
                    report.Unchanged++;
                }
                else
                {
                    // The number and the date stay. EF does not delete orphans,
                    // so the old lines go explicitly.
                    foreach (var stale in order.Items.ToList())
                    {
                        _itemDbSet.Remove(stale);
                    }

                    foreach (var line in lines)
                    {
                        order.Items.Add(line);
                    }

                    order.Modified = now;
                    report.Updated++;
                }

                touched.Add(order);
            }

            // An order whose leave has since been cancelled keeps its lines — it
            // is a numbered document, not a view — but it is counted, so nobody
            // reads "unchanged" as "still current".
            report.Stale = existing.Count(order => !touched.Contains(order));

            try
            {
                await _uow.SaveChangesAsync(userOrg.UserId);
            }
            catch (DbUpdateException e) when (IsDuplicateNumber(e))
            {
                // Two administrators generating at once allocate the same number
                // and the unique index stops the second, which is the point. Only
                // that becomes a refusal; any other write failure keeps bubbling.
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationOrderRaceLost,
                    "orderRaceLost",
                    Resx.GetResourceString("orderRaceLost"));
            }

            var ids = touched.Select(order => order.Id).ToList();
            var saved = await _orderDbSet
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(order => order.IssuedBy)
                .Include(order => order.Items)
                .Where(order => ids.Contains(order.Id))
                .OrderBy(order => order.Number)
                .ToListAsync();

            report.Orders = saved.Select(VacationMapper.ToOrder).ToList();
            return report;
        }

        public async Task<VacationDocumentDto> GetArchiveAsync(string from, string to, UserAndOrganizationDto userOrg)
        {
            var (start, end) = ParseRange(from, to);

            var orders = await PeriodQuery(from, to, userOrg.OrganizationId)
                .AsNoTracking()
                .Include(order => order.Items)
                .OrderBy(order => order.Number)
                .ToListAsync();

            if (orders.Count == 0)
            {
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationOrderEmpty,
                    "orderEmpty",
                    Resx.GetResourceString("orderHasNoRequests"));
            }

            // Every document is built in memory, so a year-wide request is
            // refused rather than attempted.
            if (orders.Count > MaxArchiveOrders)
            {
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationArchiveTooLarge,
                    "archiveTooLarge",
                    Resx.GetResourceString("archiveTooLarge", MaxArchiveOrders),
                    new Dictionary<string, object> { ["max"] = MaxArchiveOrders });
            }

            var organization = await _organizationDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == userOrg.OrganizationId);

            var settings = VacationSettingsService.Resolve(organization);

            using var stream = new MemoryStream();

            // leaveOpen, or disposing the archive closes the stream before the
            // bytes can be read out of it.
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true, Encoding.UTF8))
            {
                foreach (var order in orders)
                {
                    var entry = archive.CreateEntry(DocumentName(order), CompressionLevel.Optimal);
                    await using var entryStream = entry.Open();
                    var content = VacationOrderDocumentBuilder.Build(order, settings);
                    await entryStream.WriteAsync(content);
                }
            }

            return new VacationDocumentDto
            {
                FileName = ArchiveName(start, end),
                ContentType = "application/zip",
                Content = stream.ToArray()
            };
        }

        public async Task<VacationDocumentDto> GetOrderDocumentAsync(int id, UserAndOrganizationDto userOrg)
        {
            var order = await _orderDbSet
                .AsNoTracking()
                .Include(entity => entity.Items)
                .FirstOrDefaultAsync(entity => entity.Id == id && entity.OrganizationId == userOrg.OrganizationId);

            if (order == null)
            {
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationOrderNotFound,
                    "orderNotFound",
                    Resx.GetResourceString("orderNotFound"));
            }

            var organization = await _organizationDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == userOrg.OrganizationId);

            var settings = VacationSettingsService.Resolve(organization);

            return new VacationDocumentDto
            {
                FileName = DocumentName(order),
                ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                Content = VacationOrderDocumentBuilder.Build(order, settings)
            };
        }

        private IQueryable<VacationOrder> PeriodQuery(string from, string to, int organizationId)
        {
            var query = _orderDbSet.Where(order => order.OrganizationId == organizationId);

            if (string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to))
            {
                return query;
            }

            var (start, end) = ParseRange(from, to);

            // On the leave it grants, not on its own date: an order signed on 31
            // July for leave starting 3 August belongs to August. Hand-assembled
            // orders have no period, so they fall back to their own date.
            return query.Where(order => (order.PeriodStart ?? order.IssuedOn) >= start
                                        && (order.PeriodStart ?? order.IssuedOn) <= end);
        }

        private static (DateTime Start, DateTime End) ParseRange(string from, string to)
        {
            var start = VacationWireFormat.ParseDay(from);
            var end = VacationWireFormat.ParseDay(to);

            if (start == null || end == null || end < start)
            {
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationDatesRequired,
                    "datesRequired",
                    Resx.GetResourceString("notEnoughData"));
            }

            return (start.Value, end.Value);
        }

        /// <summary>
        /// A whole calendar month keeps the month in the name, which is what
        /// payroll files these by; any other period spells both ends out.
        /// </summary>
        private static string ArchiveName(DateTime start, DateTime end)
        {
            var wholeMonth = start.Day == 1 && end == start.AddMonths(1).AddDays(-1);

            return wholeMonth
                ? $"Isakymai_{start.ToString(MonthFormat, CultureInfo.InvariantCulture)}.zip"
                : $"Isakymai_{VacationWireFormat.ToDay(start)}_{VacationWireFormat.ToDay(end)}.zip";
        }

        private static VacationOrderItem NewItem(VacationRequest request, DateTime now)
        {
            return new VacationOrderItem
            {
                VacationRequestId = request.Id,
                // Snapshotted: a later correction to the request must not
                // silently change what the signed page says.
                EmployeeName = $"{request.Employee?.FirstName} {request.Employee?.LastName}".Trim(),
                Type = request.Type,
                DateFrom = request.DateFrom,
                DateTo = request.DateTo,
                Created = now,
                Modified = now
            };
        }

        /// <summary>By content, so a regeneration that changes nothing reports none.</summary>
        private static bool SameLines(ICollection<VacationOrderItem> current, IList<VacationOrderItem> fresh)
        {
            if (current.Count != fresh.Count)
            {
                return false;
            }

            static string Key(VacationOrderItem item) =>
                $"{item.VacationRequestId}|{item.EmployeeName}|{item.Type}|{item.DateFrom:O}|{item.DateTo:O}";

            return current.Select(Key).OrderBy(key => key)
                .SequenceEqual(fresh.Select(Key).OrderBy(key => key));
        }

        private static bool IsDuplicateNumber(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sql
                   && (sql.Number == 2601 || sql.Number == 2627);
        }

        /// <summary>An administrator sets the prefix, so path separators come out here too.</summary>
        private static string DocumentName(VacationOrder order)
        {
            var reference = new string(order.Reference
                .Where(character => character != '/' && character != '\\' && character != ':')
                .ToArray())
                .Replace("..", string.Empty);

            return $"Isakymas_{reference}.docx";
        }
    }
}
