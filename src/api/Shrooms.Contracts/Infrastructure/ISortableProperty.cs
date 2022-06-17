namespace Shrooms.Contracts.Infrastructure
{
    public interface ISortableProperty
    {
        public string SortByColumnName { get; set; }

        public string SortDirection { get; set; }
    }
}
