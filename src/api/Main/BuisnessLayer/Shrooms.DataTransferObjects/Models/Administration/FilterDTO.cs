using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.DataTransferObjects.Models.Administration
{
    public class FilterDTO
    {
        /// <summary>
        /// Filter field
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Allowed field values (equal)
        /// </summary>
        public string[] Values { get; set; }
    }
}
