namespace Shrooms.Premium.Main.BusinessLayer.Domain.Services.WebHookCallbacks.LoyaltyKudos
{
    public interface ILoyaltyKudosService
    {
        void AwardEmployeesWithKudos(string organizationName);
    }
}