using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.Domain.Services.Kudos;
using Shrooms.Domain.ServiceValidators.Validators.KudosBaskets;

namespace Shrooms.Domain.Services.KudosBaskets
{
    public class KudosBasketService : IKudosBasketService
    {
        private const int KudosMultiplier = 1;

        private static readonly SemaphoreSlim _donateLock = new SemaphoreSlim(1, 1);

        private readonly DbSet<KudosLog> _kudosLogsDbSet;
        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<KudosBasket> _kudosBasketsDbSet;
        private readonly DbSet<KudosType> _kudosTypesDbSet;
        private readonly IKudosBasketValidator _kudosBasketValidator;
        private readonly IUnitOfWork2 _uow;
        private readonly IKudosService _kudosService;

        public KudosBasketService(IUnitOfWork2 uow, IKudosBasketValidator kudosBasketValidator, IKudosService kudosService)
        {
            _uow = uow;
            _kudosBasketsDbSet = uow.GetDbSet<KudosBasket>();
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _kudosLogsDbSet = uow.GetDbSet<KudosLog>();
            _kudosTypesDbSet = uow.GetDbSet<KudosType>();
            _kudosBasketValidator = kudosBasketValidator;
            _kudosService = kudosService;
        }

        public async Task<IList<KudosBasketLogDto>> GetDonationsAsync(UserAndOrganizationDto userAndOrg)
        {
            var kudosBasket = await _kudosBasketsDbSet
                .Include(basket => basket.KudosLogs.Select(log => log.Employee))
                .FirstOrDefaultAsync(basket => basket.OrganizationId == userAndOrg.OrganizationId);

            var kudosLogs = kudosBasket?.KudosLogs.OrderByDescending(log => log.Created);

            var kudosLogDtos = MapKudosLogsToDto(kudosLogs);
            return kudosLogDtos;
        }

        public async Task<KudosBasketCreateDto> CreateNewBasketAsync(KudosBasketCreateDto newBasket)
        {
            var existsBasket = await _kudosBasketsDbSet.AnyAsync();
            _kudosBasketValidator.CheckIfBasketAlreadyExists(existsBasket);

            var kudosBasket = new KudosBasket
            {
                IsActive = true,
                Created = DateTime.UtcNow,
                CreatedBy = newBasket.UserId,
                Modified = DateTime.UtcNow,
                Title = newBasket.Title,
                Description = newBasket.Description,
                OrganizationId = newBasket.OrganizationId,
                KudosLogs = new List<KudosLog>()
            };

            _kudosBasketsDbSet.Add(kudosBasket);
            await _uow.SaveChangesAsync(false);

            newBasket.Id = kudosBasket.Id;
            newBasket.IsActive = kudosBasket.IsActive;

            return newBasket;
        }

        public async Task<KudosBasketDto> GetKudosBasketWidgetAsync(UserAndOrganizationDto userAndOrganization)
        {
            return await _kudosBasketsDbSet
                .Include(b => b.KudosLogs)
                .Where(b => b.OrganizationId == userAndOrganization.OrganizationId && b.IsActive)
                .Select(MapKudosBasketToDto())
                .FirstOrDefaultAsync();
        }

        public async Task<KudosBasketDto> GetKudosBasketAsync(UserAndOrganizationDto userAndOrganization)
        {
            var kudosBasket = await _kudosBasketsDbSet
                .Include(b => b.KudosLogs)
                .Where(b => b.OrganizationId == userAndOrganization.OrganizationId)
                .Select(MapKudosBasketToDto())
                .FirstOrDefaultAsync();

            _kudosBasketValidator.CheckIfThereIsNoBasketYet(kudosBasket);

            return kudosBasket;
        }

        public async Task DeleteKudosBasketAsync(UserAndOrganizationDto userAndOrganization)
        {
            var foundBasket = await _kudosBasketsDbSet.FirstAsync(basket => basket.OrganizationId == userAndOrganization.OrganizationId);

            foundBasket.IsActive = false;
            foundBasket.Modified = DateTime.UtcNow;
            foundBasket.ModifiedBy = userAndOrganization.UserId;
            await _uow.SaveChangesAsync(false);

            _kudosBasketsDbSet.Remove(foundBasket);
            await _uow.SaveChangesAsync(false);
        }

