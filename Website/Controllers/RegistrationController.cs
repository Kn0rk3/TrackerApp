using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrackerApp.Website.Models;
using TrackerApp.Website.TimelogProjectManagement;

namespace TrackerApp.Website.Controllers
{
    [Authorize]
    public class RegistrationController : Controller
    {
        public ActionResult Insert(DateTime? date, double hours, string message, int taskId)
        {
            JsonEnvelope<bool> result = new JsonEnvelope<bool> { Success = false, Data = false, Message = "No insert" };

            var unit = new WorkUnit();
            unit.TaskID = taskId;
            unit.TaskIDSpecified = true;
            unit.StartDateTime = date.Value;
            unit.StartDateTimeSpecified = true;
            unit.GUID = Guid.NewGuid().ToString();
            unit.EmployeeInitials = SessionHelper.Instance.Initials;
            unit.Description = message;
            unit.EndDateTime = date.Value.AddHours(hours);
            unit.EndDateTimeSpecified = true;
            unit.Duration = System.Xml.XmlConvert.ToString(TimeSpan.FromHours(hours));
            var response = SessionHelper.Instance.ProjectManagementClient.InsertWork(new[] { unit }, 99, SessionHelper.Instance.ProjectManagementToken);

            if (response.ResponseState == TimelogProjectManagement.ExecutionStatus.Success)
            {
                result = new JsonEnvelope<bool>
                {
                    Success = true,
                    Message = string.Empty
                };
            }
            else
            {
                result.Message = response.Messages.FirstOrDefault().Message;
                return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Get(DateTime? start, DateTime? end)
        {
            if (!start.HasValue)
            {
                start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }

            if (!end.HasValue)
            {
                end = start.Value;
            }

            var startDate = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
            var endDate = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 0, 0, 0);

            JsonEnvelope<IEnumerable<Registration>> result = new JsonEnvelope<IEnumerable<Registration>> { Success = false, Data = new List<Registration>(), Message = "No registrations" };
            var response = SessionHelper.Instance.ProjectManagementClient.GetEmployeeWork(SessionHelper.Instance.Initials, startDate, endDate, SessionHelper.Instance.ProjectManagementToken);
            if (response.ResponseState == TimelogProjectManagement.ExecutionStatus.Success)
            {
                result = new JsonEnvelope<IEnumerable<Registration>>
                {
                    Success = true,
                    Data = response.Return.Select(t => new Registration
                    {
                        Id = t.GUID,
                        Date = t.StartDateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds,
                        Hours = t.EndDateTime.Subtract(t.StartDateTime).TotalHours,
                        TaskId = t.Details.TaskHeader.ID,
                        TaskName = t.Details.TaskHeader.FullName,
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
