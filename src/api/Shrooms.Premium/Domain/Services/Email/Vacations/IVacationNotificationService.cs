using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Domain.Services.Email.Vacations
{
    /// <summary>
    /// Tells the people a leave request concerns that it moved. Email always,
    /// in-app unless the recipient switched it off; never the person who caused
    /// the change. Called after the change is saved, so nothing announces a
    /// change that did not happen.
    /// </summary>
    public interface IVacationNotificationService
    {
        Task NotifySubmittedAsync(VacationRequestDto request, UserAndOrganizationDto actor);

        Task NotifyChangedAsync(VacationRequestDto request, UserAndOrganizationDto actor);

        Task NotifyWithdrawnAsync(VacationRequestDto request, UserAndOrganizationDto actor);

        /// <summary>Approved, rejected or cancelled by somebody other than the owner.</summary>
        Task NotifyDecidedAsync(VacationRequestDto request, UserAndOrganizationDto actor);
    }
}
