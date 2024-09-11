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
using StackExchange.Redis;

namespace SchoolProject.Controllers
{
    [Authorize]
    public class StudentsController : Controller
    {
        private SchoolWebEntities db = new SchoolWebEntities();

        private static readonly HttpClient client = new HttpClient();

        static StudentsController()
        {
            client.BaseAddress = new Uri("http://localhost:53020/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        //// Connection string format: "hostname:port,password=yourPassword"
        //private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6379");
        //// Get a database instance from the connection
        //private static readonly IDatabase cache = redis.GetDatabase();

        //public async Task<ActionResult> Index()
        //{
        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId;

        //    // Cache anahtarlarını tanımlama
        //    var studentCacheKey = "students_cache_key";
        //    var teacherCacheKey = "st_teacher_cache_key";

        //    // Cache'den öğrenci verilerini alma
        //    var cachedStudents = await cache.StringGetAsync(studentCacheKey);
        //    var students = new List<Student>();

        //    if (cachedStudents.IsNullOrEmpty)
        //    {
        //        // Cache'de öğrenci verisi bulunamadı, API'den çek
        //        HttpResponseMessage studentResponse = await client.GetAsync("api/students");
        //        if (!studentResponse.IsSuccessStatusCode)
        //        {
        //            // Hata durumunda bir view döndür
        //            return View("Error");
        //        }
        //        var studentJsonString = await studentResponse.Content.ReadAsStringAsync();
        //        students = JsonConvert.DeserializeObject<List<Student>>(studentJsonString);

        //        // Veriyi cache'e ekle (40 saniye boyunca geçerli olacak şekilde)
        //        await cache.StringSetAsync(studentCacheKey, studentJsonString, TimeSpan.FromSeconds(40));
        //    }
        //    else
        //    {
        //        // Cache'den öğrenci verilerini al
        //        students = JsonConvert.DeserializeObject<List<Student>>(cachedStudents);
        //    }

        //    // Cache'den öğretmen verilerini alma
        //    var cachedTeachers = await cache.StringGetAsync(teacherCacheKey);
        //    var teachers = new List<Teacher>();

        //    if (cachedTeachers.IsNullOrEmpty)
        //    {
        //        // Cache'de öğretmen verisi bulunamadı, API'den çek
        //        HttpResponseMessage teacherResponse = await client.GetAsync("api/teachers");
        //        if (!teacherResponse.IsSuccessStatusCode)
        //        {
        //            // Hata durumunda bir view döndür
        //            return View("Error");
        //        }
        //        var teacherJsonString = await teacherResponse.Content.ReadAsStringAsync();
        //        teachers = JsonConvert.DeserializeObject<List<Teacher>>(teacherJsonString);

        //        // Veriyi cache'e ekle (örneğin 40 saniye boyunca geçerli olacak şekilde)
        //        await cache.StringSetAsync(teacherCacheKey, teacherJsonString, TimeSpan.FromSeconds(40));
        //    }
        //    else
        //    {
        //        // Cache'den öğretmen verilerini al
        //        teachers = JsonConvert.DeserializeObject<List<Teacher>>(cachedTeachers);
        //    }

        //    // Öğretmenler listesi oluşturmak
        //    var teacherDictionary = teachers.ToDictionary(t => t.TeacherId);

        //    // Öğrencileri ve öğretmenleri güncelle
        //    foreach (var student in students)
        //    {
        //        if (student.StudentCourses != null)
        //        {
        //            foreach (var studentCourse in student.StudentCourses)
        //            {
        //                if (studentCourse.Teacher != null && teacherDictionary.ContainsKey(studentCourse.Teacher.TeacherId))
        //                {
        //                    var teacher = teacherDictionary[studentCourse.Teacher.TeacherId];
        //                    if (teacher.IsDeleted == true)
        //                    {
        //                        studentCourse.Teacher = null; // Öğretmen silindiyse, null yapıyoruz
        //                    }
        //                    else
        //                    {
        //                        studentCourse.Teacher = teacher; // Öğretmen bilgilerini güncelle
        //                    }
        //                }
        //                else
        //                {
        //                    studentCourse.Teacher = null; // Öğretmen bulunamadıysa null yapıyoruz
        //                }
        //            }
        //        }
        //    }

        //    return View(students);
        //}


        // Redis bağlantısını yönetmek için Lazy<T> kullanarak tembel yükleme
        private static readonly Lazy<ConnectionMultiplexer> lazyRedis = new Lazy<ConnectionMultiplexer>(() =>
        {
            try
            {
                return ConnectionMultiplexer.Connect("localhost:6379");
            }
            catch (Exception ex)
            {
                // Hata mesajını logla
                Console.WriteLine($"Redis bağlantısı kurulamadı: {ex.Message}");
                // Geri dönüş olarak null döndürüyoruz
                return null;
            }
        });

        private static ConnectionMultiplexer redis => lazyRedis.Value;
        private static IDatabase cache => redis?.GetDatabase();

        public async Task<ActionResult> Index()
        {
            var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
            ViewBag.RoleId = roleId;

            var studentCacheKey = "students_cache_key";
            var teacherCacheKey = "st_teacher_cache_key";

            var students = new List<Student>();
            var teachers = new List<Teacher>();

            // Redis cache'ten veri alma ve API'ye düşme işlemlerini yönetmek için bir metot 
            async Task<List<T>> GetDataFromCacheOrApi<T>(string cacheKey, string apiEndpoint)
            {
                if (cache != null)
                {
                    var cachedData = await cache.StringGetAsync(cacheKey);
                    if (cachedData.IsNullOrEmpty)
                    {
                        HttpResponseMessage response = await client.GetAsync(apiEndpoint);
                        if (!response.IsSuccessStatusCode)
                        {
                            return new List<T>(); // Hata durumunda boş liste döndür
                        }

                        var jsonString = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<List<T>>(jsonString);

                        await cache.StringSetAsync(cacheKey, jsonString, TimeSpan.FromSeconds(1));
                        return data;
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<List<T>>(cachedData);
                    }
                }
                else
                {
                    // Redis bağlantısı yoksa API'den veri al
                    HttpResponseMessage response = await client.GetAsync(apiEndpoint);
                    if (!response.IsSuccessStatusCode)
                    {
                        return new List<T>(); // Hata durumunda boş liste döndür
                    }

                    var jsonString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<T>>(jsonString);
                }
            }

            // Öğrenci ve öğretmen verilerini al
            students = await GetDataFromCacheOrApi<Student>(studentCacheKey, "api/students");
            teachers = await GetDataFromCacheOrApi<Teacher>(teacherCacheKey, "api/teachers");

            // Öğretmenler listesi oluşturmak
            var teacherDictionary = teachers.ToDictionary(t => t.TeacherId);

            // Öğrencileri ve öğretmenleri güncelle
            foreach (var student in students)
            {
                if (student.StudentCourses != null)
                {
                    foreach (var studentCourse in student.StudentCourses)
                    {
                        if (studentCourse.Teacher != null && teacherDictionary.ContainsKey(studentCourse.Teacher.TeacherId))
                        {
                            var teacher = teacherDictionary[studentCourse.Teacher.TeacherId];
                            studentCourse.Teacher = (bool)teacher.IsDeleted ? null : teacher;
                        }
                        else
                        {
                            studentCourse.Teacher = null;
                        }
                    }
                }
            }

            return View(students);
        }



        //public async Task<ActionResult> Index()
        //{
        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId;

        //    // Öğrencileri al
        //    HttpResponseMessage studentResponse = await client.GetAsync("api/students");
        //    if (!studentResponse.IsSuccessStatusCode)
        //    {
        //        // Hata durumunda bir view dönme
        //        return View("Error");
        //    }
        //    var studentJsonString = await studentResponse.Content.ReadAsStringAsync();
        //    var students = JsonConvert.DeserializeObject<List<Student>>(studentJsonString);

        //    // Öğretmenleri al
        //    HttpResponseMessage teacherResponse = await client.GetAsync("api/teachers");
        //    if (!teacherResponse.IsSuccessStatusCode)
        //    {
        //        // Hata durumunda bir view dönme
        //        return View("Error");
        //    }
        //    var teacherJsonString = await teacherResponse.Content.ReadAsStringAsync();
        //    var teachers = JsonConvert.DeserializeObject<List<Teacher>>(teacherJsonString);

        //    // Öğretmenler listesi oluşturmak
        //    var teacherDictionary = teachers.ToDictionary(t => t.TeacherId);

        //    // Öğrencileri ve öğretmenleri güncelle
        //    foreach (var student in students)
        //    {
        //        if (student.StudentCourses != null)
        //        {
        //            foreach (var studentCourse in student.StudentCourses)
        //            {
        //                if (studentCourse.Teacher != null && teacherDictionary.ContainsKey(studentCourse.Teacher.TeacherId))
        //                {
        //                    var teacher = teacherDictionary[studentCourse.Teacher.TeacherId];
        //                    if (teacher.IsDeleted == true)
        //                    {
        //                        studentCourse.Teacher = null; // Öğretmen silindiyse, null yapıyoruz
        //                    }
        //                    else
        //                    {
        //                        studentCourse.Teacher = teacher; // Öğretmen bilgilerini güncelle
        //                    }
        //                }
        //                else
        //                {
        //                    studentCourse.Teacher = null; // Öğretmen bulunamadıysa null yapıyoruz
        //                }
        //            }
        //        }
        //    }

        //    return View(students);
        //}


        public async Task<ActionResult> Details(int id)
        {
            var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
            ViewBag.RoleId = roleId;

            HttpResponseMessage response = await client.GetAsync($"api/students/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var student = JsonConvert.DeserializeObject<Student>(jsonString);

                return View(student);
            }
            else
            {
                // Hata durumunda bir view dönme
                return View("Error");
            }
        }

        public async Task<ActionResult> Create()
        {
            HttpResponseMessage response = await client.GetAsync("api/students/create");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(jsonString);

                ViewBag.CourseList = new SelectList(data.CourseList, "CourseId", "CourseName");
                ViewBag.TeacherList = new SelectList(data.TeacherList, "TeacherId", "TeacherName");

                return View();
            }
            else
            {
                return View("Error");
            }
        }

        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Student student)
        {
            if (ModelState.IsValid)
            {
                var jsonString = JsonConvert.SerializeObject(student);
                var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("api/students/create", content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error");
                }
            }

            return View(student);
        }



