namespace GitHubRepoList.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_sp_UserOps : DbMigration
    {
        public override void Up()
        {
            Sql(GitHubRepoList.Resources.Create_sp_UserOps);
        }
        
        public override void Down()
        {
            Sql(GitHubRepoList.Resources.Drop_sp_UserOps);
        }
    }
}
