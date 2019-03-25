using System;
using Shrooms.DataTransferObjects.Models.KudosBasket;
using Shrooms.DomainExceptions.Exceptions.KudosBaskets;
using Shrooms.EntityModels.Models;

namespace DomainServiceValidators.Validators.KudosBaskets
{
    public class KudosBasketValidator : IKudosBasketValidator
    {
        public void CheckIfThereIsNoBasketYet(KudosBasketDTO basket)
        {
            if (basket == null)
            {
                throw new KudosBasketException(Shrooms.Resources.Widgets.KudosBasket.KudosBasket.KudosBasketNotExists);
            }
        }

        public void CheckIfUserHasEnoughKudos(decimal remainingKudos, decimal donationAmount)
        {
            if (remainingKudos < donationAmount)
            {
                throw new KudosBasketException(Shrooms.Resources.Widgets.KudosBasket.KudosBasket.KudosBasketNotEnoughKudos);
            }
        }

        public void CheckIfBasketIsActive(KudosBasket basket)
        {
            if (!basket.IsActive)
            {
                throw new KudosBasketException(Shrooms.Resources.Widgets.KudosBasket.KudosBasket.KudosBasketInactive);
            }
        }

        public void CheckIfBasketAlreadyExists(bool basketExists)
        {
            if (basketExists)
            {
                throw new Exception("Kudos Basket already exists");
            }
        }
    }
}
