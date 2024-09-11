using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net;
using StackExchange.Redis;
using System.Runtime.Caching;

namespace SchoolProject.Controllers
{
    [Authorize]
    public class TeachersController : Controller
    {
        private SchoolWebEntities db = new SchoolWebEntities();

        private static readonly HttpClient client = new HttpClient();
        //private static readonly MemoryCache memorycache = MemoryCache.Default;

        //private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379,password=7zlNGs8wFvWfeWNFQ0vqZvRTfNKFnfpc,abortConnect=false");
        //private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:63592,abortConnect=false");
        //private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("redis-16336.c311.eu-central-1-1.ec2.redns.redis-cloud.com:16336:6379,password=7zlNGs8wFvWfeWNFQ0vqZvRTfNKFnfpc,abortConnect=false");
        //static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("redis-16336.c311.eu-central-1-1.ec2.redns.redis-cloud.com:16336,password=7zlNGs8wFvWfeWNFQ0vqZvRTfNKFnfpc,abortConnect=false");

        //private static readonly IDatabase cache = redis.GetDatabase();


        //// Connection string format: "hostname:port,password=yourPassword"
        //private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");

        //// Get a database instance from the connection
        //private static readonly IDatabase cache = redis.GetDatabase();


        private static readonly Lazy<ConnectionMultiplexer> lazyRedis = new Lazy<ConnectionMultiplexer>(() =>
        {
            try
            {
                // Redis bağlantısını kur
                return ConnectionMultiplexer.Connect("localhost:6379");
            }
            catch (Exception ex)
            {
                // Hata mesajını logla
                Console.WriteLine($"Redis bağlantısı kurulamadı: {ex.Message}");
                // Geri dönüş olarak null veya başka bir varsayılan değer döndürebilirsiniz
                return null;
            }
        });

        private static ConnectionMultiplexer redis => lazyRedis.Value;
        private static IDatabase cache => redis?.GetDatabase();



