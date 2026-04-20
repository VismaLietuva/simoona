using Shrooms.Contracts.DataTransferObjects;

namespace Shrooms.DataLayer.EntityModels.Models
{
    public class SoftDeletableModel : BaseModel, ISoftDelete
    {
        public bool IsDeleted { get; set; }
    }
}
