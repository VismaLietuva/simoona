﻿using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Contracts.DAL;
using Shrooms.Premium.Infrastructure.VacationBot;
using Shrooms.Premium.Main.BusinessLayer.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.Vacations
{
    public class VacationHistoryService : IVacationHistoryService
    {
        private readonly IDbSet<ApplicationUser> _usersDbSet;
        private readonly IVacationBotService _vacationBotService;

        public VacationHistoryService(IUnitOfWork2 uow, IVacationBotService vacationBotService)
        {
            _vacationBotService = vacationBotService;
            _usersDbSet = uow.GetDbSet<ApplicationUser>();
        }

        public async Task<VacationDTO[]> GetVacationHistory(string userId)
        {
            var user = await _usersDbSet.SingleAsync(u => u.Id == userId);

            var vacationsInfo = await _vacationBotService.GetVacationHistory(user.Email);
            return vacationsInfo.Select(MapVacationInfoToDto).ToArray();
        }

        private static VacationDTO MapVacationInfoToDto(VacationInfo vacationInfo)
        {
            return new VacationDTO
            {
                DateFrom = vacationInfo.DateFrom,
                DateTo = vacationInfo.DateTo
            };
        }
    }
}