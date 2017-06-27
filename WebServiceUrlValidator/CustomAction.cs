using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;
using System.Net.NetworkInformation;
using System.Windows;
using System.Net;

namespace WebServiceUrlValidator
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ValidateWebServiceUrl(Session session)
        {
            session.Log("Begin WebServiceUrlValidator");

            try
            {
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(session["WEB_URL"]);
                request.Method = "HEAD";

                using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
                {

                    if (response == null)
                    {
                        MessageBoxResult unpingableDlgResult = MessageBox.Show("Can't reach host: '" + session["WEB_URL"] + "'!\nMaybe WebService hasn't been installed yet.\nDo you want to use it anyway?", "Can't Reach WebService", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (unpingableDlgResult == MessageBoxResult.No)
                        {
                            session["SERVICE_CONNECTION_SUCCESS"] = "0";
                            return ActionResult.Success;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;

                if (response == null) {
                    MessageBoxResult unreachableDlgResult = MessageBox.Show("Can't reach host: '" + session["WEB_URL"] + "'!\nMaybe WebService hasn't been installed yet.\nDo you want to use it anyway?", "Can't Reach WebService", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (unreachableDlgResult == MessageBoxResult.No)
                    {
                        session["SERVICE_CONNECTION_SUCCESS"] = "0";
                        return ActionResult.Success;
                    }
                }
            }
            catch (Exception)
            {
                MessageBoxResult unreachableDlgResult = MessageBox.Show("Can't reach host: '" + session["WEB_URL"] + "'!\nMaybe WebService hasn't been installed yet.\nDo you want to use it anyway?", "Can't Reach WebService", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (unreachableDlgResult == MessageBoxResult.No)
                {
                    session["SERVICE_CONNECTION_SUCCESS"] = "0";
                    return ActionResult.Success;
                }
            }
           

            session["SERVICE_CONNECTION_SUCCESS"] = "1";
            return ActionResult.Success;
        }
    }
}
