namespace Shrooms.Infrastructure.FireAndForget
{
    public class TenantNameContainer : ITenantNameContainer
    {
        public string TenantName { get; set; }
    }

    public interface ITenantNameContainer
    {
        string TenantName { get; set; }
    }
}
