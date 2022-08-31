﻿using Shrooms.Contracts.DataTransferObjects.EmailTemplateDtos;
using System;

namespace Shrooms.Contracts.DataTransferObjects.EmailTemplateViewModels
{
    public class StartedLotteryEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string LotteryUrl { get; set; }

        public DateTime ZonedEndDate { get; set; }

        public int EntryFee { get; set; }

        public StartedLotteryEmailTemplateViewModel(LotteryStartedEmailDto startedDto, string lotteryUrl, DateTime zonedEndDate, string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
            Title = startedDto.Title;
            EntryFee = startedDto.EntryFee;
            Description = startedDto.Description;
            LotteryUrl = lotteryUrl;
            ZonedEndDate = zonedEndDate;
        }
    }
}