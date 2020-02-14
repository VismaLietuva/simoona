using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosLogTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public KudosTypeEnum Type { get; set; }
    }
}
