using GitHubRepoList.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GitHubRepoList.Controllers
{
    public class AdminRightsRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["user"] == null)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                return;
            }

            var sessionUser = (User) filterContext.HttpContext.Session["user"];

            if (sessionUser == null || !sessionUser.is_admin)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }
    }
}