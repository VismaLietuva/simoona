namespace Shrooms.DataTransferObjects.Models.Kudos
{
    public class KudosLogInputDTO
    {
        public string Id { get; set; }
        public KudosTypeDTO PointsType { get; set; }
        public int MultipleBy { get; set; }
        public string Comment { get; set; }
    }
}