        // GET: Students/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            HttpResponseMessage response = await client.GetAsync($"api/students/edit/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(jsonString);

                var student = JsonConvert.DeserializeObject<Student>(data.Student.ToString());
                ViewBag.CourseList = new SelectList(data.CourseList, "CourseId", "CourseName");
                ViewBag.TeacherList = new SelectList(data.TeacherList, "TeacherId", "TeacherName");

                return View(student);
            }
            else
            {
                return View("Error");
            }
        }


        //// POST: Students/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Edit(Student student)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var jsonString = JsonConvert.SerializeObject(student);
        //        var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

        //        HttpResponseMessage response = await client.PostAsync("api/students/edit", content);
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return RedirectToAction("Index");
        //        }
        //        else
        //        {
        //            return View("Error");
        //        }
        //    }

        //    // Reload dropdown lists
        //    HttpResponseMessage courseResponse = await client.GetAsync("api/courses");
        //    var courseData = JsonConvert.DeserializeObject<dynamic>(await courseResponse.Content.ReadAsStringAsync());
        //    ViewBag.CourseList = new SelectList(courseData, "CourseId", "CourseName");

        //    HttpResponseMessage teacherResponse = await client.GetAsync("api/teachers");
        //    var teacherData = JsonConvert.DeserializeObject<dynamic>(await teacherResponse.Content.ReadAsStringAsync());
        //    ViewBag.TeacherList = new SelectList(teacherData, "TeacherId", "TeacherName");

        //    return View(student);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Student student, int courseId, int teacherId)
        {
            if (ModelState.IsValid)
            {
                // Formdan gelen veriyi işleme
                // Student nesnesine seçilen ders ve öğretmen bilgilerini ekleyin
                student.StudentCourses = new List<StudentCourse>
        {
            new StudentCourse
            {
                CourseId = courseId,
                TeacherId = teacherId,
                IsActive = true // Varsayılan olarak aktif
            }
        };

                // Serialize the student object including courses and teachers
                var jsonString = JsonConvert.SerializeObject(student);
                var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync("api/students/edit", content);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Error");
                }
            }

            // Reload dropdown lists
            HttpResponseMessage courseResponse = await client.GetAsync("api/courses");
            var courseData = JsonConvert.DeserializeObject<dynamic>(await courseResponse.Content.ReadAsStringAsync());
            ViewBag.CourseList = new SelectList(courseData, "CourseId", "CourseName");

            HttpResponseMessage teacherResponse = await client.GetAsync("api/teachers");
            var teacherData = JsonConvert.DeserializeObject<dynamic>(await teacherResponse.Content.ReadAsStringAsync());
            ViewBag.TeacherList = new SelectList(teacherData, "TeacherId", "TeacherName");

            return View(student);
        }


        // Remove course action
        public async Task<ActionResult> RemoveCourse(int studentId, int courseId)
        {
            var url = $"api/studentcourses/remove/{studentId}/{courseId}";
            var response = await client.DeleteAsync(url);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "The Course removed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "An error occurred while removing the Course.";
            }

            // Öğrenci düzenleme sayfasını yeniden yükleyin
            return RedirectToAction("Edit", new { id = studentId });
        }


        // GET: Courses/GetTeachersByCourse
        public async Task<ActionResult> GetTeachersByCourse(int courseId)
        {
            var response = await client.GetAsync($"api/students/GetTeachersCours?courseId={courseId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var teachers = JsonConvert.DeserializeObject<List<TeacherDto>>(json);
                return Json(teachers, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return new HttpStatusCodeResult(response.StatusCode);
            }
        }

        public class TeacherDto
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }


        // POST: Courses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                // API'ye kursu silme isteği gönder
                HttpResponseMessage response = await client.PostAsync($"api/Students/DeleteStudent/{id}", null);
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


        //public async Task<ActionResult> Index()
        //{
        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId;

        //    HttpResponseMessage response = await client.GetAsync("api/students");
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var jsonString = await response.Content.ReadAsStringAsync();
        //        var students = JsonConvert.DeserializeObject<List<Student>>(jsonString);

        //        // Null değerler kontrolü (isteğe bağlı)
        //        foreach (var student in students)
        //        {
        //            if (student.StudentCourses != null)
        //            {
        //                foreach (var studentCourse in student.StudentCourses)
        //                {
        //                    if (studentCourse.Course == null)
        //                    {
        //                        studentCourse.Course = new Course(); // Veya uygun bir default değer
        //                    }
        //                    if (studentCourse.Teacher == null)
        //                    {
        //                        studentCourse.Teacher = new Teacher(); // Veya uygun bir default değer
        //                    }
        //                }
        //            }
        //        }

        //        return View(students);
        //    }
        //    else
        //    {
        //        // Hata durumunda bir view dönme
        //        return View("Error");
        //    }
        //}



        //// GET: Students
        //public async Task<ActionResult> Index()
        //{
        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId; // Kullanıcı rolünü ViewBag ile gönderiyoruz


        //        ViewBag.ShowBackButton = true; // Geri butonunu göster

        //        // Öğrencileri API'den çekme
        //        HttpResponseMessage response = await client.GetAsync("api/students");
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var jsonString = await response.Content.ReadAsStringAsync();
        //            var students = JsonConvert.DeserializeObject<List<Student>>(jsonString);
        //            return View(students);
        //        }
        //        else
        //        {
        //            // Hata durumunda bir view dönme
        //            return View("Error");
        //        }

        //}


        // GET: Students/Create





        //// GET: Students
        //public ActionResult Index()
        //{
        //    var roleId = Session["RoleId"] != null ? (int)Session["RoleId"] : -1;
        //    ViewBag.RoleId = roleId; // Kullanıcı rolünü ViewBag ile gönderiyoruz

        //    var students = db.Students
        //                     .Where(s => !(bool)s.IsDeleted) // Silinmiş öğrencileri filtrele
        //                     .Include(s => s.StudentCourses)
        //                     .Include(s => s.StudentCourses.Select(sc => sc.Course))
        //                     .Include(s => s.StudentCourses.Select(sc => sc.Teacher))
        //                     .ToList();
        //    return View(students);
        //}



        //// GET: Students/Details/5
        //public ActionResult Details(int id)
        //{
        //    var student = db.Students
        //        .Include(s => s.StudentCourses.Select(sc => sc.Course))
        //        .Include(s => s.StudentCourses.Select(sc => sc.Teacher))
        //        .SingleOrDefault(s => s.StudentId == id);

        //    if (student == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(student);
        //}


        //// GET: Students/Create
        //public ActionResult Create()
        //{
        //    ViewBag.CourseList = new SelectList(db.Courses.Where(c => (bool)c.IsActive), "CourseId", "CourseName");
        //    return View();
        //}


        //// POST: Students/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(Student student, List<int> selectedCourses, List<int> selectedTeachers)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // Varsayılan değeri kontrol edin (isteğe bağlı)
        //        student.IsDeleted = false; // Varsayılan değer, aksi belirtilmemişse
        //        student.CreatedDate = DateTime.Now;
        //        db.Students.Add(student);
        //        db.SaveChanges();

        //        if (selectedCourses != null && selectedCourses.Count > 0)
        //        {
        //            foreach (var courseId in selectedCourses)
        //            {
        //                var teacherId = selectedTeachers.FirstOrDefault(); // Assuming first teacher for simplicity
        //                var studentCourse = new StudentCourse
        //                {
        //                    StudentId = student.StudentId,
        //                    CourseId = courseId,
        //                    TeacherId = teacherId,
        //                    CreatedDate = DateTime.Now
        //                };
        //                db.StudentCourses.Add(studentCourse);
        //            }
        //            db.SaveChanges();
        //        }

        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.CourseList = new SelectList(db.Courses.Where(c => (bool)c.IsActive), "CourseId", "CourseName");
        //    return View(student);
        //}


        //public ActionResult Edit(int id)
        //{
        //    var student = db.Students
        //                    .Include(s => s.StudentCourses.Select(sc => sc.Course))
        //                    .Include(s => s.StudentCourses.Select(sc => sc.Teacher))
        //                    .FirstOrDefault(s => s.StudentId == id);

        //    if (student == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    // Get courses that are not assigned to the student and are not deleted
        //    var assignedCourseIds = student.StudentCourses.Select(sc => sc.CourseId).ToList();
        //    ViewBag.CourseList = new SelectList(db.Courses
        //        .Where(c => !assignedCourseIds.Contains(c.CourseId) && !(bool)c.IsDeleted), // Only check IsDeleted
        //        "CourseId",
        //        "CourseName");

        //    // Get teachers who are not deleted
        //    ViewBag.TeacherList = new SelectList(db.Teachers
        //        .Where(t => !(bool)t.IsDeleted), // Only check IsDeleted
        //        "TeacherId",
        //        "TeacherName");

        //    return View(student);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(Student student, int? CourseId, int? TeacherId)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var existingStudent = db.Students.Include(s => s.StudentCourses)
        //                                         .FirstOrDefault(s => s.StudentId == student.StudentId);

        //        if (existingStudent == null)
        //        {
        //            return HttpNotFound();
        //        }

        //        // Update student details
        //        existingStudent.StudentName = student.StudentName;
        //        existingStudent.IsActive = student.IsActive;
        //        existingStudent.UpdatedDate = DateTime.Now;

        //        // Preserve CreatedDate
        //        db.Entry(existingStudent).Property(s => s.CreatedDate).IsModified = false;

        //        // Add course if selected
        //        if (CourseId.HasValue && TeacherId.HasValue)
        //        {
        //            var existingCourse = existingStudent.StudentCourses.FirstOrDefault(sc => sc.CourseId == CourseId.Value);
        //            if (existingCourse == null)
        //            {
        //                var studentCourse = new StudentCourse
        //                {
        //                    StudentId = student.StudentId,
        //                    CourseId = CourseId.Value,
        //                    TeacherId = TeacherId.Value,
        //                    CreatedDate = DateTime.Now
        //                };

        //                db.StudentCourses.Add(studentCourse);
        //            }
        //        }

        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    // Reload dropdown lists
        //    ViewBag.CourseList = new SelectList(db.Courses.Where(c => !student.StudentCourses.Any(sc => sc.CourseId == c.CourseId)), "CourseId", "CourseName");
        //    ViewBag.TeacherList = new SelectList(db.Teachers, "TeacherId", "TeacherName");

        //    return View(student);
        //}


        //public JsonResult GetTeachersByCourse(int courseId)
        //{
        //    var teachers = db.Teachers
        //                      .Where(t => t.CourseId == courseId && !(bool)t.IsDeleted) // Check if teacher is not deleted
        //                      .Select(t => new SelectListItem
        //                      {
        //                          Value = t.TeacherId.ToString(),
        //                          Text = t.TeacherName
        //                      })
        //                      .ToList();

        //    // Check if there are no available teachers for the selected course
        //    if (teachers.Count == 0)
        //    {
        //        teachers.Add(new SelectListItem
        //        {
        //            Value = "",
        //            Text = "No Teacher Assigned"
        //        });
        //    }

        //    return Json(teachers, JsonRequestBehavior.AllowGet);
        //}



        //// Remove course action
        //public ActionResult RemoveCourse(int studentId, int courseId)
        //{
        //    var studentCourse = db.StudentCourses
        //        .FirstOrDefault(sc => sc.StudentId == studentId && sc.CourseId == courseId);

        //    if (studentCourse != null)
        //    {
        //        db.StudentCourses.Remove(studentCourse);
        //        db.SaveChanges();
        //    }

        //    return RedirectToAction("Edit", new { id = studentId });
        //}


        //// GET: Students/Delete/5                       //popup var buna gerek kalmadı
        //public ActionResult Delete(int id)
        //{
        //    var student = db.Students.Find(id);
        //    if (student == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(student);
        //}

        //// POST: Students/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    var student = db.Students.Find(id);
        //    if (student != null)
        //    {
        //        student.IsDeleted = true;
        //        student.DeletedDate = DateTime.Now;
        //        db.Entry(student).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }
        //    return RedirectToAction("Index");
        //}

    }
}
