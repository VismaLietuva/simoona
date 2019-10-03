using Shrooms.Constants.EntityValidationValues;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.EntityModels.Models.Lottery
{
    public class Lottery : ImageBaseModel
    {
        public string Title { get; set; }

        [StringLength(ValidationConstants.MaxPostMessageBodyLength)]
        public string Description { get; set; }

        public DateTime EndDate { get; set; }

        public int Status { get; set; }

        public int EntryFee { get; set; }


    }
}
