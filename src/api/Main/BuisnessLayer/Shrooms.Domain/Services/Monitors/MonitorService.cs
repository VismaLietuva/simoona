using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Shrooms.DataTransferObjects.Models;
using Shrooms.DataTransferObjects.Models.Monitors;
using Shrooms.EntityModels.Models.Monitors;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.Exceptions;

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

        public IEnumerable<MonitorDTO> GetMonitorList(int organizationId)
        {
            var monitors = _monitorsDbSet
                .Where(x => x.OrganizationId == organizationId)
                .Select(x => new MonitorDTO { Id = x.Id, Name = x.Name });

            return monitors;
        }

        public void CreateMonitor(MonitorDTO newMonitor, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            if (_monitorsDbSet.Any(x => x.Name == newMonitor.Name &&
                               x.OrganizationId == userAndOrganizationDTO.OrganizationId))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Monitor names should be unique");
            }

            var timestamp = DateTime.UtcNow;
            var monitor = new Monitor()
            {
                Created = timestamp,
                Modified = timestamp,
                CreatedBy = userAndOrganizationDTO.UserId,
                ModifiedBy = userAndOrganizationDTO.UserId,
                Name = newMonitor.Name,
                OrganizationId = userAndOrganizationDTO.OrganizationId
            };
            _monitorsDbSet.Add(monitor);
            _uow.SaveChanges(false);
        }

        public MonitorDTO GetMonitorDetails(int organizationId, int monitorId)
        {
            var monitors = _monitorsDbSet
                .Where(x => x.OrganizationId == organizationId && x.Id == monitorId)
                .Select(x => new MonitorDTO { Id = x.Id, Name = x.Name })
                .FirstOrDefault();

            if (monitors == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Monitor does not exist");
            }

            return monitors;
        }

        public void UpdateMonitor(MonitorDTO monitorDTO, UserAndOrganizationDTO userAndOrganizationDTO)
        {
            var monitor = _monitorsDbSet.FirstOrDefault(x => x.Id == monitorDTO.Id &&
                                         x.OrganizationId == userAndOrganizationDTO.OrganizationId);

            if (monitor == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Monitor does not exist");
            }

            var nameAlreadyExist = _monitorsDbSet.Any(x => x.Name == monitorDTO.Name &&
                               x.OrganizationId == userAndOrganizationDTO.OrganizationId);

            if (monitorDTO.Name != monitor.Name && nameAlreadyExist)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Monitor names should be unique");
            }

            monitor.Name = monitorDTO.Name;
            monitor.Modified = DateTime.UtcNow;
            monitor.ModifiedBy = userAndOrganizationDTO.UserId;
            _uow.SaveChanges(false);
        }
    }
}
