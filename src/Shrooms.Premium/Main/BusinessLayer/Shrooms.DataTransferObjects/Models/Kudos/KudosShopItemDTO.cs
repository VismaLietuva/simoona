namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class KudosShopItemDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public string Description { get; set; }

        public string PictureId { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as KudosShopItemDTO);
        }

        public bool Equals(KudosShopItemDTO kudosShopItemDto)
        {
            if (kudosShopItemDto != null &&
                Id == kudosShopItemDto.Id)
            {
                return true;
            }

            return false;
        }
    }
}
