using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shrooms.EntityModels.Models.Lotteries
{
    public abstract class ImageBaseModel : BaseModelWithOrg
    {
        public virtual ImagesCollection Images { get; set; }
    }
}
