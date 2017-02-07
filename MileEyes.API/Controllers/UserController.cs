using System.Web.Mvc;

namespace MileEyes.API.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
    }
}