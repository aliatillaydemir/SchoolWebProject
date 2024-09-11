using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Runtime.Caching;


namespace SchoolProject.Controllers
{
    [Authorize]
    public class CoursesController : Controller
    {
        private SchoolWebEntities db = new SchoolWebEntities();
        
        private static readonly HttpClient client = new HttpClient();
        private static readonly MemoryCache cache = MemoryCache.Default;


        static CoursesController()
        {
            client.BaseAddress = new Uri("http://localhost:53020/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // GET: Courses
        public async Task<ActionResult> Index()
        {
            var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
            ViewBag.RoleId = roleId;

            // Cache key tanımlama
            var cacheKey = "courses_cache_key";
            var timeKey = "cache_time_key";

            // Cache'den veri al
            var cachedCourses = cache.Get(cacheKey) as string;
            var cachedTime = cache.Get(timeKey) as string;


            List<Course> courses;
            DateTime cacheTime;

            if (cachedCourses == null || cachedTime == null)
            {
                // Cache'de veri bulunamadı, API'den çek
                HttpResponseMessage response = await client.GetAsync("api/Courses");
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    courses = JsonConvert.DeserializeObject<List<Course>>(jsonString);

                    // Şu anki saati al
                    var currentTime = DateTime.Now.ToString("HH:mm:ss");

                    // Veriyi ve saati cache'e ekle
                    cache.Set(cacheKey, jsonString, DateTimeOffset.Now.AddSeconds(1));
                    cache.Set(timeKey, currentTime, DateTimeOffset.Now.AddSeconds(1));
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
                courses = JsonConvert.DeserializeObject<List<Course>>(cachedCourses);
                // Cache'den saati al
                cacheTime = DateTime.Parse(cachedTime);
            }

            // Şu anki saati al ve ViewBag'e ekle
            ViewBag.CurrentTime = cachedTime;

            return View(courses);
        }


        // GET: Courses/Details/5
        public async Task<ActionResult> Details(int id)
        {
            // Fetch course details from the API
            HttpResponseMessage response = await client.GetAsync($"api/Courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<Course>(jsonString);
                return View(course);
            }
            else
            {
                return HttpNotFound();
            }
        }

        //GET: Courses/Create,          //ayrıca silinmiş kurslar gösteriliyor.
        public async Task<ActionResult> Create()
        {
            // Silinmiş kursları API'den al
            HttpResponseMessage response = await client.GetAsync("api/Corses/DeletedCors");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var deletedCourses = JsonConvert.DeserializeObject<List<Course>>(jsonString);
                ViewBag.DeletedCourses = deletedCourses;
            }
            else
            {
                ViewBag.DeletedCourses = new List<Course>(); // Başarısız durumda boş liste döndür
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                // Yeni kurs oluşturmak için API'ye POST isteği gönder
                HttpResponseMessage response = await client.PostAsJsonAsync("api/Courses", course);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index"); // Başarıyla oluşturulursa ana sayfaya yönlendir
                }
                else
                {
                    ModelState.AddModelError("", "Kurs oluşturulurken bir hata oluştu.");
                }
            }

            // Model geçerli değilse veya API isteği başarısızsa view'a dön
            return View(course);
        }


        // PUT: Courses/Restore/5
        public async Task<ActionResult> Restore(int id)
        {
            // Silinmiş kursu geri getirmek için API'ye PUT isteği gönder
            HttpResponseMessage response = await client.PutAsync($"api/Courses/Restore/{id}", null);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Create"); // Başarıyla geri getirilirse Create sayfasına dön
            }
            else
            {
                return HttpNotFound();
            }
        }


