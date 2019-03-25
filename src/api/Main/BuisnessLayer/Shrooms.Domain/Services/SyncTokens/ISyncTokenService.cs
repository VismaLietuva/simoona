namespace Shrooms.Domain.Services.SyncTokens
{
    public interface ISyncTokenService
    {
        string GetToken(string name);
        string Update(string name, string syncToken);
        string Create(string name, string syncToken = "");
    }
}
