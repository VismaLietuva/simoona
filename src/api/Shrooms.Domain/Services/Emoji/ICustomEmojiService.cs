using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Emoji;

namespace Shrooms.Domain.Services.Emoji
{
    public interface ICustomEmojiService
    {
        Task<IEnumerable<CustomEmojiDto>> GetAllAsync(UserAndOrganizationDto userOrg, string tenantName);

        Task<CustomEmojiDto> CreateAsync(string name, Stream stream, string mimeType, string fileName, UserAndOrganizationDto userOrg, string tenantName);

        Task DeleteAsync(int id, UserAndOrganizationDto userOrg);
    }
}
