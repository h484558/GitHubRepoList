using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace GitHubRepoList.Models
{
    [Serializable]
    public class User
    {
        public int id { get; set; }

        public string login { get; set; }

        [ScriptIgnore]
        public string password { get; set; }

        public bool is_admin { get; set; }

        public bool is_read { get; set; }

        public bool is_write { get; set; }
    }
}