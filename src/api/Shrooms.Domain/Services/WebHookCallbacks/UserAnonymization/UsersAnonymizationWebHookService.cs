using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.Domain.Services.Picture;

namespace Shrooms.Domain.Services.WebHookCallbacks.UserAnonymization
{
    public class UsersAnonymizationWebHookService : IUsersAnonymizationWebHookService
    {
        private readonly int _anonymizeUsersAfterDays;
        private readonly int _anonymizeUsersPerRequest;

        private readonly ShroomsDbContext _shroomsContext;

        private readonly IUnitOfWork _uow;
        private readonly IPictureService _pictureService;

        public UsersAnonymizationWebHookService(IUnitOfWork uow, IPictureService pictureService)
        {
            _anonymizeUsersAfterDays = Convert.ToInt32(ConfigurationManager.AppSettings["AnonymizeUsersAfterDays"]);
            _anonymizeUsersPerRequest = Convert.ToInt32(ConfigurationManager.AppSettings["AnonymizeUsersPerRequest"]);

            _shroomsContext = uow.GetDbContextAs<ShroomsDbContext>();

            _pictureService = pictureService;
            _uow = uow;
        }

        public async Task AnonymizeUsersAsync(int organizationId)
        {
            var sqlQuery = @"SELECT TOP(@userLimit) * FROM [dbo].[AspNetUsers] WHERE
                             IsDeleted = 1 AND
                             OrganizationId = @organizationId AND
                             IsAnonymized = 0 AND
                             DATEDIFF(DAY, Modified, GETDATE()) >= @anonymizeAfterDays";

            var sqlParameters = new object[]
            {
                new SqlParameter("@organizationId", organizationId),
                new SqlParameter("@anonymizeAfterDays", _anonymizeUsersAfterDays),
                new SqlParameter("@userLimit", _anonymizeUsersPerRequest)
            };

            var usersToAnonymize = await _shroomsContext.Users.SqlQuery(sqlQuery, sqlParameters).ToListAsync();

            foreach (var user in usersToAnonymize)
            {
                await AnonymizeAsync(user, organizationId);

                await _uow.SaveAsync();
            }
        }

        private async Task AnonymizeAsync(ApplicationUser user, int organizationId)
        {
            await _pictureService.RemoveImageAsync(user.PictureId, organizationId);

            var randomString = Guid.NewGuid().ToString();

            user.Email = randomString;
            user.FirstName = randomString;
            user.LastName = randomString;
            user.PhoneNumber = randomString;
            user.UserName = randomString;
            user.FacebookEmail = randomString;
            user.GoogleEmail = randomString;
            user.Bio = string.Empty;
            user.PictureId = string.Empty;
            user.BirthDay = DateTime.UtcNow;
            user.IsAnonymized = true;
        }
    }
}