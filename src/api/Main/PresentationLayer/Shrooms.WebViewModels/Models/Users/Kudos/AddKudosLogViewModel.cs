using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shrooms.Constants.EntityValidationValues;

namespace Shrooms.WebViewModels.Models.Kudos
{
    public class AddKudosLogViewModel
    {
        [Required]
        public IEnumerable<string> ReceivingUserIds { get; set; }

        [Required]
        public int PointsTypeId { get; set; }

        [Required]
        [Range(ValidationConstants.KudosMultiplyByMinValue, ValidationConstants.KudosMultiplyByMaxValue)]
        public int MultiplyBy { get; set; }
        
        /// <summary>
        /// Explicit number of points each receiver should get. If null, amount will be calculated.
        /// </summary>
        public long? TotalPointsPerReceiver { get; set; }

        [Required]
        [MaxLength(ValidationConstants.KudosCommentMaxLength)]
        public string Comment { get; set; }

        public string PictureId { get; set; }
    }
}
