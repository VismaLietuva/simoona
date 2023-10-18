using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Banners;

namespace Shrooms.Domain.Services.Banners
{
    public interface IBannerWidgetService
    {
        Task<IEnumerable<BannerWidgetDto>> GetBannersAsync(int organizationId);
    }
}
