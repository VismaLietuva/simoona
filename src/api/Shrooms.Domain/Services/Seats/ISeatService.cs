using System.Collections.Generic;
using System.Threading.Tasks;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Contracts.DataTransferObjects.Models.Seats;

namespace Shrooms.Domain.Services.Seats
{
    public interface ISeatService
    {
        Task<SeatBoardDto> GetBoardAsync(SeatBoardArgsDto args);

        Task<IEnumerable<SeatDto>> GetByRoomAsync(int roomId, UserAndOrganizationDto userOrg);

        Task<SeatBookResultDto> BookAsync(SeatDayArgsDto args);

        Task GoHomeAsync(string day, UserAndOrganizationDto userOrg);

        Task UnreleaseAsync(SeatDayArgsDto args);

        Task<SeatDto> CreateAsync(SeatSaveArgsDto args);

        Task<SeatDto> UpdateAsync(SeatSaveArgsDto args);

        Task DeleteAsync(int id, UserAndOrganizationDto userOrg);
    }
}
