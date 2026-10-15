using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Contracts.Infrastructure;
using Shrooms.DataLayer.EntityModels.Models.VideoLibrary;

namespace Shrooms.Presentation.WebViewModels.Models.VideoLibrary
{
    public class VideoLibraryListArgsViewModel : IPageable, IValidatableObject
    {
        public const int DefaultPageSize = 24;
        public const int MaxPageSize = 100;

        [StringLength(VideoLibraryItem.MaxTitleLength)]
        public string Search { get; set; }

        [Range(1, int.MaxValue)]
        public int? VideoTypeId { get; set; }

        public bool Uncategorised { get; set; }

        public bool Recent { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, MaxPageSize)]
        public int PageSize { get; set; } = DefaultPageSize;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (From?.Date > To?.Date)
            {
                yield return new ValidationResult("From must not be after To", new[] { nameof(From), nameof(To) });
            }
        }
    }
}
