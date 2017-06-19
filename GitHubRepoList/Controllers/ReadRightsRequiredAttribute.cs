using GitHubRepoList.Models;
using System.Net;
using System.Web.Mvc;

namespace GitHubRepoList.Controllers
{
    public class ReadRightsRequiredAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["user"] == null)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                return;
            }

            var sessionUser = (User) filterContext.HttpContext.Session["user"];

            if (sessionUser == null || !sessionUser.is_read)
            {
                if (sessionUser != null && !sessionUser.is_admin)
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
            }
        }
    }
}