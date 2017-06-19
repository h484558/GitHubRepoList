using System.Web.Services.Protocols;

namespace GitHubRepoList.Services
{
    public class AuthenticationHeader : SoapHeader
    {
        public string Login;
        public string Password;
    }
}