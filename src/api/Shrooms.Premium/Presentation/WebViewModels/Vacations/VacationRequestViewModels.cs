using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Vacations;

namespace Shrooms.Premium.Presentation.WebViewModels.Vacations
{
    public abstract class VacationListingViewModel
    {
        public string Search { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// For requests these are period-overlap bounds; for the log they bound
        /// the event's own day.
        /// </summary>
        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public string Sort { get; set; }

        public string Dir { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 20;
    }

    public class VacationRequestListingViewModel : VacationListingViewModel
    {
        public string Status { get; set; }

        public VacationRequestArgsDto ToArgs(UserAndOrganizationDto userOrg)
        {
            return new VacationRequestArgsDto
            {
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId,
                Search = Search,
                Status = Status,
                Type = Type,
                From = From,
                To = To,
                Sort = Sort,
                Dir = Dir,
                Page = Page,
                PageSize = PageSize
            };
        }
    }

    public class VacationLogListingViewModel : VacationListingViewModel
    {
        public string Kind { get; set; }

        public VacationLogArgsDto ToArgs(UserAndOrganizationDto userOrg)
        {
            return new VacationLogArgsDto
            {
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId,
                Search = Search,
                Kind = Kind,
                Type = Type,
                From = From,
                To = To,
                Sort = Sort,
                Dir = Dir,
                Page = Page,
                PageSize = PageSize
            };
        }
    }

    public class VacationStatisticsListingViewModel
    {
        public string Search { get; set; }

        public string Sort { get; set; }

        public string Dir { get; set; }

        public VacationStatisticsArgsDto ToArgs(UserAndOrganizationDto userOrg)
        {
            return new VacationStatisticsArgsDto
            {
                UserId = userOrg.UserId,
                OrganizationId = userOrg.OrganizationId,
                Search = Search,
                Sort = Sort,
                Dir = Dir
            };
        }
    }

    public class VacationRequestDraftViewModel
    {
        public string Type { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string Note { get; set; }

        public VacationRequestDraftDto ToDto()
        {
            return new VacationRequestDraftDto
            {
                Type = Type,
                DateFrom = DateFrom,
                DateTo = DateTo,
                Note = Note
            };
        }
    }

    public class VacationAdminPatchViewModel
    {
        public string Type { get; set; }

        public string Status { get; set; }

        public string DateFrom { get; set; }

        public string DateTo { get; set; }

        public string Note { get; set; }

        public VacationAdminPatchDto ToDto()
        {
            return new VacationAdminPatchDto
            {
                Type = Type,
                Status = Status,
                DateFrom = DateFrom,
                DateTo = DateTo,
                Note = Note
            };
        }
    }

    public class VacationRejectViewModel
    {
        public string Reason { get; set; }
    }

    public class VacationTeamSummaryViewModel
    {
        public bool IsManager { get; set; }

        public int PendingCount { get; set; }
    }

    public class VacationSettingsViewModel
    {
        public string OrderPrefix { get; set; }

        public int OrderStartNumber { get; set; }

        public string OrderLetterhead { get; set; }

        public string OrderCity { get; set; }

        public string OrderSignature { get; set; }

        public VacationSettingsDto ToDto()
        {
            return new VacationSettingsDto
            {
                OrderPrefix = OrderPrefix,
                OrderStartNumber = OrderStartNumber,
                OrderLetterhead = OrderLetterhead,
                OrderCity = OrderCity,
                OrderSignature = OrderSignature
            };
        }
    }

    public class GenerateVacationOrdersViewModel
    {
        /// <summary>The period whose approved leave to cover, as calendar days.</summary>
        public string From { get; set; }

        public string To { get; set; }
    }

}
