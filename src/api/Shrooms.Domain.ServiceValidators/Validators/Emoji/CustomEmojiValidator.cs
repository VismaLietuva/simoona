using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Emoji;

namespace Shrooms.Domain.ServiceValidators.Validators.Emoji
{
    public class CustomEmojiValidator : ICustomEmojiValidator
    {
        private static readonly Regex NameRegex = new(ValidationConstants.CustomEmojiNameRegexPattern, RegexOptions.Compiled);

        private readonly DbSet<CustomEmoji> _customEmojisDbSet;

        public CustomEmojiValidator(IUnitOfWork2 uow)
        {
            _customEmojisDbSet = uow.GetDbSet<CustomEmoji>();
        }

        public void CheckNameFormat(string name)
        {
            if (name == null || !NameRegex.IsMatch(name))
            {
                throw new ValidationException(ErrorCodes.InvalidCustomEmojiName, "Emoji name is invalid");
            }
        }

        public async Task CheckIfNameIsTakenAsync(string name, int organizationId)
        {
            if (await _customEmojisDbSet.AnyAsync(x => x.OrganizationId == organizationId && x.Name == name && !x.IsDeleted))
            {
                throw new ValidationException(ErrorCodes.DuplicatesIntolerable, "Emoji name already exists");
            }
        }

        public void CheckIfEmojiExists(CustomEmoji emoji)
        {
            if (emoji == null)
            {
                throw new ValidationException(ErrorCodes.ContentDoesNotExist, "Emoji does not exist");
            }
        }
    }
}
