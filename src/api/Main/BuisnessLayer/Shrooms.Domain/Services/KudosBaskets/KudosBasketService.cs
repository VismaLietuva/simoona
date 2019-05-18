using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Shrooms.Constants.BusinessLayer;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.KudosBasket;
using Shrooms.Domain.Services.Kudos;
using Shrooms.DomainServiceValidators.Validators.KudosBaskets;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Kudos;
using Shrooms.Host.Contracts.DAL;

namespace Shrooms.Domain.Services.KudosBaskets
{
    public class KudosBasketService : IKudosBasketService
    {
        private const int KudosMultiplier = 1;

        private static object _donateLock = new object();

        private readonly IDbSet<KudosLog> _kudosLogsDbSet;
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IDbSet<KudosBasket> _kudosBasketsDbSet;
        private readonly IDbSet<KudosType> _kudosTypesDbSet;
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

        public IList<KudosBasketLogDTO> GetDonations(UserAndOrganizationDTO userAndOrg)
        {
            var kudosBasket = _kudosBasketsDbSet
                .Include(basket => basket.KudosLogs.Select(log => log.Employee))
                .FirstOrDefault(basket => basket.OrganizationId == userAndOrg.OrganizationId);

            var kudosLogs = kudosBasket.KudosLogs
                .OrderByDescending(log => log.Created);

            var kudosLogDtos = MapKudosLogsToDto(kudosLogs);
            return kudosLogDtos;
        }

        public KudosBasketCreateDTO CreateNewBasket(KudosBasketCreateDTO newBasket)
        {
            var existsBasket = _kudosBasketsDbSet.Any();
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
            _uow.SaveChanges(false);

            newBasket.Id = kudosBasket.Id;
            newBasket.IsActive = kudosBasket.IsActive;

            return newBasket;
        }

        public KudosBasketDTO GetKudosBasketWidget(UserAndOrganizationDTO userAndOrganization)
        {
            return _kudosBasketsDbSet
                .Include(b => b.KudosLogs)
                .Where(b => b.OrganizationId == userAndOrganization.OrganizationId
                    && b.IsActive)
                .Select(MapKudosBasketToDto())
                .FirstOrDefault();
        }

        public KudosBasketDTO GetKudosBasket(UserAndOrganizationDTO userAndOrganization)
        {
            var kudosBasket = _kudosBasketsDbSet
                .Include(b => b.KudosLogs)
                .Where(b => b.OrganizationId == userAndOrganization.OrganizationId)
                .Select(MapKudosBasketToDto())
                .FirstOrDefault();

            _kudosBasketValidator.CheckIfThereIsNoBasketYet(kudosBasket);

            return kudosBasket;
        }

        public void DeleteKudosBasket(UserAndOrganizationDTO userAndOrganization)
        {
            var foundBasket = _kudosBasketsDbSet.First(basket => basket.OrganizationId == userAndOrganization.OrganizationId);

            foundBasket.IsActive = false;
            foundBasket.Modified = DateTime.UtcNow;
            foundBasket.ModifiedBy = userAndOrganization.UserId;
            _uow.SaveChanges(false);

            _kudosBasketsDbSet.Remove(foundBasket);
            _uow.SaveChanges(false);
        }

        public void EditKudosBasket(KudosBasketEditDTO editedBasket)
        {
            var basketToEdit = _kudosBasketsDbSet
                .First(basket => basket.Id == editedBasket.Id);

            basketToEdit.Title = editedBasket.Title;
            basketToEdit.Description = editedBasket.Description;
            basketToEdit.IsActive = editedBasket.IsActive;
            basketToEdit.Modified = DateTime.UtcNow;
            basketToEdit.ModifiedBy = editedBasket.UserId;

            _uow.SaveChanges();
        }

        public void MakeDonation(KudosBasketDonationDTO donation)
        {
            lock (_donateLock)
            {
                var user = _usersDbSet
                    .First(usr => usr.Id == donation.UserId);
                var basket = _kudosBasketsDbSet
                    .Include(b => b.KudosLogs)
                    .First(b => b.OrganizationId == donation.OrganizationId);

                _kudosBasketValidator.CheckIfBasketIsActive(basket);
                _kudosBasketValidator.CheckIfUserHasEnoughKudos(user.RemainingKudos, donation.DonationAmount);

                var otherType = _kudosTypesDbSet
                    .First(type => type.Type == BusinessLayerConstants.KudosTypeEnum.Other);
                var minusType = _kudosTypesDbSet
                    .First(type => type.Type == BusinessLayerConstants.KudosTypeEnum.Minus);

                var logComment = string.Format(Resources.Widgets.KudosBasket.KudosBasket.KudosBasketDonationComment, basket.Title);
                var noUser = default(string);

                var plusLog = CreateKudosLogForBasket(donation, otherType, logComment, user.Id);
                var minusLog = CreateKudosLogForBasket(donation, minusType, logComment, noUser);

                _kudosLogsDbSet.Add(minusLog);
                basket.KudosLogs.Add(plusLog);

                _uow.SaveChanges(false);

                _kudosService.UpdateProfileKudos(user, donation);
            }
        }

        private KudosLog CreateKudosLogForBasket(KudosBasketDonationDTO donation, KudosType kudosType, string logComment, string userId)
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

        private List<KudosBasketLogDTO> MapKudosLogsToDto(IOrderedEnumerable<KudosLog> kudosLogs)
        {
            var kudosLogsDto = kudosLogs.Select(log => new KudosBasketLogDTO
            {
                DonationAmount = log.Points,
                DonationDate = log.Created,
                Donator = MapUserToDto(log.Employee)
            })
            .ToList();
            return kudosLogsDto;
        }

        private Expression<Func<KudosBasket, KudosBasketDTO>> MapKudosBasketToDto()
        {
            return b => new KudosBasketDTO
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

        private KudosBasketLogUserDTO MapUserToDto(ApplicationUser employee)
        {
            KudosBasketLogUserDTO userDto;
            if (employee == null)
            {
                userDto = new KudosBasketLogUserDTO
                {
                    FullName = BusinessLayerConstants.DeletedUserName
                };
            }
            else
            {
                userDto = new KudosBasketLogUserDTO
                {
                    Id = employee.Id,
                    FullName = $"{employee.FirstName} {employee.LastName}"
                };
            }

            return userDto;
        }
    }
}