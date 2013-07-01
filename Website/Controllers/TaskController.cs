using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrackerApp.Website.Models;

namespace TrackerApp.Website.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        /// <summary>
        /// Gets a list of tasks allocated to the currently authenticated user
        /// </summary>
        /// <returns>A JsonResult with tasks</returns>
        public ActionResult Get()
        {
            // Prepare the envelope with a faulty state
            JsonEnvelope<IEnumerable<Task>> result = new JsonEnvelope<IEnumerable<Task>> { Success = false, Data = new List<Task>(), Message = "No tasks" };

            // Query the TimeLog Project web service for tasks allocated to the employee
            var tasksResponse = SessionHelper.Instance.ProjectManagementClient.GetTasksAllocatedToEmployee(SessionHelper.Instance.Initials, SessionHelper.Instance.ProjectManagementToken);

            // Check if the state is correct
            if (tasksResponse.ResponseState == TimelogProjectManagement.ExecutionStatus.Success)
            {
                // Recreate the result including the task data
                result = new JsonEnvelope<IEnumerable<Task>>
                {
                    Success = true,
                    Data = tasksResponse.Return.Where(t => !t.IsParent).Select(t => new Task
                    {
                        Id = t.ID,
                        Name = t.FullName,
                        ProjectId = t.Details.ProjectHeader.ID,
                        ProjectName = t.Details.ProjectHeader.Name
                    }),
                    Message = string.Empty
                };
            }

            // Return the data as JSON
            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
