namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IEmailReceiver
    {
        string Email { get; set; }

        string TimeZoneKey { get; set; }
    }
}
