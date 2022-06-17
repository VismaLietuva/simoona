namespace Shrooms.Contracts.Infrastructure
{
    public interface IPageable
    {
        public int Page { get; set; }

        public int PageSize { get; set; }
    }
}
