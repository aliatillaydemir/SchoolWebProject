using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Web.Security;
using System.Configuration;

namespace SchoolProject.Controllers
{
    [Authorize]//AllowAnonymous
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        //[HttpPost]
        //public ActionResult Contact(Mail model)
        //{
        //    // Web.config'ten e-posta ayarlarını al
        //    string smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
        //    int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
        //    string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
        //    string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

        //    // MailMessage nesnesi oluşturma
        //    MailMessage message = new MailMessage
        //    {
        //        From = new MailAddress(smtpUser),
        //        Subject = model.Subject,
        //        Body = model.Body,
        //        IsBodyHtml = false
        //    };
        //    message.To.Add("atilla734@gmail.com"); // Alıcı e-posta adresi

        //    // SmtpClient nesnesi oluştur ve yapılandır
        //    SmtpClient smtp = new SmtpClient
        //    {
        //        Host = smtpServer,
        //        Port = smtpPort,
        //        EnableSsl = true,
        //        Credentials = new NetworkCredential(smtpUser, smtpPass)
        //    };

        //    try
        //    {
        //        smtp.Send(message);
        //        ViewBag.Message = "Mail has been sent successfully";
        //    }
        //    catch (SmtpException smtpEx)
        //    {
        //        ViewBag.Message = "SMTP Error: " + smtpEx.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = "An error occurred: " + ex.Message;
        //    }

        //    return View();
        //}

        [HttpPost]
        public JsonResult Contact(Mail model)
        {
            // Web.config'ten e-posta ayarlarını al
            string smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

            // MailMessage nesnesi oluşturma
            MailMessage message = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = model.Subject,
                Body = model.Body,
                IsBodyHtml = false
            };
            message.To.Add("xx@gmail.com"); // Alıcı e-posta adresi

            // SmtpClient nesnesi oluştur ve yapılandır
            SmtpClient smtp = new SmtpClient
            {
                Host = smtpServer,
                Port = smtpPort,
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPass)
            };

            try
            {
                smtp.Send(message);
                return Json(new { success = true, message = "Mail has been sent successfully" });
            }
            catch (SmtpException smtpEx)
            {
                return Json(new { success = false, message = "SMTP Error: " + smtpEx.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }


    }

}