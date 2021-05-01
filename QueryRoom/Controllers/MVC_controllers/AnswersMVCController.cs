using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using QueryRoom.Models;
using QueryRoom.Models.DTOs;

namespace QueryRoom.Controllers.MVC_controllers
{
    [Authorize]
    public class AnswersMVCController : Controller
    {
        
        // GET: AnswersMVC
        
        public ActionResult Index()                                                         //OKK for DTOs
        {
            return View();
        }

        [Authorize(Roles = "Admin, User")]
        //POst Answer
        [HttpGet]
        public ActionResult AnswerPost(string qid , string question)                        //OKK for DTOs
        {
            if (qid == null)
            {
                return View("Custom404Error");
            }
            ViewBag.Message = question;
            return View();
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        public ActionResult AnswerPost(Answers obj, Guid qid)                           //OKK for DTOs
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpClient hc = new HttpClient();
                obj.QID = qid;

                obj.USERNAME = User.Identity.Name;

                hc.BaseAddress = new Uri("https://localhost:44315/api/AnswersAPI");

                var insertAns = hc.PostAsJsonAsync<Answers>("AnswersAPI", obj);
                insertAns.Wait();

                var saveAns = insertAns.Result;
                if(saveAns.IsSuccessStatusCode)
                {
                    Response.Write("<script>alert('The Answer Is Posted')</script>");
                }
                else
                {
                    Response.Write("<script>alert('Answer Cannot Be Posted')</script>");
                }
                return View("../HomePage/Index");
            }
            else
                return View("../AccountMVC/login");
        }





        [AllowAnonymous]
        [HttpGet]
        public ActionResult ViewAnswers(string qid , string que)                                 //Okk for DTOs
        {
            if (qid == null )
            {
                return View("Custom404Error");
            }
            HttpClient hc = new HttpClient();
            IEnumerable<Answers> allAnswer = null;

            hc.BaseAddress = new Uri("https://localhost:44315/");
            var responseTask = hc.GetAsync("api/AnswersAPI/GetAnswers?qid=" + qid);

            responseTask.Wait();
            var result = responseTask.Result;
            if(result.IsSuccessStatusCode)
            {
                var readResult = result.Content.ReadAsAsync<List<Answers>>();
                readResult.Wait();
                allAnswer = readResult.Result;
            }
            else
            {
                allAnswer = Enumerable.Empty<Answers>();
                ModelState.AddModelError(string.Empty, "Server error occurred");
            }
            ViewBag.question = que;
            if (allAnswer.Count() != 0)
            {
                var orderedAns = allAnswer.ToList().OrderByDescending(x => x.ISVALIDATED).ThenByDescending(x => x.DATE);
                return View(orderedAns);
            }
            return View("NoAnswers");
        }


        [Authorize(Roles = "User")]
        //Like
        public ActionResult AddLikeOrDislike(Guid aid,int func)                                       //OKK for DTOs
        {
            var question = new Questions();
            if (aid==null)
            {
                return View("Custom404Error");
            }
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri("https://localhost:44315/");
            var responseTask = hc.GetAsync("api/AnswersAPI/InsertLikeOrDislike?aid="+aid+"&func="+func);
            responseTask.Wait();
            var result = responseTask.Result;
            if(result.IsSuccessStatusCode)
            {
                var readResult = result.Content.ReadAsAsync<Questions>();
                readResult.Wait();
                question = readResult.Result;
            }
            else
            {
                question = null;
                ModelState.AddModelError(string.Empty, "Server error occurred");
            }
            //Return question from api
            return RedirectToAction("ViewAnswers",new { qid =question.QID,  que = question.QUESTION});
        }

        //autoValidate the ans on the likes basis

        [Authorize(Roles = "Admin")]
        public ActionResult ValidateAns(Guid aid)                                                                //OKK for dto
        {
           
            var question = new Questions();
            if (aid==null)
            {
                return View("Custom404Error");
            }
            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri("https://localhost:44315/");
            var responseTask = hc.GetAsync("api/AnswersAPI/ValidateAnswer?aid="+aid);
            responseTask.Wait();
            var result = responseTask.Result;
            if(result.IsSuccessStatusCode)
            {
                var readResult = result.Content.ReadAsAsync<Questions>();
                readResult.Wait();
                question = readResult.Result;
            }
            else
            {
                question = null;
                ModelState.AddModelError(string.Empty, "Server error occurred");
            }

            return RedirectToAction("ViewAnswers", new { qid = question.QID, que = question.QUESTION });
        }


    }
}