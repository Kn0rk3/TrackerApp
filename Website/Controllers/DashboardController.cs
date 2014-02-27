using System.Web.Mvc;

namespace TrackerApp.Website.Controllers
{
    public class DashboardController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
    }
}