        public async Task EditKudosBasketAsync(KudosBasketEditDto editedBasket)
        {
            var basketToEdit = await _kudosBasketsDbSet.FirstAsync(basket => basket.Id == editedBasket.Id);

            basketToEdit.Title = editedBasket.Title;
            basketToEdit.Description = editedBasket.Description;
            basketToEdit.IsActive = editedBasket.IsActive;
            basketToEdit.Modified = DateTime.UtcNow;
            basketToEdit.ModifiedBy = editedBasket.UserId;

            await _uow.SaveChangesAsync();
        }

        public async Task MakeDonationAsync(KudosBasketDonationDto donation)
        {
            await _donateLock.WaitAsync();

            try
            {
                var user = await _usersDbSet.FirstAsync(usr => usr.Id == donation.UserId);

                var basket = await _kudosBasketsDbSet
                    .Include(b => b.KudosLogs)
                    .FirstAsync(b => b.OrganizationId == donation.OrganizationId);

                _kudosBasketValidator.CheckIfBasketIsActive(basket);
                _kudosBasketValidator.CheckIfUserHasEnoughKudos(user.RemainingKudos, donation.DonationAmount);

                var otherType = await _kudosTypesDbSet.FirstAsync(type => type.Type == KudosTypeEnum.Other);

                var minusType = await _kudosTypesDbSet.FirstAsync(type => type.Type == KudosTypeEnum.Minus);

                var logComment = string.Format(Resources.Widgets.KudosBasket.KudosBasket.KudosBasketDonationComment, basket.Title);

                var plusLog = CreateKudosLogForBasket(donation, otherType, logComment, user.Id);
                var minusLog = CreateKudosLogForBasket(donation, minusType, logComment, null);

                _kudosLogsDbSet.Add(minusLog);
                basket.KudosLogs.Add(plusLog);

                await _uow.SaveChangesAsync(false);

                await _kudosService.UpdateProfileKudosAsync(user, donation);
            }
            finally
            {
                _donateLock.Release();
            }
        }

        private static KudosLog CreateKudosLogForBasket(KudosBasketDonationDto donation, KudosType kudosType, string logComment, string userId)
        {
            var timestamp = DateTime.UtcNow;
            var kudosLog = new KudosLog
            {
                Created = timestamp,
                Modified = timestamp,
                CreatedBy = donation.UserId,
                Points = donation.DonationAmount,
                EmployeeId = userId,
                MultiplyBy = KudosMultiplier,
                Status = KudosStatus.Approved,
                Comments = logComment,
                OrganizationId = donation.OrganizationId,
                KudosTypeName = kudosType.Name,
                KudosTypeValue = kudosType.Value,
                KudosSystemType = kudosType.Type
            };
            return kudosLog;
        }

        private static List<KudosBasketLogDto> MapKudosLogsToDto(IOrderedEnumerable<KudosLog> kudosLogs)
        {
            var kudosLogsDto = kudosLogs?.Select(log => new KudosBasketLogDto
            {
                DonationAmount = log.Points,
                DonationDate = log.Created,
                Donator = MapUserToDto(log.Employee)
            })
            .ToList();

            return kudosLogsDto;
        }

        private static Expression<Func<KudosBasket, KudosBasketDto>> MapKudosBasketToDto()
        {
            return b => new KudosBasketDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                KudosDonated = b.KudosLogs
                    .Select(log => log.Points)
                    .DefaultIfEmpty(0)
                    .Sum(),
                IsActive = b.IsActive
            };
        }

        private static KudosBasketLogUserDto MapUserToDto(ApplicationUser employee)
        {
            KudosBasketLogUserDto userDto;
            if (employee == null)
            {
                userDto = new KudosBasketLogUserDto
                {
                    FullName = BusinessLayerConstants.DeletedUserName
                };
            }
            else
            {
                userDto = new KudosBasketLogUserDto
                {
                    Id = employee.Id,
                    FullName = $"{employee.FirstName} {employee.LastName}"
                };
            }

            return userDto;
        }
    }
}