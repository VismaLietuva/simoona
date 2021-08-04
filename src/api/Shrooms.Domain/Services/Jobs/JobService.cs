using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Jobs;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Jobs
{
    public class JobService : IJobService
    {
        private readonly IDbSet<JobPosition> _jobTypesDbSet;
        private readonly IUnitOfWork2 _uow;

        public JobService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _jobTypesDbSet = uow.GetDbSet<JobPosition>();
        }

        public async Task<JobTypeDto> GetJobTypeAsync(int id, UserAndOrganizationDto userOrg)
        {
            var type = await _jobTypesDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId && t.Id == id)
                .Select(MapJobTypesToDto())
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            return type;
        }

        public async Task<IEnumerable<JobTypeDto>> GetJobTypesAsync(UserAndOrganizationDto userAndOrg)
        {
            var jobTypes = await _jobTypesDbSet
                .Where(t => t.OrganizationId == userAndOrg.OrganizationId)
                .Select(MapJobTypesToDto())
                .ToListAsync();

            return jobTypes;
        }

        public async Task CreateJobTypeAsync(JobTypeDto jobType)
        {
            var alreadyExists = await _jobTypesDbSet
                .AnyAsync(t => t.Title == jobType.Title && t.OrganizationId == jobType.OrganizationId);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Job position with that title already exists");
            }

            var newType = new JobPosition
            {
                Title = jobType.Title,
                CreatedBy = jobType.UserId,
                OrganizationId = jobType.OrganizationId
            };

            _jobTypesDbSet.Add(newType);

            await _uow.SaveChangesAsync(jobType.UserId);
        }

        public async Task RemoveJobTypeAsync(int id, UserAndOrganizationDto userOrg)
        {
            var type = await _jobTypesDbSet
                .Where(t => t.OrganizationId == userOrg.OrganizationId && t.Id == id)
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            _jobTypesDbSet.Remove(type);

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task UpdateJobTypeAsync(JobTypeDto jobType)
        {
            var alreadyExists = await _jobTypesDbSet
                .AnyAsync(t => t.Title == jobType.Title && t.OrganizationId == jobType.OrganizationId && t.Id != jobType.Id);

            if (alreadyExists)
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Job position with that title already exists");
            }

            var type = await _jobTypesDbSet
                .Where(t => t.OrganizationId == jobType.OrganizationId && t.Id == jobType.Id)
                .FirstOrDefaultAsync();

            if (type == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Type not found");
            }

            type.Title = jobType.Title;

            await _uow.SaveChangesAsync(jobType.UserId);
        }

        private static Expression<Func<JobPosition, JobTypeDto>> MapJobTypesToDto()
        {
            return jobType => new JobTypeDto
            {
                Id = jobType.Id,
                Title = jobType.Title
            };
        }
    }
}
