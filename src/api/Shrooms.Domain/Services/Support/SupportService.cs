using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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

            // The streams backing the MailAttachments must stay open until the message
            // is sent, so they are disposed only after SendEmailAsync completes.
            var attachmentStreams = new List<MemoryStream>();

            try
            {
                foreach (var attachment in support.Attachments)
                {
                    var attachmentStream = new MemoryStream(attachment.Content);
                    attachmentStreams.Add(attachmentStream);
                    email.Attachments.Add(new MailAttachment(attachmentStream, attachment.FileName, attachment.ContentType));
                }

                await _mailingService.SendEmailAsync(email, true);
            }
            finally
            {
                foreach (var attachment in email.Attachments)
                {
                    attachment.Dispose();
                }

                foreach (var attachmentStream in attachmentStreams)
                {
                    attachmentStream.Dispose();
                }
            }
        }
    }
}
