using QueryRoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Web.Mvc;


namespace QueryRoom.Controllers.MVC_controllers
{
    [Authorize]
    public class QuestionsMVCController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()                                                 //OKK for dtos
        {
            return RedirectToAction("AllQuestions");
        }

        [Authorize(Roles = "Admin, User")]
        public ActionResult QuestionPost()                                           //OKK for dtos
        {
            return View();
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost]
        public ActionResult QuestionPost(Questions obj)                           //OKK for DTOs
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpClient hc = new HttpClient();
                obj.USERNAME = User.Identity.Name;
                

                //port number to be replaced as per the developer's localhost
                hc.BaseAddress = new Uri("https://localhost:44315/api/QuestionsAPI");

                var insertRec = hc.PostAsJsonAsync<Questions>("QuestionsAPI", obj);
                insertRec.Wait();

                var saveRec = insertRec.Result;
                if (saveRec.IsSuccessStatusCode)
                {
                    Response.Write("<script>alert('The Question Is Posted')</script>");
                }
                else
                {
                    Response.Write("<script>alert('Question Cannot Be Posted')</script>");
                }
                return View("../HomePage/Index");
            }
            else
                return View("../AccountMVC/login");
        }

        [Authorize(Roles = "Admin, User")]
        //user specific Questions
        [HttpGet]
        public ActionResult ViewQuestions()                                                             //OKK for DTOs
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpClient hc = new HttpClient();
                IEnumerable<Questions> question = null;
                var userid = User.Identity.Name;
                //port number to be replaced as per the developer's localhost
                hc.BaseAddress = new Uri("https://localhost:44315/");

                var responseTask = hc.GetAsync("User/Questions/" + userid);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var readResult = result.Content.ReadAsAsync<List<Questions>>();
                    readResult.Wait();
                    question = readResult.Result;
                }
                else
                {
                    question = Enumerable.Empty<Questions>();
                    ModelState.AddModelError(string.Empty, "Server error occurred");
                }
                return View(question);
            }
            else
                return View("../AccountMVC/login");
        }

        //All Questions of every user

        [Authorize(Roles ="Admin, User")]
        [HttpGet]
        public ActionResult AllQuestions(string Search_Data)                                        //OKK for DTOs   
        {
            HttpClient hc = new HttpClient();
            IEnumerable<Questions> questions = null;
            hc.BaseAddress = new Uri("https://localhost:44315/");
            var responseTask = hc.GetAsync("api/QuestionsAPI");
            responseTask.Wait();

            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readResult = result.Content.ReadAsAsync<List<Questions>>();
                readResult.Wait();
                questions = readResult.Result;
            }
            else
            {
                questions = Enumerable.Empty<Questions>();
                ModelState.AddModelError(string.Empty, "Server error occurred");
            }
            var allquestionsByOtherUsers = questions.Where(x => x.USERNAME != User.Identity.Name);
            int matched = 0;
            var SearchedList = new List<SearchResult>();
            if (!String.IsNullOrEmpty(Search_Data)) //if search data is found
            {
                var allwords = Search_Data.Split(' ');
                foreach (var que in questions)
                {
                    foreach (var words in allwords)
                    {
                        if (words.Length>1 && que.QUESTION.ToLower().Contains(words.ToLower()))
                        {
                            matched += 1;
                        }
                    }
                    if (matched > 0)
                    {
                        SearchedList.Add(new SearchResult { matchedWords = matched, question = que });
                    }

                    matched = 0;
                }
                SearchedList = SearchedList.OrderByDescending(x => x.matchedWords).ThenBy(x => x.question.QUESTION.Length - Search_Data.Length).ToList();
                allquestionsByOtherUsers = SearchedList.Select(x => x.question).ToList();  
            }
            return View(allquestionsByOtherUsers) ;
        }

        [Authorize(Roles = "Admin, User")]
        public ActionResult DeleteQuestion(string guid)                                         //OKK for dtos
        {
            if (guid == null)
            {
                return View("~/Views/AnswersMVC/Custom404Error.cshtml");
            }

            HttpClient hc = new HttpClient();
            hc.BaseAddress = new Uri("https://localhost:44315/");
            var deleteTask = hc.DeleteAsync("Questions/Delete/" + guid);
            deleteTask.Wait();
            var result = deleteTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Response.Write("<script>alert('The question is deleted')</script>");
                return View("DelConfirmed");
            }
            Response.Write("<script>alert('Deletion Failed')</script>");
            return View("ViewQuestions");

        }
    }
}