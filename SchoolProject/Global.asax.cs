using SchoolProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace SchoolProject
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_BeginRequest(object sender, EventArgs e)
        //{
        //    var context = HttpContext.Current;

        //    if (context != null && context.User != null && context.User.Identity != null)
        //    {
        //        if (context.User.Identity.IsAuthenticated)
        //        {
        //            // Kullanıcı giriş yapmış
        //            // Burada başka bir işlem yapmanıza gerek yok
        //        }
        //        else
        //        {
        //            // Kullanıcı giriş yapmamış
        //            // Hedef URL'yi almak için HttpContext kullanın
        //            var requestedUrl = context.Request.Url.AbsolutePath;

        //            // Giriş gerektiren sayfalar
        //            var protectedPages = new List<string> { "/Account/Login", "/Account/Register" };

        //            // Eğer istenen sayfa korunan sayfalardan biriyse
        //            if (protectedPages.Contains(requestedUrl))
        //            {
        //                // Kullanıcı giriş yapmadıysa InvalidPage sayfasına yönlendir
        //                context.Response.Redirect("~/Account/InvalidPage");
        //            }
        //        }
        //    }
        //}



        //protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        //{
        //    if (Context.Request.IsAuthenticated)
        //    {
        //        // Kullanıcı giriş yapmış
        //        return;
        //    }

        //    var path = Context.Request.Url.AbsolutePath.ToLower();

        //    // Giriş yapılmamışsa ve InvalidPage'e yönlendirilmemişse
        //    if (path != "/account/login" && path != "/account/invalidpage" && !path.StartsWith("/content"))
        //    {
        //        Context.Response.Redirect("~/Account/InvalidPage");
        //    }
        //}

        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    var exception = Server.GetLastError();
        //    var httpException = exception as HttpException;

        //    if (httpException != null)
        //    {
        //        var httpCode = httpException.GetHttpCode();

        //        if (httpCode == 401 || httpCode == 403 || httpCode == 404 || httpCode == 203)
        //        {
        //            Server.ClearError();
        //            Response.Redirect("~/Account/InvalidPage");
        //        }
        //    }
        //}

        //protected void Application_Error(object sender, EventArgs e)
        //{
        //    var exception = Server.GetLastError();
        //    var httpException = exception as HttpException;

        //    if (httpException != null)
        //    {
        //        var httpCode = httpException.GetHttpCode();

        //        // Kullanıcının kimlik doğrulaması yapılıp yapılmadığını kontrol et
        //        if (!HttpContext.Current.User.Identity.IsAuthenticated)
        //        {
        //            if (httpCode == 401 || httpCode == 403 || httpCode == 404 || httpCode == 203)
        //            {
        //                Server.ClearError();
        //                Response.Redirect("~/Account/InvalidPage");
        //            }
        //        }
        //        else
        //        {
        //            // Kullanıcı giriş yapmışsa, başarılı bir yönlendirme yap
        //            Server.ClearError();
        //            Response.Redirect("~/Home/Index"); // Giriş yaptıktan sonra yönlendirilecek sayfa
        //        }
        //    }
        //}



        //protected void Application_EndRequest(Object sender, EventArgs e)
        //{
        //    if (Context.Response.StatusCode == 403 || Context.Response.StatusCode == 404 || Context.Response.StatusCode == 203 || Context.Response.StatusCode == 401)
        //    {
        //        // Hata durumunda yönlendirme
        //        if (!Context.Request.Url.AbsolutePath.Equals("/Account/InvalidPage", StringComparison.OrdinalIgnoreCase))
        //        {
        //            Response.Clear();
        //            Response.Redirect("~/Account/InvalidPage");
        //            Response.End();
        //        }
        //    }
        //}

        //Beginrequest?? kullanılabilir belki

        //protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        //{
        //    if (Context.Response.StatusCode == 403 || Context.Response.StatusCode == 404 || Context.Response.StatusCode == 203 || Context.Response.StatusCode == 401)
        //    {
        //        // Hata durumunda yönlendirme
        //        if (!Context.Request.Url.AbsolutePath.Equals("/Account/InvalidPage", StringComparison.OrdinalIgnoreCase))
        //        {
        //            Response.Clear();
        //            Response.Redirect("~/Account/InvalidPage");
        //            Response.End();
        //        }
        //    }
        //}



        //protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        //{
        //    // Kullanıcı kimlik doğrulaması mevcut mu kontrol edin
        //    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
        //    {
        //        return; // Kullanıcı kimlik doğrulaması başarılı, devam et
        //    }

        //    // Kullanıcı kimlik doğrulamadıysa, yönlendirme işlemini yapın
        //    var url = Context.Request.Url.AbsolutePath;

        //    // Özel hata sayfası için doğrudan erişim kontrolü
        //    if (url != "/Account/Login" && !url.StartsWith("/Content") && !url.StartsWith("/Scripts") && !url.StartsWith("/Account/InvalidPage"))
        //    {
        //        Response.Redirect("~/Account/InvalidPage");
        //        Response.End();
        //    }
        //}



        //protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        //{
        //    var url = Context.Request.Url.AbsolutePath;

        //    Kullanıcı kimlik doğrulaması mevcut mu kontrol edin
        //    if (HttpContext.Current.User != null && HttpContext.Current.User.Identity.IsAuthenticated)
        //    {
        //        Kimlik doğrulaması yapılmış kullanıcılar için yönlendirme yapma
        //        if (url.Equals("/Account/InvalidPage", StringComparison.OrdinalIgnoreCase))
        //        {
        //            Response.Redirect("~/Home/Index"); // Örneğin anasayfaya yönlendir
        //            Response.End();
        //        }
        //        return; // Kimlik doğrulaması başarılı, devam et
        //    }

        //    Kullanıcı kimlik doğrulamadıysa, yönlendirme işlemini yapın
        //    if (url != "/Account/Login" && !url.StartsWith("/Content") && !url.StartsWith("/Scripts") && !url.StartsWith("/Account/InvalidPage"))
        //    {
        //        Response.Redirect("~/Account/InvalidPage");
        //        Response.End();
        //    }
        //}



    }


}
