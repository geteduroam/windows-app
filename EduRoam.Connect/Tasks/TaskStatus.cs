namespace EduRoam.Connect.Tasks
{
    public class TaskStatus
    {
        public static TaskStatus AsSuccess(params string[] messages)
        {
            var status = new TaskStatus(true)
            {
                Messages = messages.ToList()
            };

            return status;
        }

        public static TaskStatus AsFailure(params string[] errors)
        {
            var status = new TaskStatus(false)
            {
                Errors = errors.ToList()
            };

            return status;

        }

        public TaskStatus() : this(false)
        {
        }

        public TaskStatus(bool success)
        {
            this.Success = success;
            this.Warnings = new List<string>();
            this.Errors = new List<string>();
            this.Messages = new List<string>();
        }

        public bool Success { get; set; }

        public List<string> Warnings { get; set; }

        public List<string> Errors { get; set; }

        public List<string> Messages { get; set; }
    }
}
