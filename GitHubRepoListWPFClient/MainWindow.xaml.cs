using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GitHubRepoList.Models;
using System.Web.Script.Serialization;
using System.Web.Services.Protocols;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace GitHubRepoListWPFClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GitHubRepoListService.GitHubRepoListService repoService = new GitHubRepoListService.GitHubRepoListService();
        GitHubRepoListService.AuthenticationHeader authHeader;
        AuthenticationDetails authDetails;

        class ServiceResponse
        {
            public string status;
            public string message;
        }

        public MainWindow()
        {
            InitializeComponent();
            AuthenticaticateUser();

            if (authDetails == null)
            {
                Application.Current.Shutdown();
                return;
            }

            if (!authDetails.IsAdmin)
            {
                userTab.Visibility = Visibility.Collapsed;
            }

            if (authDetails.IsRead || authDetails.IsAdmin)
            {
                repoDataGrid.ItemsSource = new JavaScriptSerializer().Deserialize<Repo[]>(repoService.GetRepos());
            } else
            {
                this.Close();   // Close MainWindow if we have no permission to read
                MessageBoxResult createNewRepo = MessageBox.Show("You have no permission to read! Do you wish to create a new repo?", "Insufficient Privileges", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (createNewRepo == MessageBoxResult.Yes)
                {
                    // Open new repo form
                }
            }

            if (!authDetails.IsWrite && !authDetails.IsAdmin)
            {
                importMenuItem.Visibility = Visibility.Collapsed;
                deleteSelectedMenuItem.Visibility = Visibility.Collapsed;
                manageSeparator.Visibility = Visibility.Collapsed;
            }

            if (!authDetails.IsRead && !authDetails.IsAdmin)
            {
                exportMenuItem.Visibility = Visibility.Collapsed;
            }

            // Async callbacks
            repoService.ImportReposFromGitHubCompleted += RepoService_ImportReposFromGitHubCompleted;
            repoService.CreateRepoCompleted += RepoService_CreateRepoCompleted;
            repoService.CreateReposCompleted += RepoService_CreateReposCompleted;
            repoService.EditRepoCompleted += RepoService_EditRepoCompleted;
            repoService.DeleteReposCompleted += RepoService_DeleteReposCompleted;
            // processRunningProgressBar.
        }

        private void HandleServiceResponse(string eventArgsResult, string successMessage, bool skipUpdateDatagrid = false)
        {
            var serviceResponse = new JavaScriptSerializer().Deserialize<ServiceResponse>(eventArgsResult);
            if (serviceResponse.status == "OK")
            {
                UpdateDataGrid();
                StopProcessVisualisation(successMessage);
            }
            else if (serviceResponse.status == "Fail" && serviceResponse.message != null)
            {
                StopProcessVisualisation(serviceResponse.message, true);
            }
            else
            {
                StopProcessVisualisation("Unexpected response", true);
                MessageBox.Show("Unexpected server response! Move on.", "Unexpected response", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (!skipUpdateDatagrid)
                UpdateDataGrid();
        }

        private void RepoService_DeleteReposCompleted(object sender, GitHubRepoListService.DeleteReposCompletedEventArgs e)
        {
            HandleServiceResponse(e.Result, "Selected entries were successfully deleted!");
        }

        private void RepoService_EditRepoCompleted(object sender, GitHubRepoListService.EditRepoCompletedEventArgs e)
        {
            // No need to update datagrid here
            HandleServiceResponse(e.Result, "Changes saved!", true);
        }

        private void RepoService_CreateReposCompleted(object sender, GitHubRepoListService.CreateReposCompletedEventArgs e)
        {
            HandleServiceResponse(e.Result, "New repos created successfully!");
        }

        private void RepoService_CreateRepoCompleted(object sender, GitHubRepoListService.CreateRepoCompletedEventArgs e)
        {
            HandleServiceResponse(e.Result, "New repo created successfully!");
        }

        private void RepoService_ImportReposFromGitHubCompleted(object sender, GitHubRepoListService.ImportReposFromGitHubCompletedEventArgs e)
        {
            HandleServiceResponse(e.Result, "Import finished successfully!");
        }

        private void AuthenticaticateUser()
        {
            AuthWindow aw = new AuthWindow();
            if (aw.ShowDialog() == true)
            {
                authDetails = aw.AuthDetails;
                authHeader = new GitHubRepoListService.AuthenticationHeader { Login = aw.AuthDetails.Login, Password = aw.AuthDetails.Password };
                repoService.AuthenticationHeaderValue = authHeader;
            }
            else
            {
                Application.Current.Shutdown();
                return;
            }
        }

        private void UpdateDataGrid()
        {
            repoDataGrid.ItemsSource = null;
            repoDataGrid.ItemsSource = new JavaScriptSerializer().Deserialize<Repo[]>(repoService.GetRepos());
        }

        private void StartProcessVisualisation(string message)
        {
            currentProcessLabel.Content = message;
            processRunningProgressBar.IsIndeterminate = true;
        }

        private void StopProcessVisualisation(string message, bool error = false)
        {
            currentProcessLabel.Content = message;
            processRunningProgressBar.IsIndeterminate = false;
            processRunningProgressBar.Value = 100;
            if (error)
                processRunningProgressBar.Foreground = Brushes.Red;
            else
                processRunningProgressBar.Foreground = Brushes.Green;
        }

        private void logOutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            new MainWindow().Show();
            this.Close();
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void importFromGitHubMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var gitHubImportDialog = new ImportFromGitHubWindow();
            if (gitHubImportDialog.ShowDialog() == true)
            {
                StartProcessVisualisation(string.Format("Importing repos from http://github.com/{0}...", gitHubImportDialog.GitHubUsername));
                repoService.ImportReposFromGitHubAsync(gitHubImportDialog.GitHubUsername);
            }
        }

        private void importFromJsonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog jsonFileDialog = new OpenFileDialog();
            if(jsonFileDialog.ShowDialog() == false)
            {
                return;
            }

            StartProcessVisualisation(string.Format("Importing repos from: {0}", jsonFileDialog.FileName));

            Stream jsonFileStream = jsonFileDialog.OpenFile();

            string newReposJsonString;
            using (StreamReader reader = new StreamReader(jsonFileStream, Encoding.UTF8))
            {
                newReposJsonString = reader.ReadToEnd();
            }

            Repo[] newRepos;

            try
            {
                newRepos = new JavaScriptSerializer().Deserialize<Repo[]>(newReposJsonString);
            } catch (ArgumentException)
            {
                StopProcessVisualisation("Ready");
                MessageBox.Show("Failed to parse JSON file!", "Invalid JSON file", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            repoService.CreateReposAsync(new JavaScriptSerializer().Serialize(newRepos));
        }

        private void exportToJsonMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveJsonFileDialog = new SaveFileDialog();
            saveJsonFileDialog.Filter = "JSON file (*.json)|*.json";
            saveJsonFileDialog.FileName = "repos.json";

            if (saveJsonFileDialog.ShowDialog() == true)
            {
                StartProcessVisualisation(string.Format("Exporting repos to: {0}", saveJsonFileDialog.FileName));
                using (StreamWriter writer = new StreamWriter(saveJsonFileDialog.OpenFile()))
                {
                    writer.Write(repoService.GetRepos());
                }
            }
            StopProcessVisualisation(string.Format("All repos have been exported to: {0}", saveJsonFileDialog.FileName));
            MessageBox.Show("Export finished successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void deleteSelectedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all selected entries?", "Delete Selected?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }
            StartProcessVisualisation("Deleting selected repos...");
            Repo[] repos = repoDataGrid.SelectedItems.Cast<Repo>().ToArray();
            List<int> idsToDelete = new List<int>();
            foreach (var repo in repos)
            {
                idsToDelete.Add(repo.id);
            }
            // repoService.DeleteRepos(idsToDelete.ToArray());
            repoService.DeleteReposAsync(idsToDelete.ToArray());
            // UpdateDataGrid();
        }

        private void repoDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            StartProcessVisualisation("Updating entry...");
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;    // column name
                    dynamic newValue = null;   // This might be string or boolean depending on editingElement type

                    if (e.EditingElement.GetType() == typeof(TextBox))
                    {
                        newValue = (e.EditingElement as TextBox).Text;
                    } else if (e.EditingElement.GetType() == typeof(CheckBox))
                    {
                        newValue = (e.EditingElement as CheckBox).IsChecked;
                    } else
                    {
                        MessageBox.Show("Can't edit this type of field", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var editedRepo = e.Row.Item as Repo;
                    Type repoType = typeof(Repo);
                    PropertyInfo repoPropertyInfo = repoType.GetProperty(bindingPath);
                    try
                    {
                        dynamic convertedPropertyValue = Convert.ChangeType(newValue, repoPropertyInfo.PropertyType);
                        repoPropertyInfo.SetValue(editedRepo, convertedPropertyValue);
                    } catch (FormatException ex)
                    {
                        MessageBox.Show(ex.Message, "Error saving new value", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                    string serializedRepo = new JavaScriptSerializer().Serialize(editedRepo);
                    repoService.EditRepoAsync(new JavaScriptSerializer().Serialize(editedRepo));
                }
            }
        }
    }
}
