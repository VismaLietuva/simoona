namespace Shrooms.Premium.DataTransferObjects.Models.TimeRegistration
{
    public class TimeTeamMemberDto : TimeEmployeeDto
    {
        public string Status { get; set; }

        public int DailyMinutes { get; set; }
    }
}
