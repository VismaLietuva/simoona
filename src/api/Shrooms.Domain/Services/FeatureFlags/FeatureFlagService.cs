using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.FeatureFlags;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.FeatureFlags;

namespace Shrooms.Domain.Services.FeatureFlags
{
    public class FeatureFlagService : IFeatureFlagService
    {
        private readonly IUnitOfWork2 _uow;
        private readonly DbSet<FeatureFlag> _flags;
        private readonly DbSet<FeatureFlagUser> _flagUsers;
        private readonly DbSet<ApplicationUser> _users;

        public FeatureFlagService(IUnitOfWork2 uow)
        {
            _uow = uow;
            _flags = uow.GetDbSet<FeatureFlag>();
            _flagUsers = uow.GetDbSet<FeatureFlagUser>();
            _users = uow.GetDbSet<ApplicationUser>();
        }

        public async Task<IEnumerable<FeatureFlagDto>> GetAllAsync(UserAndOrganizationDto userOrg)
        {
            return await _flags
                .AsNoTracking()
                .Where(flag => flag.OrganizationId == userOrg.OrganizationId)
                .OrderBy(flag => flag.Name)
                .Select(flag => new FeatureFlagDto
                {
                    Id = flag.Id,
                    Name = flag.Name,
                    IsEnabled = flag.IsEnabled,
                    Users = flag.Users
                        .Where(member => !member.User.IsDeleted)
                        .OrderBy(member => member.User.FirstName)
                        .ThenBy(member => member.User.LastName)
                        .Select(member => new FeatureFlagUserDto
                        {
                            Id = member.UserId,
                            FullName = member.User.FirstName + " " + member.User.LastName,
                            PictureId = member.User.PictureId
                        })
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetEnabledAsync(UserAndOrganizationDto userOrg)
        {
            return await _flags
                .AsNoTracking()
                .Where(flag => flag.OrganizationId == userOrg.OrganizationId &&
                               (flag.IsEnabled || flag.Users.Any(member => member.UserId == userOrg.UserId)))
                .Select(flag => flag.Name)
                .ToListAsync();
        }

        public async Task<int> CreateAsync(NewFeatureFlagDto input, UserAndOrganizationDto userOrg)
        {
            var name = input.Name.Trim();

            if (await _flags.AnyAsync(flag => flag.OrganizationId == userOrg.OrganizationId && flag.Name == name))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, $"Feature flag \"{name}\" already exists");
            }

            var created = new FeatureFlag { OrganizationId = userOrg.OrganizationId, Name = name };
            _flags.Add(created);
            await _uow.SaveChangesAsync(userOrg.UserId);
            return created.Id;
        }

        public async Task UpdateAsync(int id, UpdateFeatureFlagDto input, UserAndOrganizationDto userOrg)
        {
            var flag = await _flags
                .Include(row => row.Users)
                .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Id == id) ?? throw NotFound();

            var requested = (input.UserIds ?? Enumerable.Empty<string>()).Distinct().ToList();
            var userIds = await _users
                .Where(user => user.OrganizationId == userOrg.OrganizationId && requested.Contains(user.Id))
                .Select(user => user.Id)
                .ToListAsync();

            flag.IsEnabled = input.IsEnabled;
            _flagUsers.RemoveRange(flag.Users.Where(member => !userIds.Contains(member.UserId)).ToList());

            foreach (var userId in userIds.Where(userId => flag.Users.All(member => member.UserId != userId)))
            {
                flag.Users.Add(new FeatureFlagUser { UserId = userId });
            }

            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        public async Task DeleteAsync(int id, UserAndOrganizationDto userOrg)
        {
            var flag = await _flags
                .FirstOrDefaultAsync(row => row.OrganizationId == userOrg.OrganizationId && row.Id == id) ?? throw NotFound();

            _flags.Remove(flag);
            await _uow.SaveChangesAsync(userOrg.UserId);
        }

        private static ValidationException NotFound()
        {
            return new ValidationException(ErrorCodes.ContentDoesNotExist, "Feature flag not found");
        }
    }
}
