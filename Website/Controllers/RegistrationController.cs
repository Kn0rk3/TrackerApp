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
        /// <summary>
        /// Inserts a new work unit
        /// </summary>
        /// <param name="date">Date of work</param>
        /// <param name="hours">Number of hours</param>
        /// <param name="message">Message of the registration</param>
        /// <param name="taskId">Task identifier to add to</param>
        /// <returns>A JsonResult including the inserted work unit</returns>
        public ActionResult Insert(DateTime? date, double hours, string message, int taskId)
        {
            // Prepare the envelope with a faulty state
            JsonEnvelope<bool> result = new JsonEnvelope<bool> { Success = false, Data = false, Message = "No insert" };

            // Construct the work unit object
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
            unit.Duration = System.Xml.XmlConvert.ToString(TimeSpan.FromHours(hours)); // Do necessary convertion to fit a TimeSpan object

            // Execute the insert request
            var response = SessionHelper.Instance.ProjectManagementClient.InsertWork(new[] { unit }, 0, SessionHelper.Instance.ProjectManagementToken);

            // Check if the response was correct
            if (response.ResponseState == TimelogProjectManagement.ExecutionStatus.Success)
            {
                // Yes, recreate the envelope
                result = new JsonEnvelope<bool>
                {
                    Success = true,
                    Message = string.Empty
                };
            }
            else
            {
                // No, take the first error message
                result.Message = response.Messages.FirstOrDefault().Message;
                result.Success = false;
            }

            // Return the data as JSON
            return new JsonResult { Data = result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /// <summary>
        /// Gets a list of registrations based on the currently authenticated user
        /// </summary>
        /// <param name="start">Period start date (optional)</param>
        /// <param name="end">Period end date (optional)</param>
        /// <returns>A JsonResult including the work units</returns>
        public ActionResult Get(DateTime? start, DateTime? end)
        {
            // Check if a start date was supplied, otherwise choose today as start
            if (!start.HasValue)
            {
                start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }

            // Check if end date was supplied, otherwise use the same as start - to only get one days of registrations back
            if (!end.HasValue)
            {
                end = start.Value;
            }

            // Reset possible time stamps
            var startDate = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
            var endDate = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 0, 0, 0);

            // Create the JSON response envelope, initialize with unsuccessful envelope
            JsonEnvelope<IEnumerable<Registration>> result = new JsonEnvelope<IEnumerable<Registration>> { Success = false, Data = new List<Registration>(), Message = "No registrations" };

            // Query the TimeLog Project web service for work units
            var response = SessionHelper.Instance.ProjectManagementClient.GetEmployeeWork(SessionHelper.Instance.Initials, startDate, endDate, SessionHelper.Instance.ProjectManagementToken);

            // Check if the response was correct
            if (response.ResponseState == TimelogProjectManagement.ExecutionStatus.Success)
            {
                // Recreate the envelope including the work units from the service
                result = new JsonEnvelope<IEnumerable<Registration>>
                {
                    Success = true,
                    Data = response.Return.Select(t => new Registration
                    {
                        Id = t.GUID,
                        Date = t.StartDateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds, // Convert to JavaScript date
                        Hours = t.EndDateTime.Subtract(t.StartDateTime).TotalHours,
                        TaskId = t.Details.TaskHeader.ID,
                        TaskName = t.Details.TaskHeader.FullName,
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
