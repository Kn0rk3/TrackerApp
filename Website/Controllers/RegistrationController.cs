using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrackerApp.Website.Models;
using TrackerApp.Website.TimelogProjectManagement;

namespace TrackerApp.Website.Controllers
{
    using System.Web.Security;

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
            var _result = new JsonEnvelope<bool> { Success = false, Data = false, Message = "No insert" };

            string _unique = string.Format("{0}{1}{2}{3}", date, hours, message, taskId);

            if (Session["LastRegistration"] != null && _unique == Session["LastRegistration"].ToString())
            {
                // Dublicate
                _result = new JsonEnvelope<bool>
                {
                    Success = false,
                    Message = string.Empty
                };

                return new JsonResult { Data = _result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }

            Session["LastRegistration"] = _unique;

            // Construct the work unit object
            var _unit = new WorkUnit();
            _unit.TaskID = taskId;
            _unit.TaskIDSpecified = true;
            _unit.StartDateTime = date.Value;
            _unit.StartDateTimeSpecified = true;
            _unit.GUID = Guid.NewGuid().ToString();
            _unit.EmployeeInitials = SessionHelper.Instance.Initials;
            _unit.Description = message;
            _unit.EndDateTime = date.Value.AddHours(hours);
            _unit.EndDateTimeSpecified = true;
            _unit.Duration = System.Xml.XmlConvert.ToString(TimeSpan.FromHours(hours)); // Do necessary convertion to fit a TimeSpan object

            // Execute the insert request
            var _response = SessionHelper.Instance.ProjectManagementClient.InsertWork(new[] { _unit }, 0, SessionHelper.Instance.ProjectManagementToken);

            // Check if the response was correct
            if (_response.ResponseState == ExecutionStatus.Success)
            {
                // Yes, recreate the envelope
                _result = new JsonEnvelope<bool>
                {
                    Success = true,
                    Message = string.Empty
                };
            }
            else if (_response.ErrorCode == 20003)
            {
                // Token not valid anymore.
                FormsAuthentication.SignOut();
            }
            else
            {
                // No, take the first error message
                _result.Message = _response.Messages.FirstOrDefault().Message;
                _result.Success = false;
            }

            // Return the data as JSON
            return new JsonResult { Data = _result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        /// <summary>
        /// Deletes a time registration
        /// </summary>
        /// <param name="registrationId"></param>
        /// <returns></returns>
        public ActionResult Delete(string registrationId)
        {
            // Prepare the envelope with a faulty state
            var _result = new JsonEnvelope<bool> { Success = false, Data = false, Message = "No delete" };

            // Execute the delete request
            var _response = SessionHelper.Instance.ProjectManagementClient.DeleteWork(new[] { registrationId }, 0, SessionHelper.Instance.ProjectManagementToken);

            // Check if the response was correct
            if (_response.ResponseState == ExecutionStatus.Success)
            {
                // Yes, recreate the envelope
                _result = new JsonEnvelope<bool>
                {
                    Success = true,
                    Message = string.Empty
                };
            }
            else if (_response.ErrorCode == 20003)
            {
                // Token not valid anymore.
                FormsAuthentication.SignOut();
            }
            else
            {
                // No, take the first error message
                _result.Message = _response.Messages.FirstOrDefault().Message;
                _result.Success = false;
            }

            // Return the data as JSON
            return new JsonResult { Data = _result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
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
            var _startDate = new DateTime(start.Value.Year, start.Value.Month, start.Value.Day, 0, 0, 0);
            var _endDate = new DateTime(end.Value.Year, end.Value.Month, end.Value.Day, 0, 0, 0);

            // Create the JSON response envelope, initialize with unsuccessful envelope
            var _result = new JsonEnvelope<IEnumerable<Registration>> { Success = false, Data = new List<Registration>(), Message = "No registrations" };

            // Query the TimeLog Project web service for work units
            var _response = SessionHelper.Instance.ProjectManagementClient.GetEmployeeWork(SessionHelper.Instance.Initials, _startDate, _endDate, SessionHelper.Instance.ProjectManagementToken);

            // Check if the response was correct
            if (_response.ResponseState == ExecutionStatus.Success)
            {
                // Recreate the envelope including the work units from the service
                _result = new JsonEnvelope<IEnumerable<Registration>>
                {
                    Success = true,
                    Data = _response.Return.Select(t => new Registration
                    {
                        Id = t.GUID,
                        Date = t.StartDateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds, // Convert to JavaScript date
                        Hours = t.EndDateTime.Subtract(t.StartDateTime).TotalHours,
                        TaskId = t.Details.TaskHeader.ID,
                        TaskName = t.Details.TaskHeader.FullName,
                        ProjectId = t.Details.ProjectHeader.ID,
                        ProjectName = t.Details.ProjectHeader.Name,
                        Comment = t.Description
                    }),
                    Message = string.Empty
                };
            }
            else if (_response.ErrorCode == 20003)
            {
                // Token not valid anymore.
                FormsAuthentication.SignOut();
            }

            // Return the data as JSON
            return new JsonResult { Data = _result, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
    }
}
