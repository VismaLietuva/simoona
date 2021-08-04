using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;

namespace Shrooms.Domain.Services.DailyMailingService
{
    public class DailyMailingService : IDailyMailingService
    {
        private readonly IDbSet<ApplicationUser> _applicationUserDbSeb;
        private readonly IDbSet<Post> _postDbSet;
        private readonly IMailingService _emailService;
        private readonly ISystemClock _systemClock;

        public DailyMailingService(IUnitOfWork2 unitOfWork, ISystemClock systemClock, IMailingService emailService)
        {
            _applicationUserDbSeb = unitOfWork.GetDbSet<ApplicationUser>();
            _postDbSet = unitOfWork.GetDbSet<Post>();
            _systemClock = systemClock;
            _emailService = emailService;
        }

        public async Task SendDigestedWallPostsAsync()
        {
            var todayDate = _systemClock.UtcNow;
            var yesterdaysDate = _systemClock.UtcNow.AddDays(-1);

            // Select users that want to receive emails at this hour
            var usersToEmail = await _applicationUserDbSeb
                .Include(u => u.Organization)
                .Include(u => u.WallUsers.Select(y => y.Wall))
                .Where(u => u.DailyMailingHour.HasValue && u.DailyMailingHour.Value.Hours == _systemClock.UtcNow.Hour)
                .ToListAsync();

            if (!usersToEmail.Any())
            {
                return;
            }

            // Select new posts created in 24 hours
            var postsToEmail = await _postDbSet
                .Include(p => p.Author)
                .Where(p => p.Created <= todayDate && p.Created > yesterdaysDate)
                .ToListAsync();

            if (!postsToEmail.Any())
            {
                return;
            }

            foreach (var user in usersToEmail)
            {
                var subwallJoinDate = user.WallUsers
                    .Select(x => x.Created)
                    .Single();

                // Select posts that this user can see
                var sendPosts = postsToEmail
                    .Where(p => user.WallUsers.Any(w => w.WallId == p.WallId))
                    .Where(p => user.WallUsers.Any(w => w.WallId == p.WallId) || p.LastActivity > subwallJoinDate)
                    .ToList();

                // Send email if any posts left
                if (sendPosts.Any())
                {
                    await SendEmailAsync(user.Email, sendPosts, user.Organization.ShortName);
                }
            }
        }

        private async Task SendEmailAsync(string userEmail, IEnumerable<Post> wallPosts, string organizationShortName)
        {
            var messageBody = GetMessageBody(wallPosts, organizationShortName);

            var emailDTO = new EmailDto(userEmail, BusinessLayerConstants.ShroomsInfoEmailSubject, messageBody);

            await _emailService.SendEmailAsync(emailDTO);
        }

        private static string GetMessageBody(IEnumerable<Post> wallPosts, string organizationShortName)
        {
            var wallPostList = new StringBuilder();
            foreach (var post in wallPosts)
            {
                var displayName = post.Author == null
                    ? BusinessLayerConstants.DeletedUserName
                    : (string.IsNullOrEmpty(post.Author.FirstName) && string.IsNullOrEmpty(post.Author.LastName)
                        ? post.Author.UserName
                        : $"{post.Author.FirstName} {post.Author.LastName}");

                // Could be some issues with time zones, because post.Created is UTC, local is server's (not user's) local
                wallPostList.AppendFormat(
                    BusinessLayerConstants.WallPostsListTemplate,
                    post.Created.ToString("yyyy-MM-dd HH:mm"),
                    displayName,
                    post.MessageBody);
            }

            return string.Format(BusinessLayerConstants.ShroomsInfoEmailMessageBodyTemplate, wallPostList, BusinessLayerConstants.SimonaUrl, organizationShortName);
        }
    }
}
