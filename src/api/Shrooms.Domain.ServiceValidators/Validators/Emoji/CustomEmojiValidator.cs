using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.Constants;
using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Exceptions;
using Shrooms.DataLayer.EntityModels.Models.Emoji;
using SixLabors.ImageSharp;

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

        public async Task CheckImageAsync(Stream content)
        {
            if (!content.CanSeek)
            {
                throw new ArgumentException("Stream must be seekable", nameof(content));
            }

            IImageInfo imageInfo;
            try
            {
                imageInfo = await Image.IdentifyAsync(content);
            }
            catch (ImageFormatException)
            {
                throw new ValidationException(ErrorCodes.InvalidCustomEmojiImage, "Image file is corrupted or could not be decoded");
            }

            if (imageInfo == null)
            {
                throw new ValidationException(ErrorCodes.InvalidCustomEmojiImage, "File is not a recognized image format");
            }

            if (imageInfo.Width > WebApiConstants.MaximumCustomEmojiDimensionInPixels ||
                imageInfo.Height > WebApiConstants.MaximumCustomEmojiDimensionInPixels)
            {
                throw new ValidationException(ErrorCodes.CustomEmojiImageTooLarge, $"Image dimensions are too large. Maximum is {WebApiConstants.MaximumCustomEmojiDimensionInPixels}x{WebApiConstants.MaximumCustomEmojiDimensionInPixels} pixels");
            }

            content.Position = 0;
        }
    }
}
