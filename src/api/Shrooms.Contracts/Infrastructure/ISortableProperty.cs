namespace Shrooms.Contracts.Infrastructure
{
    public interface ISortableProperty
    {
        string SortByColumnName { get; set; }

        string SortDirection { get; set; }
    }
}
