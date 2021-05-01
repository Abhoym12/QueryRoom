using Newtonsoft.Json;
using QueryRoom.DTOs;
using QueryRoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace QueryRoom.Controllers.MVC_controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminMVCController : Controller
    {
        // GET: AdminMVC
        public ActionResult Index()                                                               //OKK for dto
        {
            return View();
        }

        public ActionResult GetAllUsers()                                                               //OKK for dto
        {
            HttpClient hc = new HttpClient();
            IEnumerable<UserDetails> users = null;
            hc.BaseAddress = new Uri("https://localhost:44315/");
            var responseTask = hc.GetAsync("api/AdminAPI");
            responseTask.Wait();

            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readResult = result.Content.ReadAsAsync<List<UserDetails>>();
                readResult.Wait();
                users = readResult.Result;
            }
            else
            {
                users = Enumerable.Empty<UserDetails>();
                ModelState.AddModelError(string.Empty, "Server error occurred");
            }
            return View(users);
        }

    }
}