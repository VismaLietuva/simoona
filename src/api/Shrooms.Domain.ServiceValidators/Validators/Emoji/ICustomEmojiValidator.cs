using System.Threading.Tasks;
using Shrooms.DataLayer.EntityModels.Models.Emoji;

namespace Shrooms.Domain.ServiceValidators.Validators.Emoji
{
    public interface ICustomEmojiValidator
    {
        void CheckNameFormat(string name);

        Task CheckIfNameIsTakenAsync(string name, int organizationId);

        void CheckIfEmojiExists(CustomEmoji emoji);
    }
}
