namespace Shrooms.Contracts.DataTransferObjects.Models.Kudos
{
    public class KudosLogInputDto
    {
        public string Id { get; set; }
        public KudosTypeDto PointsType { get; set; }
        public int MultipleBy { get; set; }
        public string Comment { get; set; }
    }
}
