using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Text.RegularExpressions;
using System.Windows;
using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.Net;
using Microsoft.Web.Administration;

namespace IISConfigurationValidator
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ValidateIISConfiguration(Session session)
        {
            session.Log("Begin ValidateIISConfiguration");

            string appName = session["APP_NAME"];
            string appPort = session["APP_PORT"];
            string appPool = session["APP_POOL"];

            // Check app pool and app name
            Regex appNameAndPoolRegex = new Regex(@"^\w[\w\s]+$");
            if (!appNameAndPoolRegex.IsMatch(appPool))
            {
                MessageBox.Show("Invalid Application Pool!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                session["IIS_CONFIGURATION_SUCCESS"] = "0";
                return ActionResult.Success;
            }
            else if (!appNameAndPoolRegex.IsMatch(appName))
            {
                MessageBox.Show("Invalid Application Name!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                session["IIS_CONFIGURATION_SUCCESS"] = "0";
                return ActionResult.Success;
            }

            int port = 0;

            try
            {
                port = int.Parse(appPort);
            }
            catch (Exception)
            {
                MessageBox.Show("Invalid port value: '" + appPort + "'!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                session["IIS_CONFIGURATION_SUCCESS"] = "0";
                return ActionResult.Success;
            }

            // Check if port is free
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipGlobalProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    MessageBox.Show("Port '" + port + "' is currently in use!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                    session["IIS_CONFIGURATION_SUCCESS"] = "0";
                    return ActionResult.Success;
                }
            }

            // Check if there is no site with such name
            var iisManager = new ServerManager();
            SiteCollection sites = iisManager.Sites;

            foreach (var site in sites)
            {
                if (site.Name == appName)
                {
                    MessageBox.Show("Site '" + appName + "' already exists! Please choose another site name.", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                    session["IIS_CONFIGURATION_SUCCESS"] = "0";
                    return ActionResult.Success;
                }
            }


            session["IIS_CONFIGURATION_SUCCESS"] = "1";
            return ActionResult.Success;
        }
    }
}
