using System.Collections.Generic;

namespace Shrooms.Domain.Services.Email.Converters
{
    public class CompiledEmailTemplateWithReceiverEmails
    {
        public string Body { get; set; }

        public IEnumerable<string> ReceiverEmails { get; set; }
    }
}
