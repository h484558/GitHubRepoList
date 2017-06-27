using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Text.RegularExpressions;
using System.Windows;
using System.DirectoryServices;
using System.Net.NetworkInformation;
using System.Net;

namespace IISConfigurationValidator
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ValidateIISConfiguration(Session session)
        {
            session.Log("Begin ValidateIISConfiguration");

            // Check app pool and app name
            Regex appNameAndPoolRegex = new Regex(@"^\w+$");
            if (!appNameAndPoolRegex.IsMatch(session["APP_POOL"]))
            {
                MessageBox.Show("Invalid Application Pool!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                session["IIS_CONFIGURATION_SUCCESS"] = "0";
                return ActionResult.Success;
            }
            else if (!appNameAndPoolRegex.IsMatch(session["APP_NAME"]))
            {
                MessageBox.Show("Invalid Application Name!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                session["IIS_CONFIGURATION_SUCCESS"] = "0";
                return ActionResult.Success;
            }

            int port = 0;

            try
            {
                port = int.Parse(session["APP_PORT"]);
            }
            catch (FormatException)
            {
                MessageBox.Show("Port '" + port + "' is currently in use!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
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

            session["IIS_CONFIGURATION_SUCCESS"] = "1";
            return ActionResult.Success;
        }
    }
}
