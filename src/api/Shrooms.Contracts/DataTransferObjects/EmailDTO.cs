using System.Collections.Generic;
using System.Net.Mail;
using Shrooms.Contracts.Constants;

namespace Shrooms.Contracts.DataTransferObjects
{
    public record EmailDto
    {
        public string SenderFullName { get; private set; }

        public string SenderEmail { get; private set; }

        public IEnumerable<string> Receivers { get; set; }

        public string Subject { get; private set; }

        public string Body { get; private set; }

        /// <summary>
        /// Files attached to the outgoing message. Never null; empty means no
        /// attachments. Callers own disposal — the streams behind these must stay
        /// open until the mail has been sent.
        /// </summary>
        public IList<Attachment> Attachments { get; } = new List<Attachment>();

        public EmailDto(string senderFullName, string senderEmail, IEnumerable<string> receivers, string subject, string body)
        {
            Body = body;
            Subject = subject;
            Receivers = receivers;
            SenderEmail = senderEmail;
            SenderFullName = senderFullName;
        }

        public EmailDto(string senderFullName, string senderEmail, string receiver, string subject, string body)
            : this(senderFullName, senderEmail, new List<string> { receiver }, subject, body)
        {
        }

        public EmailDto(IEnumerable<string> receivers, string subject, string body)
        {
            Body = body;
            Subject = subject;
            Receivers = receivers;
            SenderEmail = BusinessLayerConstants.FromEmailAddress;
            SenderFullName = BusinessLayerConstants.EmailSenderName;
        }

        public EmailDto(string receiver, string subject, string body)
        {
            Body = body;
            Subject = subject;
            Receivers = new List<string> { receiver };
            SenderEmail = BusinessLayerConstants.FromEmailAddress;
            SenderFullName = BusinessLayerConstants.EmailSenderName;
        }
    }
}
