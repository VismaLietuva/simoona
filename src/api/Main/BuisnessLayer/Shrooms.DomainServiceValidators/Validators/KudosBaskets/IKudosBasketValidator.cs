using Shrooms.DataTransferObjects.Models.KudosBasket;
using Shrooms.EntityModels.Models;

namespace DomainServiceValidators.Validators.KudosBaskets
{
    public interface IKudosBasketValidator
    {
        void CheckIfBasketAlreadyExists(bool basketIsActive);
        void CheckIfThereIsNoBasketYet(KudosBasketDTO basket);
        void CheckIfUserHasEnoughKudos(decimal remainingKudos, decimal donationAmount);
        void CheckIfBasketIsActive(KudosBasket basket);
    }
}
