using System;
using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.EntityModels.Models.Search
{
    // Read model for global search: one row per searchable item, written by raw SQL
    // (SearchIndexSql) and never through the change tracker.
    public class SearchDocument
    {
        public const int EntityIdMaxLength = 128;
        public const int TitleMaxLength = 400;
        public const int DescriptionMaxLength = 400;
        public const int AccessKeyMaxLength = 200;

        public long Id { get; set; }

        public int OrganizationId { get; set; }

        public SearchDocumentKind Kind { get; set; }

        public string EntityId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }

        public string PictureId { get; set; }

        /// <summary>Target of the result link when it is not the item itself, e.g. the kudos receiver.</summary>
        public string LinkId { get; set; }

        /// <summary>
        /// With OwnerUserId, limits who may find the item: when either is set, only the
        /// owner or a user holding this key (e.g. "wall:12", "category:IT") finds it.
        /// </summary>
        public string AccessKey { get; set; }

        public string OwnerUserId { get; set; }

        public string SearchText { get; set; }

        public DateTime IndexedAt { get; set; }
    }
}
