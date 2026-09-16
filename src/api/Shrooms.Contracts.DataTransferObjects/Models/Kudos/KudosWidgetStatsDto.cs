using System.Collections.Generic;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    /// <summary>
    /// The two Kudos leaderboards the wall widget shows side by side, fetched in one round trip.
    /// </summary>
    public class KudosWidgetStatsDto
    {
        public IEnumerable<KudosBasicDataDto> TabOne { get; set; }

        public IEnumerable<KudosBasicDataDto> TabTwo { get; set; }
    }
}
