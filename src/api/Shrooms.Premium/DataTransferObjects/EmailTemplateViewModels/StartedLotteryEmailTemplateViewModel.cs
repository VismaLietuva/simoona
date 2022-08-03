﻿using Shrooms.Contracts.DataTransferObjects;
using Shrooms.Premium.DataTransferObjects.Models.Lotteries;
using System;

namespace Shrooms.Premium.DataTransferObjects.EmailTemplateViewModels
{
    public class StartedLotteryEmailTemplateViewModel : BaseEmailTemplateViewModel
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string LotteryUrl { get; set; }

        public DateTime EndDate { get; set; }

        public int EntryFee { get; set; }

        public StartedLotteryEmailTemplateViewModel(LotteryStartedEmailDto startedDto, string lotteryUrl, string userNotificationSettingsUrl)
            : base(userNotificationSettingsUrl)
        {
            Title = startedDto.Title;
            EndDate = startedDto.EndDate;
            EntryFee = startedDto.EntryFee;
            Description = startedDto.Description;
            LotteryUrl = lotteryUrl;
        }
    }
}