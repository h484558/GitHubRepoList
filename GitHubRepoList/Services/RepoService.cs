using GitHubRepoList.App_Start;
using GitHubRepoList.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Script.Serialization;

namespace GitHubRepoList.Services
{
    public class RepoService
    {
        private AuthenticationHeader _authInfo;
        private List<SqlParameter> _authSqlParams;
        private RepoDBContext _db;
        private string _exceptionMessage;

        public RepoService(AuthenticationHeader authInfo)
        {
            _authInfo = authInfo ?? new AuthenticationHeader { Login = Resources.DefaultAuthHeaderLogin, Password = Resources.DefaultAuthHeaderPassword };
            _authSqlParams = new List<SqlParameter>
            {
                new SqlParameter("@login", _authInfo.Login),
                new SqlParameter("@password", _authInfo.Password)
            };
            _db = new RepoDBContext();
        }

        public List<Repo> GetRepos()
        {
            return _db.Database.SqlQuery<Repo>("EXEC GetRepos @login, @password", _authSqlParams.ToArray()).ToList();
        }

        public string GetReposJson()
        {
            return new JavaScriptSerializer().Serialize(GetRepos());
        }

        public bool CreateRepo(Repo newRepo)
        {
            try
            {
                List<SqlParameter> newRepoParams = new List<SqlParameter>
                {
                    new SqlParameter("@name", newRepo.name ?? ""),
                    new SqlParameter("@full_name", newRepo.full_name ?? ""),
                    new SqlParameter("@description", newRepo.description ?? ""),
                    new SqlParameter("@url", newRepo.url ?? ""),
                    new SqlParameter("@created_at", newRepo.created_at ?? "")
                };
                
                _db.Database.ExecuteSqlCommand("EXEC CreateRepo @name, @full_name, @description, @url, @created_at", newRepoParams.ToArray());
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                _exceptionMessage = ex.Message;
                return false;
            }
        }

        public string CreateRepoFromJson(string newRepoJson)
        {
            Repo newRepo = null;

            try
            {
                newRepo = new JavaScriptSerializer().Deserialize<Repo>(newRepoJson);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Failed to parse JSON!" });
            }

            if (CreateRepo(newRepo))
                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            else
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = _exceptionMessage });
        }

        public bool CreateRepos(Repo[] newRepos)
        {
            foreach (var repo in newRepos)
            {
                if (!CreateRepo(repo))
                {
                    return false;
                }
            }

            return true;
        }

        public string CreateReposFromJson(string newReposJson)
        {
            Repo[] newRepos = null;

            try
            {
                newRepos = new JavaScriptSerializer().Deserialize<Repo[]>(newReposJson);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Failed to parse JSON!" });
            }

            if (CreateRepos(newRepos))
                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            else
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = _exceptionMessage });
        }


        public bool UpdateRepo(Repo repo)
        {
            try
            {
                var columns = typeof(Repo).GetProperties().Select(property => property.Name).ToArray();

                foreach (string column in columns)
                {
                    if (column == "id" || column == "owner")
                        continue;

                    List<SqlParameter> editRepoParams = new List<SqlParameter>
                    {
                        new SqlParameter("@login", _authInfo.Login),
                        new SqlParameter("@password", _authInfo.Password),
                        new SqlParameter("@repo_id", repo.id),
                        new SqlParameter("@column_name", column),
                        new SqlParameter("@new_value", typeof(Repo).GetProperty(column).GetValue(repo) ?? "")
                    };

                    _db.Database.ExecuteSqlCommand("EXEC UpdateRepo @login, @password, @repo_id, @column_name, @new_value", editRepoParams.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                _exceptionMessage = ex.Message;
                return false;
            }
        }

        public string UpdateRepoJson(string repoJson)
        {
            Repo repoToUpdate = null;

            try
            {
                repoToUpdate = new JavaScriptSerializer().Deserialize<Repo>(repoJson);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Failed to parse JSON!" });
            }

            if (UpdateRepo(repoToUpdate))
                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            else
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = _exceptionMessage });
        }

        public bool DeleteRepos(int[] ids)
        {
            try
            {
                foreach (int id in ids)
                {
                    List<SqlParameter> deleteRepoParams = new List<SqlParameter>
                    {
                        new SqlParameter("@login", _authInfo.Login),
                        new SqlParameter("@password", _authInfo.Password),
                        new SqlParameter("@repo_id", id)
                    };

                    _db.Database.ExecuteSqlCommand("EXEC DeleteRepo @login, @password, @repo_id", deleteRepoParams.ToArray());
                }
               
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                _exceptionMessage = ex.Message;
                return false;
            }
        }

        public string DeleteReposJson(int[] ids)
        {
            if (DeleteRepos(ids))
            {
                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            } else
            {
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = _exceptionMessage });
            }
        }

    }
}