﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Monitors;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Monitors;

namespace Shrooms.Domain.Services.Monitors
{
    public class MonitorService : IMonitorService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly IDbSet<Monitor> _monitorsDbSet;

        public MonitorService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _monitorsDbSet = uow.GetDbSet<Monitor>();
        }

        public async Task<IEnumerable<MonitorDto>> GetMonitorListAsync(int organizationId)
        {
            var monitors = await _monitorsDbSet
                .Where(x => x.OrganizationId == organizationId)
                .Select(x => new MonitorDto { Id = x.Id, Name = x.Name })
                .ToListAsync();

            return monitors;
        }

        public async Task CreateMonitorAsync(MonitorDto newMonitor, UserAndOrganizationDto userAndOrganizationDto)
        {
            if (await _monitorsDbSet.AnyAsync(x => x.Name == newMonitor.Name &&
                                                   x.OrganizationId == userAndOrganizationDto.OrganizationId))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Monitor names should be unique");
            }

            var timestamp = DateTime.UtcNow;
            var monitor = new Monitor
            {
                Created = timestamp,
                Modified = timestamp,
                CreatedBy = userAndOrganizationDto.UserId,
                ModifiedBy = userAndOrganizationDto.UserId,
                Name = newMonitor.Name,
                OrganizationId = userAndOrganizationDto.OrganizationId
            };

            _monitorsDbSet.Add(monitor);
            await _uow.SaveChangesAsync(false);
        }

        public async Task<MonitorDto> GetMonitorDetailsAsync(int organizationId, int monitorId)
        {
            var monitors = await _monitorsDbSet
                .Where(x => x.OrganizationId == organizationId && x.Id == monitorId)
                .Select(x => new MonitorDto { Id = x.Id, Name = x.Name })
                .FirstOrDefaultAsync();

            if (monitors == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Monitor does not exist");
            }

            return monitors;
        }

        public async Task UpdateMonitorAsync(MonitorDto monitorDto, UserAndOrganizationDto userAndOrganizationDto)
        {
            var monitor = await _monitorsDbSet.FirstOrDefaultAsync(x => x.Id == monitorDto.Id &&
                                                                        x.OrganizationId == userAndOrganizationDto.OrganizationId);

            if (monitor == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Monitor does not exist");
            }

            var nameAlreadyExist = await _monitorsDbSet.AnyAsync(x => x.Name == monitorDto.Name &&
                                                                      x.OrganizationId == userAndOrganizationDto.OrganizationId);

            if (monitorDto.Name != monitor.Name && nameAlreadyExist)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Monitor names should be unique");
            }

            monitor.Name = monitorDto.Name;
            monitor.Modified = DateTime.UtcNow;
            monitor.ModifiedBy = userAndOrganizationDto.UserId;

            await _uow.SaveChangesAsync(false);
        }
    }
}
