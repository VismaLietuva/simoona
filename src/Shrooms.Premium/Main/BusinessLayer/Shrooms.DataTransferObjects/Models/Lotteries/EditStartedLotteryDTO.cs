﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Lotteries
{
    public class EditStartedLotteryDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        public string Description { get; set; }
 
    }
}
