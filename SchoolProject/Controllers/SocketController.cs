using Newtonsoft.Json;
using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace SchoolProject.Controllers
{
    [Authorize]
    public class SocketController : Controller
    {

        private static readonly HttpClient client = new HttpClient();

        static SocketController()
        {
            client.BaseAddress = new Uri("http://localhost:53020/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        // GET: Socket
        public async Task<ActionResult> Index()
        {
                var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
                ViewBag.RoleId = roleId;

                List<Course> courses;
                string apiUrl = "api/Courses/";

                try
                {
                    // API'ye istek yap
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        // JSON verisini oku
                        var jsonString = await response.Content.ReadAsStringAsync();
                        // JSON verisini deserialize et
                        courses = JsonConvert.DeserializeObject<List<Course>>(jsonString);
                    }
                    else
                    {
                        // API isteği başarısızsa, hata sayfasına yönlendir
                        return View("Error");
                    }
                }
                catch (Exception ex)
                {
                    // Hata durumunda, View'da hata mesajını gösterebilirsiniz
                    ViewBag.ErrorMessage = ex.Message;
                    return View("Error");
                }

                return View(courses);
            }



        }

    }


