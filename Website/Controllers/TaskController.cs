using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrackerApp.Website.Models;

namespace TrackerApp.Website.Controllers
{
    using System.Web.Security;

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
            var _result = new JsonEnvelope<IEnumerable<Task>> { Success = false, Data = new List<Task>(), Message = "No tasks" };

            // Query the TimeLog Project web service for tasks allocated to the employee
            var _tasksResponse = SessionHelper.Instance.ProjectManagementClient.GetTasksAllocatedToEmployee(SessionHelper.Instance.Initials, SessionHelper.Instance.ProjectManagementToken);

            // Check if the state is correct
            if (_tasksResponse.ResponseState == TimelogProjectManagement.ExecutionStatus.Success)
            {
                // Recreate the result including the task data
                _result = new JsonEnvelope<IEnumerable<Task>>
                {
                    Success = true,
                    Data = _tasksResponse.Return.Where(t => !t.IsParent).Select(t => new Task
                    {
                        Id = t.ID,
                        Name = t.FullName,
                        ProjectId = t.Details.ProjectHeader.ID,
                        ProjectName = t.Details.ProjectHeader.Name
                    }),
                    Message = string.Empty
                };
            }
            else if (_tasksResponse.ErrorCode == 20004 || _tasksResponse.ErrorCode == 20003)
            {
                // Token not valid anymore.
                FormsAuthentication.SignOut();
                return new JsonResult { Data = new { Error = true, _tasksResponse.ErrorCode, _tasksResponse.Messages.FirstOrDefault().Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
            else
            {
                return new JsonResult { Data = new { Error = true, _tasksResponse.ErrorCode, _tasksResponse.Messages.FirstOrDefault().Message }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            // Return the data as JSON
            return new JsonResult { Data = _result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
