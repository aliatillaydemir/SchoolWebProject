using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace SchoolProject.Socket
{
    public class CourseHub : Hub
    {
        // İstemcilere veri gönderimi yapılır
        public void UpdateCourse(int courseId, bool isActive)
        {
            // API'ye istek yaparak kursun durumunu güncelleriz
            var apiUrl = $"api/Courses/update/{courseId}";
            var httpClient = new HttpClient();
            var content = new StringContent(JsonConvert.SerializeObject(new { IsActive = isActive }), Encoding.UTF8, "application/json");

            var response = httpClient.PutAsync(apiUrl, content).Result;

            if (response.IsSuccessStatusCode)
            {
                // Durum başarıyla güncellendiğinde tüm istemcilere güncellemeyi göndeririz
                Clients.All.CourseUpdated(courseId, isActive);
            }
        }
    }

}