        static TeachersController()
        {
            client.BaseAddress = new Uri("http://localhost:53020/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        ////GET: Teachers
        //public async Task<ActionResult> Index()
        //{
        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId;

        //    // Kullanıcı rolü kontrolü
        //    if (roleId == 2)
        //    {
        //        // Kullanıcı 'Teacher' sekmesine erişmeye çalışırsa yönlendir
        //        ViewBag.ErrorMessage = "Access denied. Redirecting...";
        //        return RedirectToAction("InvalidPage", "Account");
        //    }
        //    else
        //    {
        //        ViewBag.ShowBackButton = true;

        //        // Store a simple string
        //        dbredis.StringSet("foo", "bar");

        //        // Retrieve the string
        //        string value = dbredis.StringGet("foo");
        //        Console.WriteLine(value); // Outputs: bar


        //        return View();
        //    }
        //}



        //GET: Teachers
        public async Task<ActionResult> Index()
        {
            var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
            ViewBag.RoleId = roleId;

            // Kullanıcı rolü kontrolü
            if (roleId == 2)
            {
                // Kullanıcı 'Teacher' sekmesine erişmeye çalışırsa yönlendir
                ViewBag.ErrorMessage = "Access denied. Redirecting...";
                return RedirectToAction("InvalidPage", "Account");
            }
            else
            {
                ViewBag.ShowBackButton = true;

                List<Teacher> teachers;
                var cacheKey = "teachers_cache_key";

                if (cache != null)
                {
                    // Cache'den veri al
                    var cachedTeachers = await cache.StringGetAsync(cacheKey);

                    if (cachedTeachers.IsNullOrEmpty)
                    {
                        // Cache'de veri bulunamadı, API'den çek
                        HttpResponseMessage response = await client.GetAsync("api/teachers");
                        if (response.IsSuccessStatusCode)
                        {
                            var jsonString = await response.Content.ReadAsStringAsync();
                            teachers = JsonConvert.DeserializeObject<List<Teacher>>(jsonString);

                            // Veriyi cache'e ekle (örneğin 40 saniye boyunca geçerli olacak şekilde)
                            await cache.StringSetAsync(cacheKey, jsonString, TimeSpan.FromSeconds(1));
                        }
                        else
                        {
                            // Hata durumunda View döndür
                            return View("Error");
                        }
                    }
                    else
                    {
                        // Cache'den veri al
                        teachers = JsonConvert.DeserializeObject<List<Teacher>>(cachedTeachers);
                    }
                }
                else
                {
                    // Redis bağlantısı yoksa doğrudan API'den veri al
                    HttpResponseMessage response = await client.GetAsync("api/teachers");
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        teachers = JsonConvert.DeserializeObject<List<Teacher>>(jsonString);
                    }
                    else
                    {
                        // Hata durumunda View döndür
                        return View("Error");
                    }
                }

                return View(teachers);
            }
        }



            // GET: Teachers
            //public async Task<ActionResult> Index()
            //{
            //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
            //    ViewBag.RoleId = roleId;

            //    // Kullanıcı rolü kontrolü
            //    if (roleId == 2)
            //    {
            //        // Kullanıcı 'Teacher' sekmesine erişmeye çalışırsa yönlendir
            //        ViewBag.ErrorMessage = "Access denied. Redirecting...";
            //        return RedirectToAction("InvalidPage", "Account");
            //    }
            //    else
            //    {
            //        ViewBag.ShowBackButton = true;

            //        // Cache key tanımlama
            //        var cacheKey = "teachers_cache_key";

            //        // Cache'den veri al
            //        var cachedTeachers = memorycache.Get(cacheKey) as string;

            //        List<Teacher> teachers;

            //        if (cachedTeachers == null)
            //        {
            //            // Cache'de veri bulunamadı, API'den çek
            //            HttpResponseMessage response = await client.GetAsync("api/teachers");
            //            if (response.IsSuccessStatusCode)
            //            {
            //                var jsonString = await response.Content.ReadAsStringAsync();
            //                teachers = JsonConvert.DeserializeObject<List<Teacher>>(jsonString);

            //                // Veriyi cache'e ekle (örneğin 40 saniye boyunca geçerli olacak şekilde)
            //                memorycache.Set(cacheKey, jsonString, DateTimeOffset.Now.AddSeconds(40));
            //            }
            //            else
            //            {
            //                // Hata durumunda View döndür
            //                return View("Error");
            //            }
            //        }
            //        else
            //        {
            //            // Cache'den veri al
            //            teachers = JsonConvert.DeserializeObject<List<Teacher>>(cachedTeachers);
            //        }

            //        return View(teachers);
            //    }
            //}

            // GET: Teachers/Details/5
            public async Task<ActionResult> Details(int id)
        {
            // Fetch teacher details from the API
            HttpResponseMessage response = await client.GetAsync($"api/teacher/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var teacher = JsonConvert.DeserializeObject<Teacher>(jsonString);
                return View(teacher);
            }
            else
            {
                return HttpNotFound();
            }
        }

        // GET: Teachers/Create
        public async Task<ActionResult> Create()
        {
            HttpResponseMessage response = await client.GetAsync("api/Courses/Active");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var activeCourses = JsonConvert.DeserializeObject<List<Course>>(jsonString);
                ViewBag.CourseList = new SelectList(activeCourses, "CourseId", "CourseName");
            }
            else
            {
                ViewBag.CourseList = new SelectList(new List<Course>(), "CourseId", "CourseName");
            }

            return View();
        }


