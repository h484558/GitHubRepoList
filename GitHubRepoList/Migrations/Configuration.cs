namespace GitHubRepoList.Migrations
{
    using Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GitHubRepoList.Models.RepoDBContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(GitHubRepoList.Models.RepoDBContext context)
        {
            context.Users.AddOrUpdate(
                u => u.login,
                new User { login = "admin", password = "admin", is_admin = true, is_write = false, is_read = false },
                new User { login = "guest", password = "guest", is_admin = false, is_write = true, is_read = true },
                new User { login = "limited", password = "limited", is_admin = false, is_write = false, is_read = false },
                new User { login = "ro", password = "ro", is_admin = false, is_write = false, is_read = true }
            );

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
