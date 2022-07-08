namespace Shrooms.Contracts.Infrastructure
{
    public interface IPageable
    {
        int Page { get; set; }

        int PageSize { get; set; }
    }
}
