using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Group;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Lottery;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Notifications;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Domain.Services.Picture
{
    public interface IPictureReferenceService
    {
        /// <summary>How many stored records currently point at the given picture key, across every table that holds pictures.</summary>
        Task<int> CountReferencesAsync(string blobKey);
    }

    /// <summary>
    /// Picture keys are public (every avatar URL shows one) and clients submit them freely, so "delete the
    /// previous picture" must not trust the caller. Before a file is removed, the picture must not be in
    /// use by anything other than the single record that is about to drop it.
    /// </summary>
    public class PictureReferenceService : IPictureReferenceService
    {
        private readonly IUnitOfWork2 _uow;

        public PictureReferenceService(IUnitOfWork2 uow)
        {
            _uow = uow;
        }

        public async Task<int> CountReferencesAsync(string blobKey)
        {
            if (string.IsNullOrEmpty(blobKey))
            {
                return 0;
            }

            var count = 0;
            count += await _uow.GetDbSet<ApplicationUser>().IgnoreQueryFilters().CountAsync(u => u.PictureId == blobKey);
            count += await _uow.GetDbSet<Floor>().IgnoreQueryFilters().CountAsync(f => f.PictureId == blobKey);
            count += await _uow.GetDbSet<Banner>().IgnoreQueryFilters().CountAsync(b => b.PictureId == blobKey);
            count += await _uow.GetDbSet<Committee>().IgnoreQueryFilters().CountAsync(c => c.PictureId == blobKey);
            count += await _uow.GetDbSet<Group>().IgnoreQueryFilters().CountAsync(g => g.PictureId == blobKey);
            count += await _uow.GetDbSet<KudosLog>().IgnoreQueryFilters().CountAsync(k => k.PictureId == blobKey);
            count += await _uow.GetDbSet<KudosShopItem>().IgnoreQueryFilters().CountAsync(k => k.PictureId == blobKey);
            count += await _uow.GetDbSet<Notification>().IgnoreQueryFilters().CountAsync(n => n.PictureId == blobKey);
            count += await _uow.GetDbSet<ServiceRequest>().IgnoreQueryFilters().CountAsync(s => s.PictureId == blobKey);
            count += await _uow.GetDbSet<VideoLibraryItem>().IgnoreQueryFilters().CountAsync(v => v.PictureId == blobKey);
            count += await _uow.GetDbSet<Event>().IgnoreQueryFilters().CountAsync(e => e.ImageName == blobKey);
            count += await _uow.GetDbSet<Project>().IgnoreQueryFilters().CountAsync(p => p.Logo == blobKey);
            count += await _uow.GetDbSet<Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall>().IgnoreQueryFilters().CountAsync(w => w.Logo == blobKey);

            // Posts, comments and lotteries store a serialized list of keys.
            count += await _uow.GetDbSet<Post>().IgnoreQueryFilters().CountAsync(p => p.Images.Serialized.Contains(blobKey));
            count += await _uow.GetDbSet<Comment>().IgnoreQueryFilters().CountAsync(c => c.Images.Serialized.Contains(blobKey));
            count += await _uow.GetDbSet<Lottery>().IgnoreQueryFilters().CountAsync(l => l.Images.Serialized.Contains(blobKey));

            return count;
        }
    }
}
