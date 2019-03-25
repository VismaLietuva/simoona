using Shrooms.Constants.BusinessLayer;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class KudosLogTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public ConstBusinessLayer.KudosTypeEnum Type { get; set; }
    }
}
