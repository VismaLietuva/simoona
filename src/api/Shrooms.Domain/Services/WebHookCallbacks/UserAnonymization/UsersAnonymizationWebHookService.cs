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
                // The photo is the user's personal data: it goes even if someone else pointed their own record at
                // the key. The converse (this user having adopted a colleague's key so that the colleague's
                // avatar is deleted here) is prevented where the key is written: PutPersonalInfo refuses a key
                // that is already in use. Deleting before the save means a storage failure throws here and
                // leaves the user un-anonymized, so the next run retries instead of marking the photo as handled.
                if (!string.IsNullOrEmpty(user.PictureId))
                {
                    await _pictureService.RemoveImageIgnoringReferencesAsync(user.PictureId, organization.Id);
                }

                await AnonymizeAsync(user, organization.Id);

                await _uow.SaveChangesAsync();
            }
        }

        private async Task AnonymizeAsync(ApplicationUser user, int organizationId)
        {
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