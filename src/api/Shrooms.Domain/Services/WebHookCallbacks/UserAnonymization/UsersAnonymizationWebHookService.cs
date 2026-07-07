using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Picture;

namespace Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization
{
    public class UsersAnonymizationWebHookService : IUsersAnonymizationWebHookService
    {
        private readonly int _anonymizeUsersAfterDays;
        private readonly int _anonymizeUsersPerRequest;

        private readonly DbSet<ApplicationUser> _usersDbSet;
        private readonly DbSet<Organization> _organizationsDbSet;

        private readonly IUnitOfWork2 _uow;
        private readonly IPictureService _pictureService;

        public UsersAnonymizationWebHookService(IUnitOfWork2 uow, IPictureService pictureService, IConfiguration configuration)
        {
            _anonymizeUsersAfterDays = int.TryParse(configuration["AnonymizeUsersAfterDays"], out var days) ? days : 14;
            _anonymizeUsersPerRequest = int.TryParse(configuration["AnonymizeUsersPerRequest"], out var perReq) ? perReq : 10;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();

            _pictureService = pictureService;
            _uow = uow;
        }

        public async Task AnonymizeUsersAsync(string organizationName)
        {
            var organization = await _organizationsDbSet.FirstAsync(org => org.ShortName == organizationName);

            var cutoffDate = DateTime.UtcNow.AddDays(-_anonymizeUsersAfterDays);
            var usersToAnonymize = await _usersDbSet
                .Where(u => u.IsDeleted &&
                            u.OrganizationId == organization.Id &&
                            !u.IsAnonymized &&
                            u.Modified <= cutoffDate)
                .Take(_anonymizeUsersPerRequest)
                .ToListAsync();

            foreach (var user in usersToAnonymize)
            {
                await AnonymizeAsync(user, organization.Id);

                await _uow.SaveChangesAsync();
            }
        }

        private async Task AnonymizeAsync(ApplicationUser user, int organizationId)
        {
            if (!string.IsNullOrEmpty(user.PictureId))
            {
                await _pictureService.RemoveImageAsync(user.PictureId, organizationId);
            }

            var randomString = Guid.NewGuid().ToString();

            user.Email = randomString;
            user.FirstName = randomString;
            user.LastName = randomString;
            user.PhoneNumber = randomString;
            user.UserName = randomString;
            user.FacebookEmail = randomString;
            user.GoogleEmail = randomString;
            user.MicrosoftEmail = randomString;
            user.Bio = string.Empty;
            user.PictureId = string.Empty;
            user.BirthDay = DateTime.UtcNow;
            user.IsAnonymized = true;
        }
    }
}