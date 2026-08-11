using System.IO;
using System.Threading.Tasks;

namespace Shrooms.Domain.ServiceValidators.Validators.Emoji
{
    public interface ICustomEmojiValidator
    {
        void CheckNameFormat(string name);

        Task CheckIfNameIsTakenAsync(string name, int organizationId);

        Task CheckImageAsync(Stream content);
    }
}
