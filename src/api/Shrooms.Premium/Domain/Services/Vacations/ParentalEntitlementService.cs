using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class ParentalEntitlementService : IParentalEntitlementService
    {
        private const string NoneWire = "none";

        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<ParentalEntitlement> _entitlementDbSet;
        private readonly DbSet<VacationRequest> _requestDbSet;
        private readonly DbSet<ApplicationUser> _userDbSet;
        private readonly DbSet<Organization> _organizationDbSet;

        public ParentalEntitlementService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _entitlementDbSet = uow.GetDbSet<ParentalEntitlement>();
            _requestDbSet = uow.GetDbSet<VacationRequest>();
            _userDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationDbSet = uow.GetDbSet<Organization>();
        }

        public async Task<ParentalBalanceDto> GetBalanceAsync(UserAndOrganizationDto userOrg)
        {
            var entitlement = await _entitlementDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.OrganizationId == userOrg.OrganizationId && e.EmployeeId == userOrg.UserId);

            if (entitlement == null)
            {
                return new ParentalBalanceDto();
            }

            var today = await _organizationDbSet.TodayAsync(userOrg.OrganizationId);
            var rule = ParentalEntitlementRules.RuleFor(entitlement.Type);
            var (from, to) = ParentalEntitlementRules.PeriodContaining(rule.Period, today);

            var balance = new ParentalBalanceDto
            {
                Type = ParentalEntitlementRules.TypeToWire(entitlement.Type),
                Unit = ParentalEntitlementRules.UnitToWire(rule.Unit),
                Amount = rule.Amount,
                Period = ParentalEntitlementRules.PeriodToWire(rule.Period),
                PeriodFrom = VacationWireFormat.ToDay(from),
                PeriodTo = VacationWireFormat.ToDay(to)
            };

            if (rule.Unit == ParentalUnit.Hours)
            {
                return balance;
            }

            var requests = await _requestDbSet
                .AsNoTracking()
                .Where(request => request.OrganizationId == userOrg.OrganizationId
                                  && request.EmployeeId == userOrg.UserId
                                  && request.Type == VacationRequestType.Parental
                                  && request.DateFrom >= from
                                  && request.DateFrom <= to)
                .ToListAsync();

            var booked = ParentalEntitlementRules.BookedDays(requests, rule, today);
            balance.Booked = booked;
            balance.Left = Math.Max(0, rule.Amount - booked);

            return balance;
        }

        public async Task<IList<ParentalEntitlementDto>> GetAllAsync(ParentalEntitlementListArgsDto args)
        {
            var userQuery = _userDbSet
                .AsNoTracking()
                .Where(user => user.OrganizationId == args.OrganizationId);

            if (!string.IsNullOrWhiteSpace(args.Search))
            {
                var term = args.Search.Trim();
                userQuery = userQuery.Where(user => user.FirstName.Contains(term) || user.LastName.Contains(term));
            }

            var users = await userQuery.ToListAsync();

            var entitlements = await _entitlementDbSet
                .AsNoTracking()
                .Where(e => e.OrganizationId == args.OrganizationId)
                .ToDictionaryAsync(e => e.EmployeeId);

            var rows = users.Select(user =>
            {
                entitlements.TryGetValue(user.Id, out var entitlement);
                return ToDto(user, entitlement);
            });

            var descending = string.Equals(args.Dir, "desc", StringComparison.OrdinalIgnoreCase);

            return (descending
                    ? rows.OrderByDescending(row => row.Employee.FullName)
                    : rows.OrderBy(row => row.Employee.FullName))
                .ToList();
        }

        public async Task<ParentalEntitlementDto> SetAsync(string employeeId, string type, UserAndOrganizationDto userOrg)
        {
            var cleared = string.IsNullOrWhiteSpace(type) || type.Trim() == NoneWire;
            var parsed = ParentalEntitlementRules.ParseType(type);

            if (!cleared && parsed == null)
            {
                throw VacationRequestValidator.Fail(
                    ErrorCodes.VacationParentalTypeInvalid,
                    "parentalTypeInvalid",
                    Resx.GetResourceString("parentalTypeInvalid"));
            }

            var employee = await _userDbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.Id == employeeId && user.OrganizationId == userOrg.OrganizationId);

            if (employee == null)
            {
                throw VacationRequestValidator.NotFound();
            }

            var entitlement = await _entitlementDbSet
                .FirstOrDefaultAsync(e => e.OrganizationId == userOrg.OrganizationId && e.EmployeeId == employeeId);

            if (parsed == null)
            {
                if (entitlement != null)
                {
                    _entitlementDbSet.Remove(entitlement);
                    await _uow.SaveChangesAsync(userOrg.UserId);
                }

                return ToDto(employee, null);
            }

            if (entitlement == null)
            {
                entitlement = new ParentalEntitlement
                {
                    OrganizationId = userOrg.OrganizationId,
                    EmployeeId = employeeId,
                    Type = parsed.Value
                };
                _entitlementDbSet.Add(entitlement);
            }
            else if (entitlement.Type != parsed.Value)
            {
                entitlement.Type = parsed.Value;
            }
            else
            {
                return ToDto(employee, entitlement);
            }

            await _uow.SaveChangesAsync(userOrg.UserId);

            return ToDto(employee, entitlement);
        }

        private static ParentalEntitlementDto ToDto(ApplicationUser user, ParentalEntitlement entitlement)
        {
            return new ParentalEntitlementDto
            {
                Employee = VacationMapper.ToPerson(user),
                Type = entitlement == null ? null : ParentalEntitlementRules.TypeToWire(entitlement.Type),
                UpdatedAt = entitlement == null
                    ? null
                    : DateTime.SpecifyKind(entitlement.Modified, DateTimeKind.Utc)
            };
        }
    }
}
