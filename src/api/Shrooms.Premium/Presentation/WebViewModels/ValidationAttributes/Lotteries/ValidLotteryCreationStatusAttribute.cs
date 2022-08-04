using Shrooms.Contracts.Enums;
using Shrooms.Premium.Domain.DomainExceptions.Lotteries;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.Premium.Presentation.WebViewModels.ValidationAttributes.Lotteries
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidLotteryCreationStatusAttribute : ValidLotteryStatusBaseAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var result = base.IsValid(value, validationContext);

            if (result != ValidationResult.Success)
            {
                return result;
            }

            var lotteryStatus = (LotteryStatus)value;

            if (lotteryStatus != LotteryStatus.Started && lotteryStatus != LotteryStatus.Drafted)
            {
                throw new LotteryException($"Lottery status has to be {LotteryStatus.Started} or {LotteryStatus.Drafted}");
            }

            return ValidationResult.Success;
        }
    }
}
