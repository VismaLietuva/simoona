namespace Shrooms.Premium.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public interface ILoyaltyKudosService
    {
        void AwardEmployeesWithKudos(string organizationName);
    }
}