namespace Shrooms.Contracts.Infrastructure.Email
{
    public interface IEmailReceiver
    {
        public string Email { get; set; }

        public string TimeZoneKey { get; set; }
    }
}
