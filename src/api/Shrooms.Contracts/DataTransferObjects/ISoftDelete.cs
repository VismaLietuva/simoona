namespace Shrooms.Contracts.DataTransferObjects
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}