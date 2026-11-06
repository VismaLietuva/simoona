using Microsoft.AspNetCore.Identity;
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
        private readonly DbSet<IdentityUserLogin<string>> _userLoginsDbSet;
        private readonly DbSet<IdentityUserClaim<string>> _userClaimsDbSet;
        private readonly DbSet<IdentityUserToken<string>> _userTokensDbSet;

        private readonly IUnitOfWork2 _uow;
        private readonly IPictureService _pictureService;

        public UsersAnonymizationWebHookService(IUnitOfWork2 uow, IPictureService pictureService, IConfiguration configuration)
        {
            _anonymizeUsersAfterDays = int.TryParse(configuration["AnonymizeUsersAfterDays"], out var days) ? days : 14;
            _anonymizeUsersPerRequest = int.TryParse(configuration["AnonymizeUsersPerRequest"], out var perReq) ? perReq : 10;

            _usersDbSet = uow.GetDbSet<ApplicationUser>();
            _organizationsDbSet = uow.GetDbSet<Organization>();
            _userLoginsDbSet = uow.GetDbSet<IdentityUserLogin<string>>();
            _userClaimsDbSet = uow.GetDbSet<IdentityUserClaim<string>>();
            _userTokensDbSet = uow.GetDbSet<IdentityUserToken<string>>();

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
                // The photo is the user's own data: delete even if referenced (PutPersonalInfo stops users adopting foreign keys).
                // Delete before the save so a storage failure leaves the user for the next run.
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
            // Identity looks users up by these, so they must match what its normalizer makes of the new values.
            user.NormalizedEmail = randomString.ToUpperInvariant();
            user.NormalizedUserName = randomString.ToUpperInvariant();
            user.PasswordHash = null;
            user.FacebookEmail = randomString;
            user.GoogleEmail = randomString;
            user.MicrosoftEmail = randomString;
            user.Bio = string.Empty;
            user.AbsentComment = null;
            user.PictureId = string.Empty;
            user.BirthDay = DateTime.UtcNow;
            user.IsAnonymized = true;

            // Deleting a user drops its logins already; this catches rows left by older deletes or other paths.
            // Provider keys are the user's id at Google/Facebook/Microsoft, and claims and tokens can carry anything.
            _userLoginsDbSet.RemoveRange(await _userLoginsDbSet.Where(login => login.UserId == user.Id).ToListAsync());
            _userClaimsDbSet.RemoveRange(await _userClaimsDbSet.Where(claim => claim.UserId == user.Id).ToListAsync());
            _userTokensDbSet.RemoveRange(await _userTokensDbSet.Where(token => token.UserId == user.Id).ToListAsync());
        }
    }
}