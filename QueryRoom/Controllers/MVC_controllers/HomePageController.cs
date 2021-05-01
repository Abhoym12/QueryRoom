using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QueryRoom.Controllers.MVC_controllers
{
    [Authorize]
    public class HomePageController : Controller
    {
        // GET: HomePage
        public ActionResult Index()
        {
            ViewBag.username = User.Identity.Name;
            if(User.IsInRole("Admin"))
                return RedirectToAction("Index","AdminMVC");
            return View();
        }
    }
}