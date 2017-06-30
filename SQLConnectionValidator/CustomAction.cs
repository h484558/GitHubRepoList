using System;
using Microsoft.Deployment.WindowsInstaller;
using System.Data.SqlClient;
using System.Windows;

namespace SQLConnectionValidator
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ValidateSqlConnectionServer(Session session)
        {
            session.Log("Begin ValidateSqlConnectionServer");

            string dbUser = session["DB_USER"];
            string dbPassword = session["DB_PASSWORD"];
            string dbServer = session["DB_SERVER"];
            string dbDatabase = session["DB_DATABASE"];
            string rawConnectionString = "Data Source={0};Network Library=DBMSSOCN;Initial Catalog='master';User ID={2};Password={3};";

            SqlConnection sqlConnection = new SqlConnection(string.Format(rawConnectionString, dbServer, dbDatabase, dbUser, dbPassword));

            try
            {
                sqlConnection.Open();

                if (new SqlCommand("SELECT db_id('" + dbDatabase + "')", sqlConnection).ExecuteScalar() != DBNull.Value)
                {
                    MessageBoxResult existingDbDlgResult = MessageBox.Show("Database '" + dbDatabase + "' already exists! Do you want to use it anyway?", "Database Exists", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (existingDbDlgResult == MessageBoxResult.No)
                    {
                        session["DB_CONNECTION_SUCCESS"] = "0";
                        return ActionResult.Success;
                    }
                }
                else
                {
                   
                    try
                    {
                        new SqlCommand("CREATE DATABASE " + dbDatabase + "; DROP DATABASE " + dbDatabase + "", sqlConnection).ExecuteScalar();
                    }
                    catch (SqlException)
                    {
                        session["DB_CONNECTION_SUCCESS"] = "0";
                        MessageBox.Show("Invalid DB name!", "Invalid DB Name", MessageBoxButton.OK, MessageBoxImage.Error);
                        return ActionResult.Success;
                    }
                    
                }                
            } catch (SqlException)
            {
                session["DB_CONNECTION_SUCCESS"] = "0";
                MessageBox.Show("Failed to connect to DB Server!", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return ActionResult.Success;
            }

            session["DB_CONNECTION_SUCCESS"] = "1";
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult ValidateSqlConnectionClient(Session session)
        {

            session.Log("Begin ValidateSqlConnectionServer");

            string dbUser = session["DB_USER"];
            string dbPassword = session["DB_PASSWORD"];
            string dbServer = session["DB_SERVER"];
            string dbDatabase = session["DB_DATABASE"];
            string rawConnectionString = "Data Source={0};Network Library=DBMSSOCN;Initial Catalog='master';User ID={2};Password={3};";

            SqlConnection sqlConnection = new SqlConnection(string.Format(rawConnectionString, dbServer, dbDatabase, dbUser, dbPassword));

            try
            {
                sqlConnection.Open();

                if (new SqlCommand("SELECT db_id('" + dbDatabase + "')", sqlConnection).ExecuteScalar() == DBNull.Value)
                {
                    MessageBox.Show("Can't find DB '" + dbDatabase + "'! Please check spelling and try again.", "Can't connect to DB", MessageBoxButton.OK, MessageBoxImage.Error);
                    session["DB_CONNECTION_SUCCESS"] = "0";
                    return ActionResult.Success;
                }
            }
            catch (SqlException)
            {
                session["DB_CONNECTION_SUCCESS"] = "0";
                MessageBox.Show("Failed to connect to DB Server!", "Connection Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return ActionResult.Success;
            }

            session["DB_CONNECTION_SUCCESS"] = "1";
            return ActionResult.Success;
        }
    }
}
