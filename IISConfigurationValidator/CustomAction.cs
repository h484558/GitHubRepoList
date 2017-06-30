using System;
using Microsoft.Deployment.WindowsInstaller;
using System.Text.RegularExpressions;
using System.Windows;
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

            string appName = session["APP_NAME"];
            string appPort = session["APP_PORT"];
            string appPool = session["APP_POOL"];
            string appHost = session["APP_HOSTNAME"];

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

            if (port < 1 || port > 65535)
            {
                MessageBox.Show("Invalid port value: '" + port + "'! Valid port range is 1-65535.", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
                session["IIS_CONFIGURATION_SUCCESS"] = "0";
                return ActionResult.Success;
            }

            // Check hostname
            if (Uri.CheckHostName(appHost) == UriHostNameType.Unknown)
            {
                MessageBox.Show("Invalid Hostname: '" + appHost + "'!", "Invalid IIS Configuration", MessageBoxButton.OK, MessageBoxImage.Error);
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

            session["WEB_URL"] = string.Format("http://{0}:{1}/", session["APP_HOSTNAME"], port);
            session["IIS_CONFIGURATION_SUCCESS"] = "1";
            return ActionResult.Success;
        }
    }
}
