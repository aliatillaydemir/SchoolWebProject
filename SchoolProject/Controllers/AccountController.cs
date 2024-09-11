using SchoolProject.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;

namespace SchoolProject.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private static readonly HttpClient client = new HttpClient();

        static AccountController()
        {
            client.BaseAddress = new Uri("http://localhost:53020/"); 
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // GET: Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        // GET: Login
        public ActionResult Login()
        {
            ViewBag.HideNavbar = true; // Hide the navbar on the Login page
            return View();
        }

        //// POST: Login
        //[HttpPost]
        //public ActionResult Login(string username, string password)
        //{
        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        ViewBag.ErrorMessage = "Username and password are required.";
        //        return View();
        //    }

        //    using (SchoolWebEntities db = new SchoolWebEntities())
        //    {
        //        var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Username == username && u.Password == password);

        //        if (user != null)
        //        {
        //            FormsAuthentication.SetAuthCookie(username, false);
        //            Session["RoleId"] = user.RoleId;
        //            return RedirectToAction("Index", "Home");
        //        }
        //        else
        //        {
        //            // Hata mesajını belirle ve login sayfasına geri dön
        //            ViewBag.ErrorMessage = "Invalid username or password!";
        //            return View();
        //        }
        //    }
        //}


        // GET: Register

        // POST: Login
        [HttpPost]
        public async Task<ActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Username and password are required.";
                return View();
            }

            var loginModel = new { Username = username, Password = password };
            HttpResponseMessage response = await client.PostAsJsonAsync("api/Users/Login", loginModel);

            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadAsAsync<dynamic>();
                FormsAuthentication.SetAuthCookie(username, false);
                Session["RoleId"] = (int)user.RoleId;
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password!";
                return View();
            }
        }

        public ActionResult Register()
        {
            ViewBag.HideNavbar = true; // Hide the navbar on the Register page
            return View();
        }

        //// POST: Register
        //[HttpPost]
        //public ActionResult Register(string username, string password, string role)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        using (SchoolWebEntities db = new SchoolWebEntities())
        //        {
        //            if (db.Users.Any(u => u.Username == username))
        //            {
        //                ViewBag.ErrorMessage = "Username already exists. Try Again.";
        //                return View();
        //            }

        //            var newUser = new User
        //            {
        //                Username = username,
        //                Password = password,
        //                RoleId = int.Parse(role) // Ensure role ID is validated
        //            };

        //            db.Users.Add(newUser);
        //            db.SaveChanges();

        //            return RedirectToAction("Login");
        //        }
        //    }

        //    return View();
        //}

        // POST: Register

        [HttpPost]
        public async Task<ActionResult> Register(string username, string password, string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                ViewBag.ErrorMessage = "All fields are required.";
                return View();
            }

            var user = new { Username = username, Password = password, RoleId = int.Parse(role) };
            HttpResponseMessage response = await client.PostAsJsonAsync("api/Users/Register", user);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.ErrorMessage = "Registration failed or username already exists.";
                return View();
            }
        }

        public ActionResult InvalidPage()
        {
            ViewBag.HideNavbar = true; // Hide the navbar on the Invalid Page
            return View();
        }
    }
}
