using Shrooms.Contracts.Enums;

namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosTypeDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Value { get; set; }

        public bool Hidden { get; set; }

        public bool IsNecessary { get; set; }

        public KudosTypeEnum Type { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as KudosTypeDTO);
        }

        public bool Equals(KudosTypeDTO kudosTypeDto)
        {
            if (kudosTypeDto != null && Id == kudosTypeDto.Id)
            {
                return true;
            }

            return false;
        }
    }
}