        // POST: Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                // POST teacher to API
                var response = await client.PostAsJsonAsync("api/Teachers", teacher);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index"); // Redirect to index or another view upon success
                }
                else
                {
                    // Add an error message if the response is not successful
                    ModelState.AddModelError("", "An error occurred while creating the teacher.");
                }
            }

            // If the model state is invalid or API call fails, reload the dropdown list and return the view
            var courseResponse = await client.GetAsync("api/Courses/Active");
            if (courseResponse.IsSuccessStatusCode)
            {
                var courseJson = await courseResponse.Content.ReadAsStringAsync();
                var courses = JsonConvert.DeserializeObject<List<Course>>(courseJson);
                ViewBag.CourseList = new SelectList(courses, "CourseId", "CourseName");
            }

            return View(teacher);
        }


        // GET: Teachers/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            // Get teacher details
            var teacherResponse = await client.GetAsync($"api/Teacher/{id}");
            if (!teacherResponse.IsSuccessStatusCode)
            {
                return HttpNotFound();
            }
            var teacherJson = await teacherResponse.Content.ReadAsStringAsync();
            var teacher = JsonConvert.DeserializeObject<Teacher>(teacherJson);

            // Get active courses
            var coursesResponse = await client.GetAsync("api/Courses/Active");
            if (!coursesResponse.IsSuccessStatusCode)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Unable to retrieve courses.");
            }
            var coursesJson = await coursesResponse.Content.ReadAsStringAsync();
            var courses = JsonConvert.DeserializeObject<List<Course>>(coursesJson);

            ViewBag.CourseList = new SelectList(courses, "CourseId", "CourseName", teacher.CourseId);

            return View(teacher);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(Teacher teacher)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Update API endpoint URL
        //        HttpResponseMessage response = await client.PutAsJsonAsync($"api/Teachers/{teacher.TeacherId}", teacher);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return RedirectToAction("Index"); // Redirect to the Index page on success
        //        }
        //        else
        //        {
        //            var errorContent = await response.Content.ReadAsStringAsync();
        //            ModelState.AddModelError("", $"Teacher could not be updated. Server response: {errorContent}");
        //        }
        //    }

        //    // Reload the course list and return the view in case of failure
        //    HttpResponseMessage courseResponse = await client.GetAsync("api/Courses/Active");
        //    if (courseResponse.IsSuccessStatusCode)
        //    {
        //        var jsonString = await courseResponse.Content.ReadAsStringAsync();
        //        var activeCourses = JsonConvert.DeserializeObject<List<Course>>(jsonString);
        //        ViewBag.CourseList = new SelectList(activeCourses, "CourseId", "CourseName");
        //    }
        //    else
        //    {
        //        ViewBag.CourseList = new SelectList(new List<Course>(), "CourseId", "CourseName");
        //    }

        //    return View(teacher);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                // Update API endpoint URL
                HttpResponseMessage response = await client.PutAsJsonAsync($"api/Teachers/{teacher.TeacherId}", teacher);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index"); // Redirect to the Index page on success
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Teacher could not be updated. Server response: {errorContent}");
                }
            }

            // Reload the course list and return the view in case of failure
            HttpResponseMessage courseResponse = await client.GetAsync("api/Courses/Active");
            if (courseResponse.IsSuccessStatusCode)
            {
                var jsonString = await courseResponse.Content.ReadAsStringAsync();
                var activeCourses = JsonConvert.DeserializeObject<List<Course>>(jsonString);
                ViewBag.CourseList = new SelectList(activeCourses, "CourseId", "CourseName");
            }
            else
            {
                ViewBag.CourseList = new SelectList(new List<Course>(), "CourseId", "CourseName");
            }

            return View(teacher);
        }



        // POST: Teachers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // API'ye kursu silme isteği gönder
                HttpResponseMessage response = await client.PostAsync($"api/Teachers/DeleteTeacher/{id}", null);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "Kursu silerken bir hata oluştu.");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Kursu silerken bir hata oluştu.");
            }

            // Hata durumunda Index sayfasına dön
            return RedirectToAction("Index");
        }



        //// POST: Teachers/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    var teacher = db.Teachers.Find(id);
        //    if (teacher == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    teacher.IsDeleted = true;
        //    teacher.DeletedDate = DateTime.Now.Date; // Only date part

        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}


        //// GET: Teachers
        //public ActionResult Index()
        //{
        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId; // Kullanıcı rolünü ViewBag ile gönderiyoruz

        //    // Kullanıcı rolü kontrolü
        //    if (roleId == 2) // Kullanıcı rolü  //eğer user id == 2 ise yani standart kullanıcı isek...
        //    {
        //        // Kullanıcı 'Teacher' sekmesine erişmeye çalışırsa yönlendir
        //        ViewBag.ErrorMessage = "Access denied. Redirecting...";
        //        return RedirectToAction("InvalidPage", "Account");
        //    }
        //    else
        //    {
        //        ViewBag.ShowBackButton = true; // Geri butonunu göster
        //        var teachers = db.Teachers.Include(t => t.Course).ToList();
        //        return View(teachers);
        //    }
        //}


        //// GET: Teachers/Details/5
        //public ActionResult Details(int id)
        //{
        //    var teacher = db.Teachers.Include(t => t.Course).FirstOrDefault(t => t.TeacherId == id);
        //    if (teacher == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(teacher);
        //}

        //// GET: Teachers/Create
        //public ActionResult Create()
        //{
        //    // Populate the dropdown list with available courses that are not deleted
        //    ViewBag.CourseList = new SelectList(db.Courses.Where(c => !(bool)c.IsDeleted), "CourseId", "CourseName");
        //    return View();
        //}


        //// POST: Teachers/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Teacher teacher)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Set default values for optional fields
        //        teacher.CreatedDate = DateTime.Now;
        //        teacher.IsDeleted = false; // Set default IsDeleted to false

        //        // Default to true if IsActive is not provided
        //        if (!teacher.IsActive.HasValue)
        //        {
        //            teacher.IsActive = true; // Default to Active
        //        }

        //        // Check if the provided CourseId exists
        //        if (!db.Courses.Any(c => c.CourseId == teacher.CourseId))
        //        {
        //            ModelState.AddModelError("CourseId", "The selected course does not exist.");
        //            ViewBag.CourseList = new SelectList(db.Courses, "CourseId", "CourseName", teacher.CourseId);
        //            return View(teacher);
        //        }

        //        db.Teachers.Add(teacher);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    // If validation fails, reload the course list and return the view
        //    ViewBag.CourseList = new SelectList(db.Courses, "CourseId", "CourseName", teacher.CourseId);
        //    return View(teacher);
        //}


        //// GET: Teachers/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    var teacher = db.Teachers.Find(id);
        //    if (teacher == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    ViewBag.CourseList = new SelectList(db.Courses, "CourseId", "CourseName", teacher.CourseId);
        //    return View(teacher);
        //}


        //// POST: Teachers/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Teacher teacher)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var existingTeacher = db.Teachers.Find(teacher.TeacherId);
        //        if (existingTeacher == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        // Ensure that IsActive and IsDeleted are not null
        //        existingTeacher.TeacherName = teacher.TeacherName;
        //        existingTeacher.CourseId = teacher.CourseId;
        //        existingTeacher.IsActive = teacher.IsActive.HasValue ? teacher.IsActive.Value : existingTeacher.IsActive;
        //        existingTeacher.IsDeleted = teacher.IsDeleted.HasValue ? teacher.IsDeleted.Value : existingTeacher.IsDeleted;
        //        existingTeacher.CreatedDate = teacher.CreatedDate.HasValue ? teacher.CreatedDate.Value : existingTeacher.CreatedDate;
        //        existingTeacher.UpdatedDate = DateTime.Now; // Update the UpdatedDate

        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.CourseList = new SelectList(db.Courses, "CourseId", "CourseName", teacher.CourseId);
        //    return View(teacher);
        //}

        //// GET: Teachers/Delete/5       //popup var buna gerek kalmadı
        //public ActionResult Delete(int id)
        //{
        //    var teacher = db.Teachers.Find(id);
        //    if (teacher == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(teacher);
        //}


    }
}
