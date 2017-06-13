using GitHubRepoList.App_Start;
using GitHubRepoList.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace GitHubRepoList
{
    public class AuthenticationHeader : SoapHeader
    {
        public string Login;
        public string Password;
    }

    public class UserInfo : IDisposable
    {
        private User user;
        RepoDBContext db = new RepoDBContext();

        public UserInfo(string login)
        {
            user = db.Users.Where(u => u.login == login).FirstOrDefault();
        }

        public bool CanRead()
        {
            if ((user == null) || (!user.is_read && !user.is_admin)) return false;
            return true;
        }

        public bool CanWrite()
        {
            if ((user == null) || (!user.is_write && !user.is_admin)) return false;
            return true;
        }

        public bool IsAdmin()
        {
            if (user == null || !user.is_admin) return false;
            return true;
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }

    /// <summary>
    /// Summary description for GitHubRepoListService
    /// </summary>
    [WebService(Namespace = "http://githubrepolist.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class GitHubRepoListService : System.Web.Services.WebService
    {
        RepoDBContext db = new RepoDBContext();
        public AuthenticationHeader AuthInfo;

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Authenticate()
        {
            var user = db.Users.Where(u => u.login == AuthInfo.Login).Where(u => u.password == AuthInfo.Password).FirstOrDefault();
            if (user == null)
            {
                return string.Empty;
            }
            return new JavaScriptSerializer().Serialize(new { Login = user.login,
                Password = user.password,
                IsRead = user.is_read,
                IsWrite = user.is_write,
                IsAdmin = user.is_admin });
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetRepos()
        {
            if (!(new UserInfo(AuthInfo.Login).CanRead()))
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Insufficient privileges!" });
            }

            var repos = db.Repos.Include("owner").OrderBy(r => r.sort_position);
            var json = new JavaScriptSerializer().Serialize(repos);
            
            return json;
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetRepoDetails(int id)
        {
            if (!(new UserInfo(AuthInfo.Login).CanRead()))
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Insufficient privileges!" });
            }

            var repo = db.Repos.Include("owner").Where(r => r.id == id).FirstOrDefault();
            var json = new JavaScriptSerializer().Serialize(repo);
            return json;
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CreateRepo(string newRepoJson)
        {
            try
            {
                Repo newRepo = new JavaScriptSerializer().Deserialize<Repo>(newRepoJson);
                db.Repos.Add(newRepo);
                db.SaveChanges();
                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = ex.Message });
            }
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CreateRepos(string newRepoJson)
        {
            try
            {
                Repo[] newRepos = new JavaScriptSerializer().Deserialize<Repo[]>(newRepoJson);
                db.Repos.AddRange(newRepos);
                db.SaveChanges();
                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = ex.Message });
            }
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EditRepo(string repoJson)
        {
            if (!(new UserInfo(AuthInfo.Login).CanWrite()))
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Insufficient privileges!" });
            }
            try
            {
                Repo repo = new JavaScriptSerializer().Deserialize<Repo>(repoJson);
                db.Repos.Attach(repo);
                db.Entry(repo).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = ex.Message });
            }
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeleteRepos(int[] ids)
        {
            if (!(new UserInfo(AuthInfo.Login).CanWrite()))
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Insufficient privileges!" });
            }
            try
            {
                var reposToRemove = db.Repos.Include("owner").Where(r => ids.Contains(r.id));

                if (reposToRemove != null)
                {
                    foreach (var repo in reposToRemove)
                    {
                        if (repo.owner != null)
                        {
                            db.RepoOwners.Remove(repo.owner);
                        }
                    }

                    db.Repos.RemoveRange(reposToRemove);
                    db.SaveChanges();

                    return new JavaScriptSerializer().Serialize(new { status = "OK" });
                }

                Logger.WriteLog("Can't find selected repos: " + string.Join(",", ids), LoggerLevel.INFO);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Can't find selected repos!" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = ex.Message });
            }
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ImportReposFromGitHub(string login)
        {
            if (!(new UserInfo(AuthInfo.Login).CanWrite()))
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Insufficient privileges!" });
            }
            try
            {
                string html = string.Empty;
                string url = "https://api.github.com/users/" + login + "/repos";

                Repo[] repos = null;

                var client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                var json = client.DownloadString(url);

                repos = new JavaScriptSerializer().Deserialize<Repo[]>(json);

                db.Repos.AddRange(repos);
                db.SaveChanges();

                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = ex.Message });
            }
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetUsers()
        {
            if (!(new UserInfo(AuthInfo.Login).IsAdmin()))
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Insufficient privileges!" });
            }
            var users = db.Users;
            var json = new JavaScriptSerializer().Serialize(users);
            return json;
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EditUser(string userJson)
        {
            if (!(new UserInfo(AuthInfo.Login).IsAdmin()))
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Insufficient privileges!" });
            }
            try
            {
                User user = new JavaScriptSerializer().Deserialize<User>(userJson);
                db.Users.Attach(user);
                db.Entry(user).State = System.Data.Entity.EntityState.Modified;
                db.Entry(user).Property(u => u.password).IsModified = false;
                if (db.Users.Local.Where(u => u.is_admin == true).Count() < 1)
                {
                    return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "There should be at least one admin user!" });
                }
                else
                {
                    db.SaveChanges();
                }

                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = ex.Message });
            }
        }
    }
}