        // GET: Courses/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            HttpResponseMessage response = await client.GetAsync($"api/Courses/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var course = JsonConvert.DeserializeObject<Course>(jsonString);
                if (course != null && !course.IsDeleted.GetValueOrDefault(false))
                {
                    return View(course);
                }
            }
            return HttpNotFound();
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Course course)
        {
            if (ModelState.IsValid)
            {
                // Kurs güncelleme için API'ye PUT isteği gönder
                HttpResponseMessage response = await client.PostAsJsonAsync($"api/Courses/update/{course.CourseId}", course);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Kursu güncellerken bir hata oluştu: {errorMessage}");
                }
            }

            // Model geçerli değilse veya API isteği başarısızsa view'a dön
            return View(course);
        }



        // POST: Courses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // API'ye kursu silme isteği gönder
                HttpResponseMessage response = await client.PostAsync($"api/Courses/DeleteCourse/{id}", null);
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



        //// GET: Courses
        //public ActionResult Index()
        //{

        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId; // Kullanıcı rolünü ViewBag ile gönderiyoruz

        //    // Fetch courses where IsDeleted is false
        //    var courses = db.Courses.Where(c => !(bool)c.IsDeleted).ToList();
        //    return View(courses);

        //}


        //// GET: Courses/Details/5
        //public ActionResult Details(int id)
        //{
        //    var course = db.Courses.Find(id);
        //    if (course == null || (bool)course.IsDeleted)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(course);
        //}

        //// GET: Courses/Create
        //public ActionResult Create()
        //{
        //    //return View();
        //    // Silinmiş kursları al
        //    var deletedCourses = db.Courses.Where(c => c.IsDeleted ?? false).ToList();

        //    // View'e silinmiş kursları gönder
        //    ViewBag.DeletedCourses = deletedCourses;
        //    return View();

        //}

        //// POST: Courses/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Course course)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            course.CreatedDate = DateTime.Now;
        //            course.IsDeleted = false; // Default to not deleted
        //            course.IsActive = course.IsActive ?? true; // Default to Active

        //            db.Courses.Add(course);
        //            db.SaveChanges();

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception ex)
        //        {
        //            ModelState.AddModelError("", "Unable to save changes.");
        //        }
        //    }

        //    return View(course);
        //}

        //// GET: Courses/Restore/5
        //public ActionResult Restore(int id)
        //{
        //    var course = db.Courses.Find(id);
        //    if (course == null || !course.IsDeleted.GetValueOrDefault(false))
        //    {
        //        return HttpNotFound();
        //    }

        //    // Kursu geri getir
        //    course.IsDeleted = false;
        //    course.DeletedDate = null; // Silinme tarihini sıfırla
        //    db.SaveChanges();

        //    return RedirectToAction("Create");
        //}


        //// GET: Courses/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    var course = db.Courses.Find(id);
        //    if (course == null || course.IsDeleted.GetValueOrDefault(false))
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(course);
        //}


        //// POST: Courses/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Course course)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            // CourseId kullanarak mevcut kursu bul
        //            var existingCourse = db.Courses.Find(course.CourseId);
        //            if (existingCourse == null || existingCourse.IsDeleted.GetValueOrDefault(false))
        //            {
        //                return HttpNotFound();
        //            }

        //            // Verileri güncelle
        //            existingCourse.CourseName = course.CourseName;
        //            existingCourse.IsActive = course.IsActive;
        //            existingCourse.UpdatedDate = DateTime.Now;

        //            // Entry durumunu değiştirme
        //            db.Entry(existingCourse).State = EntityState.Modified;
        //            db.SaveChanges();

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception ex)
        //        {
        //            // Hata mesajı
        //            ModelState.AddModelError("", $"Unable to save changes. Error: {ex.Message}");
        //        }
        //    }

        //    return View(course);
        //}


        //// GET: Courses/Delete/5                        //artık popup var buna gerek kalmadı.
        //public ActionResult Delete(int id)
        //{
        //    var course = db.Courses.Find(id);
        //    if (course == null || (bool)course.IsDeleted)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(course);
        //}

        //// POST: Courses/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        var course = db.Courses.Find(id);
        //        if (course == null || (bool)course.IsDeleted)
        //        {
        //            return HttpNotFound();
        //        }

        //        // Perform soft delete
        //        course.IsDeleted = true;
        //        course.DeletedDate = DateTime.Now.Date; // Only set the date part
        //        db.SaveChanges();

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
