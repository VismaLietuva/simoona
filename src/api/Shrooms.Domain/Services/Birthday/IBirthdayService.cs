using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects.Models.Birthdays;

namespace Shrooms.Domain.Services.Birthday
{
    public interface IBirthdayService
    {
        Task<IEnumerable<BirthdayDTO>> GetWeeklyBirthdaysAsync(DateTime date);
    }
}