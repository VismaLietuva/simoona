using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shrooms.Contracts.DAL;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Committee;
using Shrooms.DataLayer.EntityModels.Models.Emoji;
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
        /// <summary>Whether any stored record points at the given picture key, across every table that holds pictures, including custom emoji.</summary>
        Task<bool> IsReferencedAsync(string blobKey);
    }

    /// <summary>
    /// Picture keys are public (every avatar URL shows one) and clients submit them freely, so "delete the
    /// previous picture" must not trust the caller. Before a file is removed, the picture must not be in
    /// use by anything other than the single record that is about to drop it. The checks stop at the first
    /// hit; the cheapest and most likely tables come first, the substring scans of serialized lists last.
    /// </summary>
    public class PictureReferenceService : IPictureReferenceService
    {
        private readonly IUnitOfWork2 _uow;

        public PictureReferenceService(IUnitOfWork2 uow)
        {
            _uow = uow;
        }

        public async Task<bool> IsReferencedAsync(string blobKey)
        {
            if (string.IsNullOrEmpty(blobKey))
            {
                return false;
            }

            return await _uow.GetDbSet<ApplicationUser>().IgnoreQueryFilters().AnyAsync(u => u.PictureId == blobKey)
                || await _uow.GetDbSet<Group>().IgnoreQueryFilters().AnyAsync(g => g.PictureId == blobKey)
                || await _uow.GetDbSet<Committee>().IgnoreQueryFilters().AnyAsync(c => c.PictureId == blobKey)
                || await _uow.GetDbSet<Floor>().IgnoreQueryFilters().AnyAsync(f => f.PictureId == blobKey)
                || await _uow.GetDbSet<RoomType>().IgnoreQueryFilters().AnyAsync(r => r.IconId == blobKey)
                || await _uow.GetDbSet<Banner>().IgnoreQueryFilters().AnyAsync(b => b.PictureId == blobKey)
                || await _uow.GetDbSet<KudosLog>().IgnoreQueryFilters().AnyAsync(k => k.PictureId == blobKey)
                || await _uow.GetDbSet<KudosShopItem>().IgnoreQueryFilters().AnyAsync(k => k.PictureId == blobKey)
                || await _uow.GetDbSet<Notification>().IgnoreQueryFilters().AnyAsync(n => n.PictureId == blobKey)
                || await _uow.GetDbSet<ServiceRequest>().IgnoreQueryFilters().AnyAsync(s => s.PictureId == blobKey)
                || await _uow.GetDbSet<VideoLibraryItem>().IgnoreQueryFilters().AnyAsync(v => v.PictureId == blobKey)
                || await _uow.GetDbSet<Event>().IgnoreQueryFilters().AnyAsync(e => e.ImageName == blobKey)
                || await _uow.GetDbSet<Project>().IgnoreQueryFilters().AnyAsync(p => p.Logo == blobKey)
                || await _uow.GetDbSet<Shrooms.DataLayer.EntityModels.Models.Multiwall.Wall>().IgnoreQueryFilters().AnyAsync(w => w.Logo == blobKey)
                || await _uow.GetDbSet<CustomEmoji>().IgnoreQueryFilters().AnyAsync(e => e.BlobName == blobKey)
                // Lottery.Images is mapped as a primitive collection (not an owned Serialized string).
                || await _uow.GetDbSet<Lottery>().IgnoreQueryFilters().AnyAsync(l => l.Images.Contains(blobKey))
                // Posts and comments store a serialized list of keys: substring scans, kept last.
                || await _uow.GetDbSet<Post>().IgnoreQueryFilters().AnyAsync(p => p.Images.Serialized.Contains(blobKey))
                || await _uow.GetDbSet<Comment>().IgnoreQueryFilters().AnyAsync(c => c.Images.Serialized.Contains(blobKey));
        }
    }
}
