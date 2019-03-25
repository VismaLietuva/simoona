using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.EntityModels.Models
{
    public class ExternalLink : BaseModelWithOrg
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
