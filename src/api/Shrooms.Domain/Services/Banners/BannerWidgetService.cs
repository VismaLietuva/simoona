using Shrooms.Contracts.DAL;
using Shrooms.Contracts.Infrastructure;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Banners;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.Services.Banners
{
    public class BannerWidgetService : IBannerWidgetService
    {
        private readonly DbSet<Banner> _bannersDbSet;
        private readonly ISystemClock _systemClock;

        public BannerWidgetService(IUnitOfWork2 uow, ISystemClock systemClock)
        {
            _bannersDbSet = uow.GetDbSet<Banner>();
            _systemClock = systemClock;
        }

        public async Task<IEnumerable<BannerWidgetDto>> GetBannersAsync(int organizationId)
        {
            return await _bannersDbSet
                .Where(x => x.OrganizationId == organizationId &&
                            (x.ValidFrom == null || x.ValidFrom <= _systemClock.UtcNow) &&
                            (x.ValidTo == null || x.ValidTo >= _systemClock.UtcNow))
                .Select(x => new BannerWidgetDto
                {
                    Url = x.Url,
                    PictureId = x.PictureId
                })
                .ToListAsync();
        }
    }
}
