using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using TrackerApp.Website.Models;

namespace TrackerApp.Website.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult SignOn()
        {
            ViewBag.Url = Request.Cookies.Get("TimeTrackr.Settings.Url") != null ? Request.Cookies.Get("TimeTrackr.Settings.Url").Value : "https://app.timelog.dk/local";
            ViewBag.Initials = Request.Cookies.Get("TimeTrackr.Settings.Initials") != null ? Request.Cookies.Get("TimeTrackr.Settings.Initials").Value : string.Empty;

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
                var response = SessionHelper.Instance.SecurityClient.GetToken(initials, password);

                // Did we get a token?
                if (response.ResponseState == TimelogSecurity.ExecutionStatus.Success)
                {
                    // Store the token for later
                    SessionHelper.Instance.SecurityToken = response.Return[0];

                    // Get the name of the user
                    var userResponse = SessionHelper.Instance.SecurityClient.GetUser(response.Return[0]);
                    SessionHelper.Instance.FirstName = userResponse.Return[0].FirstName;
                    SessionHelper.Instance.Url = url;
                    SessionHelper.Instance.Initials = initials;

                    // Store the logon information for next time
                    Response.Cookies.Add(new System.Web.HttpCookie("TimeTrackr.Settings.Url", url));
                    Response.Cookies.Add(new System.Web.HttpCookie("TimeTrackr.Settings.Initials", initials));

                    // Authenticate with the application
                    FormsAuthentication.SetAuthCookie(initials, true);

                    // Go to the dashboard
                    return Redirect("~/");
                }
                else
                {
                    // Loop through the error messages and print them to the user
                    foreach (var item in response.Messages.Where(m => m.ErrorCode > 0))
                    {
                        if (item.ErrorCode == 40001)
                        {
                            ViewData.ModelState.AddModelError("Initials", "Initials or password wrong");
                        }
                        else
                        {
                            ViewData.ModelState.AddModelError("Initials", item.Message);
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                // Url is most likely wrong
                ViewData.ModelState.AddModelError("Initials", "Unable to connect to the service. Please check the URL");
            }

            return View();
        }

        public ActionResult SignOut()
        {
            // Remove the authentication and redirect to login page
            FormsAuthentication.SignOut();
            return Redirect("~/");
        }
    }
}
