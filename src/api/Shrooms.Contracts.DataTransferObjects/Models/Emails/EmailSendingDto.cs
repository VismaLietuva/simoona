using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Emails
{
    public class EmailSendingDto
    {
        public IEnumerable<string> BccAdresses { get; set; }

        public IEnumerable<string> DestinationAdresses { get; set; }

        public string Body { get; set; }

        public string Subject { get; set; }

        public string Sender { get; set; }

        public string Footer { get; set; }

        public bool Html { get; set; }
    }
}
