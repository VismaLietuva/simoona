using System;
using System.Collections.Generic;
using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.Premium.DataTransferObjects.Models.Events
{
    public class EventChangeOptionsDto : UserAndOrganizationDto
    {
        public Guid EventId { get; set; }

        /// <summary>
        /// Legacy flat options. Always replaces the participant's flat selection.
        /// </summary>
        public IEnumerable<int> ChosenOptions { get; set; }

        /// <summary>
        /// Question-owned option ids. Null means the caller is not touching answers: the stored
        /// ones are kept and no answer rule is enforced. An empty list clears them. Assigned by the
        /// controller rather than AutoMapper, which maps an omitted array to an empty one and so
        /// cannot tell "omitted" from "clear my answers".
        /// </summary>
        public IEnumerable<int> Answers { get; set; }
    }
}