using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TrackerApp.Website.Models;

namespace TrackerApp.Website.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult SignOn()
        {
            // Fetch data about the previous logon
            var _settingsUrl = this.Request.Cookies.Get("TimeTrackr.Settings.Url");
            if (_settingsUrl != null)
            {
                this.ViewBag.Url = _settingsUrl.Value;
            }

            var _settingsInitials = this.Request.Cookies.Get("TimeTrackr.Settings.Initials");
            if (_settingsInitials != null)
            {
                this.ViewBag.Initials = _settingsInitials.Value;
            }

            return View();
        }

        [HttpPost]
        public ActionResult SignOn(string url, string initials, string password)
        {
            try
            {
                // Store the URL to support multiple TimeLog Project instances
                SessionHelper.Instance.Url = url.Replace("http://", "https://").Trim('/');

                // Fetch the token
                var _response = SessionHelper.Instance.SecurityClient.GetToken(initials, password);

                // Did we get a token?
                if (_response.ResponseState == TimelogSecurity.ExecutionStatus.Success)
                {
                    // Store the token for later
                    SessionHelper.Instance.SecurityToken = _response.Return[0];

                    // Get the name of the user
                    var _userResponse = SessionHelper.Instance.SecurityClient.GetUser(_response.Return[0]);
                    SessionHelper.Instance.FirstName = _userResponse.Return[0].FirstName;
                    SessionHelper.Instance.Url = url;
                    SessionHelper.Instance.Initials = initials;
                    SessionHelper.Instance.Authenticate(initials);

                    // Authenticate with the application
                    return RedirectToAction("Index", "Dashboard");
                }
                else
                {
                    // Loop through the error messages and print them to the user
                    foreach (var _item in _response.Messages.Where(m => m.ErrorCode > 0))
                    {
                        this.ViewData.ModelState.AddModelError(
                            "Initials",
                            _item.ErrorCode == 40001 ? "Initials or password wrong" : _item.Message);
                    }                    
                }
            }
            catch (Exception)
            {
                // Url is most likely wrong
                ViewData.ModelState.AddModelError("Initials", "Unable to connect to the service. Please check the URL");
            }

            return this.SignOn();
        }

        public ActionResult SignOut()
        {
            // Remove the authentication and redirect to login page
            SessionHelper.Instance.Deauthenticate();
            return Redirect("~/");
        }
    }
}
