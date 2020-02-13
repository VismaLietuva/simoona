using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Shrooms.EntityModels.Models;
using Shrooms.EntityModels.Models.Multiwall;
using Shrooms.Host.Contracts.Constants;
using Shrooms.Host.Contracts.DAL;
using Shrooms.Host.Contracts.DataTransferObjects;
using Shrooms.Host.Contracts.Infrastructure;
using Shrooms.Host.Contracts.Infrastructure.Email;

namespace Shrooms.Domain.Services.DailyMailingService
{
    public class DailyMailingService : IDailyMailingService
    {
        private readonly IDbSet<ApplicationUser> _applicationUserDbSeb;
        private readonly IDbSet<Post> _postDbSet;
        private readonly IMailingService _emailService;
        private readonly ISystemClock _systemClock;

        public DailyMailingService(
            IUnitOfWork2 unitOfWork,
            ISystemClock systemClock,
            IMailingService emailService)
        {
            _applicationUserDbSeb = unitOfWork.GetDbSet<ApplicationUser>();
            _postDbSet = unitOfWork.GetDbSet<Post>();
            _systemClock = systemClock;
            _emailService = emailService;
        }

        public void SendDigestedWallPosts()
        {
            var todaysDate = _systemClock.UtcNow;
            var yesterdaysDate = _systemClock.UtcNow.AddDays(-1);

            // Select users that want to receive emails at this hour
            var usersToEmail = _applicationUserDbSeb
                .Include(u => u.Organization)
                .Include(u => u.WallUsers.Select(y => y.Wall))
                .Where(u => u.DailyMailingHour.HasValue && u.DailyMailingHour.Value.Hours == _systemClock.UtcNow.Hour)
                .ToList();

            if (!usersToEmail.Any())
            {
                return;
            }

            // Select new posts created in 24 hours
            var postsToEmail = _postDbSet
                .Include(p => p.Author)
                .Where(p => p.Created <= todaysDate && p.Created > yesterdaysDate)
                .ToList();

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
                    SendEmail(user.Email, sendPosts, user.Organization.ShortName);
                }
            }
        }

        private void SendEmail(string userEmail, IEnumerable<Post> wallPosts, string organizationShortName)
        {
            var messageBody = GetMessageBody(wallPosts, organizationShortName);
            var messageSubject = BusinessLayerConstants.ShroomsInfoEmailSubject;

            var emailDTO = new EmailDto(userEmail, messageSubject, messageBody);

            _emailService.SendEmail(emailDTO);
        }

        private string GetMessageBody(IEnumerable<Post> wallPosts, string organizationShortName)
        {
            var wallPostList = new StringBuilder();
            foreach (var post in wallPosts)
            {
                var displayName = post.Author == null ? BusinessLayerConstants.DeletedUserName :
                    (string.IsNullOrEmpty(post.Author.FirstName) && string.IsNullOrEmpty(post.Author.LastName)
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
