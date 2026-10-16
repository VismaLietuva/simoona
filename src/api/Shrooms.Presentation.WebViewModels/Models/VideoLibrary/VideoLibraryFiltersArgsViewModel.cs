using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoLibraryFiltersArgsViewModel : IValidatableObject
    {
        [StringLength(VideoLibraryItem.MaxTitleLength)]
        public string Search { get; set; }

        public DateOnly? From { get; set; }

        public DateOnly? To { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From > To)
            {
                yield return new ValidationResult("From must not be after To", new[] { nameof(From), nameof(To) });
            }
        }
    }
}
