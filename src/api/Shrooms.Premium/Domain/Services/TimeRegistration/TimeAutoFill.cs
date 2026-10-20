namespace Shrooms.Premium.Domain.Services.TimeRegistration
{
    public class TimeAutoFill
    {
        public TimeAutoFill(int projectId, int taskId, string customerName, string projectName, string taskName)
        {
            ProjectId = projectId;
            TaskId = taskId;
            CustomerName = customerName;
            ProjectName = projectName;
            TaskName = taskName;
        }

        public int ProjectId { get; }

        public int TaskId { get; }

        public string CustomerName { get; }

        public string ProjectName { get; }

        public string TaskName { get; }
    }
}
