using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GitHubRepoList.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            // Clear cookies if user is no longer is logged in
            if (Session["user"] == null)
            {
                if (Request.Cookies["login_info"] != null)
                {
                    var loginInfoCookie = new HttpCookie("login_info");
                    loginInfoCookie.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Add(loginInfoCookie);
                }
            }
            return View();
        }
    }
}