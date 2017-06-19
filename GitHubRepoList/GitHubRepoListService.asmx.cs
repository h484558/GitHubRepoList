using GitHubRepoList.App_Start;
using GitHubRepoList.Models;
using GitHubRepoList.Services;
using System;
using System.Net;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace GitHubRepoList
{
    [WebService(Namespace = "http://githubrepolist.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class GitHubRepoListService : System.Web.Services.WebService
    {
        RepoDBContext db = new RepoDBContext();
        public AuthenticationHeader AuthInfo;

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Authenticate()
        {
            return new UserService(AuthInfo).AuthUserJson();
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetRepos()
        {
            return new RepoService(AuthInfo).GetReposJson();
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CreateRepo(string newRepoJson)
        {
            return new RepoService(AuthInfo).CreateRepoFromJson(newRepoJson);
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string CreateRepos(string newReposJson)
        {
            return new RepoService(AuthInfo).CreateReposFromJson(newReposJson);
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EditRepo(string repoJson)
        {
            return new RepoService(AuthInfo).UpdateRepoJson(repoJson);
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string DeleteRepos(int[] ids)
        {
            return new RepoService(AuthInfo).DeleteReposJson(ids);
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string ImportReposFromGitHub(string login)
        {
            try
            {
                string html = string.Empty;
                string url = string.Format(Resources.GitHubApiListReposUrl, login);

                var client = new WebClient();
                client.Headers.Add("user-agent", Resources.WebClientUserAgent);
                var json = client.DownloadString(url);

                new RepoService(AuthInfo).CreateReposFromJson(json);

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
            return new UserService(AuthInfo).GetUsersJson();
        }

        [WebMethod]
        [SoapHeader("AuthInfo")]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string EditUser(string userJson)
        {
            return new UserService(AuthInfo).UpdateUserJson(userJson);
        }
    }
}
