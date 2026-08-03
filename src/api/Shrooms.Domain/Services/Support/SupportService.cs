using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Support;
using Shrooms.Contracts.Infrastructure;
using Shrooms.Contracts.Infrastructure.Email;
using Shrooms.DataLayer.EntityModels.Models;
using MailAttachment = System.Net.Mail.Attachment;

namespace Shrooms.Domain.Services.Support
{
    public class SupportService : ISupportService
    {
        private readonly DbSet<ApplicationUser> _applicationUsers;
        private readonly IMailingService _mailingService;
        private readonly IApplicationSettings _applicationSettings;

        public SupportService(IUnitOfWork2 uow, IMailingService mailingService, IApplicationSettings applicationSettings)
        {
            _mailingService = mailingService;
            _applicationSettings = applicationSettings;
            _applicationUsers = uow.GetDbSet<ApplicationUser>();
        }

        public async Task SubmitTicketAsync(UserAndOrganizationDto userAndOrganization, SupportDto support)
        {
            var currentApplicationUser = await _applicationUsers.SingleAsync(u => u.Id == userAndOrganization.UserId);

            var email = new EmailDto(currentApplicationUser.FullName, currentApplicationUser.Email, _applicationSettings.SupportEmail, $"{support.Type}: {support.Subject}", support.Message);

            // The stream backing a MailAttachment must stay open until the message is
            // sent, so it is disposed only after SendEmailAsync completes.
            MemoryStream attachmentStream = null;

            try
            {
                if (support.Attachment != null)
                {
                    attachmentStream = new MemoryStream(support.Attachment.Content);
                    email.Attachment = new MailAttachment(attachmentStream, support.Attachment.FileName, support.Attachment.ContentType);
                }

                await _mailingService.SendEmailAsync(email, true);
            }
            finally
            {
                email.Attachment?.Dispose();
                attachmentStream?.Dispose();
            }
        }
    }
}
