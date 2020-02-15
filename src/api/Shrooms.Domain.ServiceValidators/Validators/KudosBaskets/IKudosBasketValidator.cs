using Shrooms.Contracts.DataTransferObjects.Models.KudosBasket;
using Shrooms.DataLayer.EntityModels.Models;

namespace Shrooms.Domain.ServiceValidators.Validators.KudosBaskets
{
    public interface IKudosBasketValidator
    {
        void CheckIfBasketAlreadyExists(bool basketIsActive);
        void CheckIfThereIsNoBasketYet(KudosBasketDTO basket);
        void CheckIfUserHasEnoughKudos(decimal remainingKudos, decimal donationAmount);
        void CheckIfBasketIsActive(KudosBasket basket);
    }
}
