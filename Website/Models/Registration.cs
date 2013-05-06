
namespace TrackerApp.Website.Models
{
    public class Registration
    {
        public string Id { get; set; }

        public string ProjectName { get; set; }

        public string TaskName { get; set; }

        public int ProjectId { get; set; }

        public int TaskId { get; set; }

        public double Hours { get; set; }

        public double Date { get; set; }
    }
}