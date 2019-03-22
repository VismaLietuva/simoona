namespace Shrooms.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public interface ILoyaltyKudosService
    {
        void AwardEmployeesWithKudos(string organizationName);
    }
}
