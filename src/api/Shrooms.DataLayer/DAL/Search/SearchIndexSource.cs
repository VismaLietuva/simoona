using Shrooms.Contracts.Enums;

namespace Shrooms.DataLayer.DAL.Search
{
    /// <summary>
    /// One SELECT that turns a source table into search rows. It returns every row,
    /// deleted or hidden ones included with IsSearchable = 0, so a refresh can remove
    /// their search row. Columns: FilterKey, OrganizationId, EntityId, Title,
    /// Description, Date, PictureId, LinkId, AccessKey, OwnerUserId, SearchText,
    /// IsSearchable. (OrganizationId, EntityId) must be unique.
    /// </summary>
    public sealed class SearchIndexSource
    {
        public SearchIndexSource(string name, SearchDocumentKind kind, string sql, bool filterKeyIsEntityId = true)
        {
            Name = name;
            Kind = kind;
            Sql = sql;
            FilterKeyIsEntityId = filterKeyIsEntityId;
        }

        public string Name { get; }

        public SearchDocumentKind Kind { get; }

        public string Sql { get; }

        public bool FilterKeyIsEntityId { get; }

        public override string ToString() => Name;
    }
}
