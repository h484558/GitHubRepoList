using System;
using System.Data.Entity;

namespace GitHubRepoList.Models
{
    [Serializable]
    public class Repo
    {
        public int id { get; set;  }

        public string name { get; set; }

        public string full_name { get; set; }

        public RepoOwner owner { get; set; }

        public bool @private { get; set; }

        public string html_url { get; set; }

        public string description { get; set; }

        public bool fork { get; set; }

        public string url { get; set; }

        public string forks_url { get; set; }

        public string created_at { get; set; }

        public string updated_at { get; set; }

        public string git_url { get; set; }

        public int sort_position { get; set; }
    }

    public class RepoDBContext : DbContext
    {
        public RepoDBContext() {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<RepoDBContext, Migrations.Configuration>());
        }

        public DbSet<Repo> Repos { get; set; }

        public DbSet<RepoOwner> RepoOwners { get; set; }

        public DbSet<User> Users { get; set; }
    }
}