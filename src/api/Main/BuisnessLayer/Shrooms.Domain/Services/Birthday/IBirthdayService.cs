using System;
using System.Collections.Generic;
using Shrooms.DataTransferObjects.Models.Birthdays;

namespace Shrooms.Domain.Services.Birthday
{
    public interface IBirthdayService
    {
        IEnumerable<BirthdayDTO> GetWeeklyBirthdays(DateTime date);
    }
}