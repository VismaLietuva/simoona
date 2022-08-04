using Shrooms.Contracts.Enums;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Lotteries
{
    public abstract class ValidLotteryStatusBaseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (value is not LotteryStatus)
            {
                throw new ArgumentException($"The property must be of type {typeof(LotteryStatus)}");
            }

            if (!Enum.IsDefined(typeof(LotteryStatus), value))
            {
                throw new LotteryException($"Lottery status {value} is invalid");
            }

            return ValidationResult.Success;
        }
    }
}
