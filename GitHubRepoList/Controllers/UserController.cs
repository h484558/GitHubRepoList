using GitHubRepoList.App_Start;
using GitHubRepoList.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace GitHubRepoList.Controllers
{
    public class UserController : Controller
    {
        private RepoDBContext db = new RepoDBContext();

        // POST: /User/SignIn

        [HttpPost]
        public ActionResult SignIn(string login, string password)
        {
            var user = db.Users.Where(u => u.login == login).FirstOrDefault();

            if (user == null || user.password != password)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            Session.Add("user", user);

            HttpCookie loginInfo = new HttpCookie("login_info");
            loginInfo.Value = new JavaScriptSerializer().Serialize(
                    new { login = user.login,
                          is_admin = user.is_admin,
                          is_write = user.is_write,
                          is_read = user.is_read
                    }
                );
            loginInfo.Expires = DateTime.Now.AddDays(7);

            Response.Cookies.Add(loginInfo);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // POST: /User/SignOut

        [HttpPost]
        public ActionResult SignOut()
        {
            // Remove login_info cookie
            if (Request.Cookies["login_info"] != null)
            {
                var loginInfoCookie = new HttpCookie("login_info");
                loginInfoCookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(loginInfoCookie);
            }

            Session.Clear();
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // GET: /User
        [AdminRightsRequired]
        public ActionResult Index()
        {
            var users = db.Users;
            var json = new JavaScriptSerializer().Serialize(users);
            return Content(json);
        }

        // POST: /User/Edit/Id
        [AdminRightsRequired]
        public ActionResult Edit(int id, User user)
        {
            try
            {
                db.Users.Attach(user);
                db.Users.Load();
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.Entry(user).Property(u => u.password).IsModified = false;
                
                if (db.Users.Local.Where(u => u.is_admin == true).Count() < 1)
                {
                    return Json(new { status = "Fail", message = "There should be at least one admin user!" });
                } else
                {
                    db.SaveChanges();
                }

                return Json(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return Json(new { status = "Fail" , message = ex.Message});
            }
        }
    }
}