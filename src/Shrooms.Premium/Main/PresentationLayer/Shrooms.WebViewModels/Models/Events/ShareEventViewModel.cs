﻿using Shrooms.Constants.EntityValidationValues;
using System.ComponentModel.DataAnnotations;

namespace Shrooms.WebViewModels.Models.Events
{
    public class ShareEventViewModel
    {
        [Required]

        public string MessageBody { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int WallId { get; set; }

        [Required]
        public string EventId { get; set; }
    }
}