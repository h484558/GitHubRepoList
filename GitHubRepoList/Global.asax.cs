using GitHubRepoList.App_Start;
using GitHubRepoList.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace GitHubRepoList
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            RepoDBContext dbContext = new RepoDBContext();
            dbContext.Database.Initialize(true);
        }

        void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            if (exception == null) return;
            Logger.WriteLog(exception);
            Server.ClearError();
        }
    }
}
