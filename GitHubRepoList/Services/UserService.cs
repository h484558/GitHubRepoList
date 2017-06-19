using GitHubRepoList.App_Start;
using GitHubRepoList.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace GitHubRepoList.Services
{
    public class UserService
    {
        private AuthenticationHeader _authInfo;
        private List<SqlParameter> _authSqlParams;
        private RepoDBContext _db;
        private string _exceptionMessage;

        public UserService(AuthenticationHeader authInfo)
        {
            _authInfo = authInfo ?? new AuthenticationHeader { Login = "limited", Password = "limited" };
            _authSqlParams = new List<SqlParameter>
            {
                new SqlParameter("@login", _authInfo.Login),
                new SqlParameter("@password", _authInfo.Password)
            };
            _db = new RepoDBContext();
        }

        public User AuthUser()
        {
            return _db.Database.SqlQuery<User>("EXEC AuthUser @login, @password", _authSqlParams.ToArray()).FirstOrDefault();
        }

        public string AuthUserJson()
        {
            var user = AuthUser();
            if (user == null)
                return string.Empty;
            else
                return new JavaScriptSerializer().Serialize(new { Login = user.login,
                    Password = user.password,
                    IsRead = user.is_read,
                    IsWrite = user.is_write,
                    IsAdmin = user.is_admin });
        }

        public List<User> GetUsers()
        {
            return _db.Database.SqlQuery<User>("EXEC GetUsers @login, @password", _authSqlParams.ToArray()).ToList();
        }

        public string GetUsersJson()
        {
            return new JavaScriptSerializer().Serialize(GetUsers());
        }

        public bool UpdateUser(User user)
        {
            try
            {
                var columns = typeof(User).GetProperties().Select(property => property.Name).ToArray();

                foreach (string column in columns)
                {
                    if (column == "id" || column == "password")
                        continue;

                    List<SqlParameter> editUserParams = new List<SqlParameter>
                    {
                        new SqlParameter("@login", _authInfo.Login),
                        new SqlParameter("@password", _authInfo.Password),
                        new SqlParameter("@user_id", user.id),
                        new SqlParameter("@column_name", column),
                        new SqlParameter("@new_value", typeof(User).GetProperty(column).GetValue(user) ?? "")
                    };

                    _db.Database.ExecuteSqlCommand("EXEC UpdateUser @login, @password, @user_id, @column_name, @new_value", editUserParams.ToArray());
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

        public string UpdateUserJson(string userJson)
        {
            User userToUpdate = null;

            try
            {
                userToUpdate = new JavaScriptSerializer().Deserialize<User>(userJson);
            }
            catch (Exception ex)
            {
                Logger.WriteLog(ex);
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = "Failed to parse JSON!" });
            }

            if (UpdateUser(userToUpdate))
                return new JavaScriptSerializer().Serialize(new { status = "OK" });
            else
                return new JavaScriptSerializer().Serialize(new { status = "Fail", message = _exceptionMessage });
        }
    }
}