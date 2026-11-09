using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Shrooms.Contracts.Enums;
using Shrooms.DataLayer.EntityModels.Models;
using Shrooms.DataLayer.EntityModels.Models.Books;
using Shrooms.DataLayer.EntityModels.Models.Events;
using Shrooms.DataLayer.EntityModels.Models.Kudos;
using Shrooms.DataLayer.EntityModels.Models.Multiwall;
using Shrooms.DataLayer.EntityModels.Models.Polls;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;
using GroupEntity = Shrooms.DataLayer.EntityModels.Models.Group.Group;

namespace Shrooms.DataLayer.DAL.Search
{
    /// <summary>
    /// The searchable entities a save touched. Captured before the save, while the
    /// change tracker still knows what changed, and resolved to keys after it, once
    /// new rows have their generated ids.
    /// </summary>
    internal sealed class SearchIndexChangeSet
    {
        private readonly List<(SearchIndexSource Source, Func<string> Key)> _changes;

        private SearchIndexChangeSet(List<(SearchIndexSource Source, Func<string> Key)> changes)
        {
            _changes = changes;
        }

        public static SearchIndexChangeSet Capture(ChangeTracker changeTracker)
        {
            if (!SearchIndexing.IsEnabled)
            {
                return null;
            }

            var changes = new List<(SearchIndexSource, Func<string>)>();
            foreach (var entry in changeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                {
                    continue;
                }

                var change = Describe(entry);
                if (change.HasValue)
                {
                    changes.Add(change.Value);
                }
            }

            return changes.Count == 0 ? null : new SearchIndexChangeSet(changes);
        }

        public IReadOnlyList<(SearchIndexSource Source, IReadOnlyCollection<string> Keys)> Resolve()
        {
            return _changes
                .GroupBy(change => change.Source)
                .Select(group => (group.Key, (IReadOnlyCollection<string>)group
                    .Select(change => change.Key())
                    .Where(key => !string.IsNullOrEmpty(key))
                    .Distinct()
                    .ToList()))
                .Where(change => change.Item2.Count > 0)
                .ToList();
        }

        // A wall's posts carry its name and visibility; anything else about a wall
        // (its description, logo, notification settings) leaves them untouched.
        private static readonly HashSet<string> WallPropertiesInPosts = new()
        {
            nameof(Wall.Name), nameof(Wall.Type), nameof(Wall.IsDeleted), nameof(Wall.OrganizationId)
        };

        private static (SearchIndexSource, Func<string>)? Describe(EntityEntry entry)
        {
            return entry.Entity switch
            {
                Wall wall when entry.State == EntityState.Modified &&
                               entry.Properties.Any(p => p.IsModified && WallPropertiesInPosts.Contains(p.Metadata.Name))
                    => (SearchIndexSources.PostsByWall, () => wall.Id.ToString()),
                IdentityUserRole<string> userRole => (SearchIndexSources.People, () => userRole.UserId),
                Post post => (SearchIndexSources.Posts, () => post.Id.ToString()),
                ApplicationUser user => (SearchIndexSources.People, () => user.Id),
                Event @event => (SearchIndexSources.Events, () => @event.Id.ToString()),
                KudosLog kudos => (SearchIndexSources.Kudos, () => kudos.Id.ToString()),
                Project project => (SearchIndexSources.Projects, () => project.Id.ToString()),
                GroupEntity group => (SearchIndexSources.Groups, () => group.Id.ToString()),
                BookOffice bookOffice => (SearchIndexSources.BookOffices, () => bookOffice.Id.ToString()),
                Book book => (SearchIndexSources.BooksByBook, () => book.Id.ToString()),
                Poll poll => (SearchIndexSources.Polls, () => poll.Id.ToString()),
                VideoLibraryItem video => (SearchIndexSources.Videos, () => video.Id.ToString()),
                ServiceRequest request => (SearchIndexSources.ServiceRequests, () => request.Id.ToString()),
                _ => null
            };
        }
    }
}
