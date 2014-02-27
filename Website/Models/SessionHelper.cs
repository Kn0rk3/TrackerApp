
using System.ServiceModel;
using System.Web;
using System.Web.Security;
namespace TrackerApp.Website.Models
{
    using System;

    using TrackerApp.Website.TimelogSecurity;

    public class SessionHelper
    {
        private static int CookieDaysTimeout = 90;

        public static string SessionInitials = "TimeTrackr.User.Initials";
        public static string SessionUrl = "TimeTrackr.User.Url";
        public static string SessionFirstName = "TimeTrackr.User.FirstName";
        public static string SessionSecurityTokenHash = "TimeTrackr.User.SecurityToken.Hash";
        public static string SessionSecurityTokenInitials = "TimeTrackr.User.SecurityToken.Initials";
        public static string SessionSecurityTokenExpires = "TimeTrackr.User.SecurityToken.Expires";

        private static SessionHelper instance;
        private TimelogProjectManagement.ProjectManagementServiceClient projectManagementClient;
        private SecurityServiceClient securityClient;

        private SecurityToken securityToken;

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
                var _value = HttpContext.Current.Request.Cookies[SessionInitials];
                if (_value == null)
                {
                    // Log off
                    FormsAuthentication.SignOut();
                    HttpContext.Current.Response.Redirect("~/");
                    return string.Empty;
                }

                return _value.Value;
            }
            set
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionInitials, value) { Expires = DateTime.Now.AddDays(CookieDaysTimeout) });
            }
        }

        public string Url
        {
            get
            {
                var _value = HttpContext.Current.Request.Cookies[SessionUrl];
                if (_value == null)
                {
                    // Log off
                    FormsAuthentication.SignOut();
                    HttpContext.Current.Response.Redirect("~/");
                    return string.Empty;
                }

                return _value.Value;
            }
            set
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionUrl, value.Replace("http://", "https://")) { Expires = DateTime.Now.AddDays(CookieDaysTimeout) });
            }
        }

        public string FirstName
        {
            get
            {
                var _value = HttpContext.Current.Request.Cookies[SessionFirstName];
                if (_value == null)
                {
                    return "N/A";
                }

                return HttpUtility.UrlDecode(_value.Value);
            }
            set
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionFirstName, HttpUtility.UrlEncode(value)) { Expires = DateTime.Now.AddDays(CookieDaysTimeout) });
            }
        }

        public SecurityToken SecurityToken
        {
            get
            {
                if (securityToken == null)
                {
                    var _tokenExpires = HttpContext.Current.Request.Cookies[SessionSecurityTokenExpires];
                    var _tokenHash = HttpContext.Current.Request.Cookies[SessionSecurityTokenHash];
                    var _tokenInitials = HttpContext.Current.Request.Cookies[SessionSecurityTokenInitials];

                    if (_tokenExpires == null || _tokenHash == null || _tokenInitials == null)
                    {
                        // Log off
                        FormsAuthentication.SignOut();
                        HttpContext.Current.Response.Redirect("~/");
                        return null;
                    }

                    securityToken = new SecurityToken
                                    {
                                        Expires = Convert.ToDateTime(_tokenExpires.Value),
                                        Hash = _tokenHash.Value,
                                        Initials = _tokenInitials.Value
                                    };
                }

                return securityToken;
            }
            set
            {
                securityToken = value;
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionSecurityTokenExpires, value.Expires.ToString("r")) { Expires = DateTime.Now.AddDays(CookieDaysTimeout) });
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionSecurityTokenHash, value.Hash) { Expires = DateTime.Now.AddDays(CookieDaysTimeout) });
                HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionSecurityTokenInitials, value.Initials) { Expires = DateTime.Now.AddDays(CookieDaysTimeout) });
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
                    var _binding = new BasicHttpsBinding { MaxReceivedMessageSize = 1024000 };
                    var _endpoint = new EndpointAddress(Url + "/WebServices/ProjectManagement/V1_3/ProjectManagementServiceSecure.svc");
                    projectManagementClient = new TimelogProjectManagement.ProjectManagementServiceClient(_binding, _endpoint);
                }

                return projectManagementClient;
            }
        }

        public SecurityServiceClient SecurityClient
        {
            get
            {
                if (securityClient == null)
                {
                    var _binding = new BasicHttpsBinding { MaxReceivedMessageSize = 1024000 };
                    var _endpoint = new EndpointAddress(Url + "/WebServices/Security/V1_2/SecurityServiceSecure.svc");
                    securityClient = new SecurityServiceClient(_binding, _endpoint);
                }

                return securityClient;
            }
        }

        public void Authenticate(string initials)
        {
            var _ticket = new FormsAuthenticationTicket(
                             1,
                             initials,
                             DateTime.Now,
                             DateTime.Now.AddDays(CookieDaysTimeout),
                             true,
                             string.Empty
                             );
            var _encryptedTicket = FormsAuthentication.Encrypt(_ticket);
            var _authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, _encryptedTicket) { Expires = DateTime.Now.AddDays(CookieDaysTimeout) };
            HttpContext.Current.Response.Cookies.Add(_authCookie);
        }

        public void Deauthenticate()
        {
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionSecurityTokenExpires) { Expires = DateTime.Now.AddDays(-1d) });
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionSecurityTokenHash) { Expires = DateTime.Now.AddDays(-1d) });
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionSecurityTokenInitials) { Expires = DateTime.Now.AddDays(-1d) });
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(SessionFirstName) { Expires = DateTime.Now.AddDays(-1d) });
            FormsAuthentication.SignOut();
        }
    }
}