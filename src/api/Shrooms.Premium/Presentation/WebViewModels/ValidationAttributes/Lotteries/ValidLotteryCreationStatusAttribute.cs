using Shrooms.Contracts.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Lotteries
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidLotteryCreationStatusAttribute : ValidationAttribute
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
                return new ValidationResult($"Lottery status {value} is invalid");
            }

            var lotteryStatus = (LotteryStatus)value;

            if (lotteryStatus != LotteryStatus.Started && lotteryStatus != LotteryStatus.Drafted)
            {
                return new ValidationResult($"Lottery status has to be {LotteryStatus.Started} or {LotteryStatus.Drafted}");
            }

            return ValidationResult.Success;
        }
    }
}
