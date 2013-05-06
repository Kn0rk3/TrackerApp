
using System.ServiceModel;
using System.Web;
using System.Web.Security;
namespace TrackerApp.Website.Models
{
    public class SessionHelper
    {
        private static SessionHelper instance;
        private TimelogProjectManagement.ProjectManagementServiceClient projectManagementClient;
        private TimelogSecurity.SecurityServiceClient securityClient;

        public static SessionHelper Instance
        {
            get
            {
                return instance ?? (instance = new SessionHelper());
            }
        }

        private SessionHelper()
        {
        }

        public string Initials
        {
            get
            {
                return HttpContext.Current.Session["TimeTrackr.User.Initials"].ToString();
            }
            set
            {
                HttpContext.Current.Session["TimeTrackr.User.Initials"] = value;
            }
        }

        public string Url
        {
            get
            {
                var value = HttpContext.Current.Session["TimeTrackr.User.Url"].ToString();
                if (value == null)
                {
                    // Log off
                    FormsAuthentication.SignOut();
                    HttpContext.Current.Response.Redirect("~/");
                    return string.Empty;
                }

                return value.ToString();
            }
            set
            {
                HttpContext.Current.Session["TimeTrackr.User.Url"] = value;
            }
        }

        public string FirstName
        {
            get
            {
                var value = HttpContext.Current.Session["TimeTrackr.User.FirstName"];
                if (value == null)
                {
                    // Log off
                    FormsAuthentication.SignOut();
                    HttpContext.Current.Response.Redirect("~/");
                    return string.Empty;
                }

                return value.ToString();
            }
            set
            {
                HttpContext.Current.Session["TimeTrackr.User.FirstName"] = value;
            }
        }

        public TimelogSecurity.SecurityToken SecurityToken
        {
            get
            {
                return (TimelogSecurity.SecurityToken)HttpContext.Current.Session["TimeTrackr.User.SecurityToken"];
            }
            set
            {
                HttpContext.Current.Session["TimeTrackr.User.SecurityToken"] = value;
            }
        }

        public TimelogProjectManagement.SecurityToken ProjectManagementToken
        {
            get
            {
                return new TimelogProjectManagement.SecurityToken
                {
                    Expires = SecurityToken.Expires,
                    Hash = SecurityToken.Hash,
                    Initials = SecurityToken.Initials
                };
            }
        }

        public TimelogProjectManagement.ProjectManagementServiceClient ProjectManagementClient
        {
            get
            {
                if (projectManagementClient == null)
                {
                    var binding = new BasicHttpsBinding() { MaxReceivedMessageSize = 1024000 };
                    var endpoint = new EndpointAddress(Url + "/WebServices/ProjectManagement/V1_3/ProjectManagementServiceSecure.svc");
                    projectManagementClient = new TimelogProjectManagement.ProjectManagementServiceClient(binding, endpoint);
                }

                return projectManagementClient;
            }
        }

        public TimelogSecurity.SecurityServiceClient SecurityClient
        {
            get
            {
                if (securityClient == null)
                {
                    var binding = new BasicHttpsBinding() { MaxReceivedMessageSize = 1024000 };
                    var endpoint = new EndpointAddress(Url + "/WebServices/Security/V1_2/SecurityServiceSecure.svc");
                    securityClient = new TimelogSecurity.SecurityServiceClient(binding, endpoint);
                }

                return securityClient;
            }
        }
    }
}