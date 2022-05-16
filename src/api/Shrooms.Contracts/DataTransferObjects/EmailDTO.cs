using System.Collections.Generic;
using System.Net.Mail;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DataTransferObjects.Users;

namespace Shrooms.Contracts.DataTransferObjects
{
    public class EmailDto
    {
        public string SenderFullName { get; private set; }

        public string SenderEmail { get; private set; }

        public IEnumerable<string> Receivers { get; private set; }

        public string Subject { get; private set; }

        public string Body { get; private set; }

        public Attachment Attachment { get; set; }

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
