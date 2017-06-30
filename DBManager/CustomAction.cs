using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Windows;

namespace DBManager
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult DropDBOnUninstall(Session session)
        {
            session.Log("Begin DropDBOnUninstall");

            RegistryKey sqlInfo = Registry.CurrentUser.OpenSubKey(@"Software\GHRLSQLInfo", false);

            string dbUser = (string)sqlInfo.GetValue("DB_USER");
            string dbPassword = (string)sqlInfo.GetValue("DB_PASSWORD");
            string dbServer = (string)sqlInfo.GetValue("DB_SERVER");
            string dbDatabase = (string)sqlInfo.GetValue("DB_DATABASE");
            string rawConnectionString = "Data Source={0};Network Library=DBMSSOCN;Initial Catalog='master';User ID={2};Password={3};";

            MessageBoxResult existingDbDlgResult = MessageBox.Show("Do you want to drop '" + dbDatabase + "'?", "Drop DB", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (existingDbDlgResult == MessageBoxResult.Yes)
            {
                SqlConnection sqlConnection = new SqlConnection(string.Format(rawConnectionString, dbServer, dbDatabase, dbUser, dbPassword));

                try
                {
                    sqlConnection.Open();
                    new SqlCommand("DROP DATABASE " + dbDatabase + "", sqlConnection).ExecuteScalar();
                }
                catch (SqlException)
                {
                    MessageBox.Show("Failed to drop DB! Please drop it manually.", "Failed to drop DB", MessageBoxButton.OK, MessageBoxImage.Error);
                    return ActionResult.Success;
                }
            }

            return ActionResult.Success;
        }
    }
}
