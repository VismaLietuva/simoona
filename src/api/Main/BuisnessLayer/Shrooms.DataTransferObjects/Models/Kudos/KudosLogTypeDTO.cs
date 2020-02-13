using Shrooms.Host.Contracts.Enums;

namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class KudosLogTypeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
        public KudosTypeEnum Type { get; set; }
    }
}
