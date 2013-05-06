using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrackerApp.Website.Models;

namespace TrackerApp.Website.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        public ActionResult Get()
        {
            JsonEnvelope<IEnumerable<Task>> result = new JsonEnvelope<IEnumerable<Task>> { Success = false, Data = new List<Task>(), Message = "No tasks" };
            var tasksResponse = SessionHelper.Instance.ProjectManagementClient.GetTasksAllocatedToEmployee(SessionHelper.Instance.Initials, SessionHelper.Instance.ProjectManagementToken);
            if (tasksResponse.ResponseState == TimelogProjectManagement.ExecutionStatus.Success)
            {
                result = new JsonEnvelope<IEnumerable<Task>>
                {
                    Success = true,
                    Data = tasksResponse.Return.Select(t => new Task
                    {
                        Id = t.ID,
                        Name = t.FullName,
                        ProjectId = t.Details.ProjectHeader.ID,
                        ProjectName = t.Details.ProjectHeader.Name
                    }),
                    Message = string.Empty
                };
            }

            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
