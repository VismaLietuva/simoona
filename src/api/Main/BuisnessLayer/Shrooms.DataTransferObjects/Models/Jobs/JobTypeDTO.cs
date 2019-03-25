namespace Shrooms.DataTransferObjects.Models.Jobs
{
    public class JobTypeDTO : UserAndOrganizationDTO
    {
        public int Id { get; set; }
        
        public string Title { get; set; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JobTypeDTO);
        }

        public bool Equals(JobTypeDTO jobTypeDto)
        {
            if (jobTypeDto != null &&
                Id == jobTypeDto.Id)
            {
                return true;
            }

            return false;
        }
    }
}
