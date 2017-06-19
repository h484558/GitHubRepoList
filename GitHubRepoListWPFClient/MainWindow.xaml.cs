using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GitHubRepoList.Models;
using System.Web.Script.Serialization;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Net;

namespace GitHubRepoListWPFClient
{

    public partial class MainWindow : Window
    {
        GitHubRepoListService.GitHubRepoListService repoService = new GitHubRepoListService.GitHubRepoListService();
        GitHubRepoListService.AuthenticationHeader authHeader;
        AuthenticationDetails authDetails;

        class ServiceResponse
        {
            public string status { get; set; }
            public string message { get; set; }
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

            // Suggest repo creation if current user doesn't have read privileges
            if (authDetails.IsRead || authDetails.IsAdmin)
            {
                repoDataGrid.ItemsSource = new JavaScriptSerializer().Deserialize<Repo[]>(repoService.GetRepos());
            } else
            {
                this.Hide();   // Hide MainWindow if we have no permission to read
                SuggestRepoCreation();
            }

            // Show Users table if current user is admin
            if (authDetails.IsAdmin)
            {
                userTab.Visibility = Visibility.Visible;    // Users tab has Collapsed visibility by default
                userDataGrid.ItemsSource = new JavaScriptSerializer().Deserialize<User[]>(repoService.GetUsers());
            }

            // Hide write controls on form
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
            repoService.EditUserCompleted += RepoService_EditUserCompleted;
        }

        private void SuggestRepoCreation(string message = "You have no permission to read! Do you wish to create a new repo?")
        {
            MessageBoxResult createNewRepo = MessageBox.Show(message, "Insufficient Privileges", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (createNewRepo == MessageBoxResult.Yes)
            {
                var newRepoDialog = new NewRepoWindow(authDetails.Login);

                if (newRepoDialog.ShowDialog() == true)
                {
                    var newRepo = newRepoDialog.NewRepo;
                    repoService.CreateRepo(new JavaScriptSerializer().Serialize(newRepo));
                    MessageBox.Show("New repo has been created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    SuggestRepoCreation("Do you want to create one more repo?");
                }
                else
                {
                    MessageBox.Show("No changes have been made!", "No Changes", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                }
            } else
            {
                Application.Current.Shutdown();
            }
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

        private void RepoService_EditUserCompleted(object sender, GitHubRepoListService.EditUserCompletedEventArgs e)
        {
            try
            {
                HandleServiceResponse(e.Result, "User has been successfully updated!");
            }
            catch (TargetInvocationException)
            {
                StopProcessVisualisation("Connection lost!", true);
                MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RepoService_DeleteReposCompleted(object sender, GitHubRepoListService.DeleteReposCompletedEventArgs e)
        {
            try
            {
                HandleServiceResponse(e.Result, "Selected entries were successfully deleted!");
            }
            catch (TargetInvocationException)
            {
                StopProcessVisualisation("Connection lost!", true);
                MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RepoService_EditRepoCompleted(object sender, GitHubRepoListService.EditRepoCompletedEventArgs e)
        {
            try
            {
                HandleServiceResponse(e.Result, "Changes saved!", true);
            }
            catch (TargetInvocationException)
            {
                StopProcessVisualisation("Connection lost!", true);
                MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RepoService_CreateReposCompleted(object sender, GitHubRepoListService.CreateReposCompletedEventArgs e)
        {
            try
            {
                HandleServiceResponse(e.Result, "New repos created successfully!");
            }
            catch (TargetInvocationException)
            {
                StopProcessVisualisation("Connection lost!", true);
                MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RepoService_CreateRepoCompleted(object sender, GitHubRepoListService.CreateRepoCompletedEventArgs e)
        {
            try
            {
                HandleServiceResponse(e.Result, "New repo created successfully!");
            }
            catch (TargetInvocationException)
            {
                StopProcessVisualisation("Connection lost!", true);
                MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RepoService_ImportReposFromGitHubCompleted(object sender, GitHubRepoListService.ImportReposFromGitHubCompletedEventArgs e)
        {
            try
            {
                HandleServiceResponse(e.Result, "Import finished successfully!");
            }
            catch (TargetInvocationException)
            {
                StopProcessVisualisation("Connection lost!", true);
                MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            try
            {
                repoDataGrid.ItemsSource = null;
                repoDataGrid.ItemsSource = new JavaScriptSerializer().Deserialize<Repo[]>(repoService.GetRepos());
            } catch (Exception)
            {
                this.Hide();
                MessageBox.Show("Authentication credentials have changed! Please sign in again.", "Outdated Authentication Credentials", MessageBoxButton.OK, MessageBoxImage.Warning);
                new MainWindow().Show();
                this.Close();
            }
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

            using (StreamReader reader = new StreamReader(jsonFileStream, Encoding.UTF8))
            {
                string newReposJsonString = reader.ReadToEnd();
                try
                {
                    repoService.CreateReposAsync(newReposJsonString);
                }
                catch (ArgumentException)
                {
                    StopProcessVisualisation("Ready");
                    MessageBox.Show("Failed to parse JSON file!", "Invalid JSON file", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            
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
                    try
                    {
                        writer.Write(repoService.GetRepos());
                    }
                    catch (WebException)
                    {
                        StopProcessVisualisation("Connection lost!", true);
                        MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            StopProcessVisualisation(string.Format("All repos have been exported to: {0}", saveJsonFileDialog.FileName));
            MessageBox.Show("Export finished successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void deleteSelectedMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete all selected entries?", "Delete Selected?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            StartProcessVisualisation("Deleting selected repos...");
            Repo[] repos = repoDataGrid.SelectedItems.Cast<Repo>().ToArray();
            List<int> idsToDelete = new List<int>();
            foreach (var repo in repos)
            {
                idsToDelete.Add(repo.id);
            }

            repoService.DeleteReposAsync(idsToDelete.ToArray());
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
                    dynamic newValue = null;

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
                    
                    repoService.EditRepoAsync(new JavaScriptSerializer().Serialize(editedRepo));
                }
            }
        }

        private void addNewRepoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var newRepoDialog = new NewRepoWindow(authDetails.Login);
            if (newRepoDialog.ShowDialog() == true)
            {
                var newRepo = newRepoDialog.NewRepo;
                repoService.CreateRepoAsync(new JavaScriptSerializer().Serialize(newRepo));
            }
            else
            {
                StopProcessVisualisation("No changes have been made!");
                processRunningProgressBar.Foreground = Brushes.Gold;
            }
        }

        private void userDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            StartProcessVisualisation("Updating user...");
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;    // column name
                    dynamic newValue = null;

                    if (e.EditingElement.GetType() == typeof(TextBox))
                    {
                        newValue = (e.EditingElement as TextBox).Text;
                    }
                    else if (e.EditingElement.GetType() == typeof(CheckBox))
                    {
                        newValue = (e.EditingElement as CheckBox).IsChecked;
                    }
                    else
                    {
                        MessageBox.Show("Can't edit this type of field", "Fail", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var editedUser = e.Row.Item as User;
                    Type repoType = typeof(User);
                    PropertyInfo userPropertyInfo = repoType.GetProperty(bindingPath);
                    try
                    {
                        dynamic convertedPropertyValue = Convert.ChangeType(newValue, userPropertyInfo.PropertyType);
                        userPropertyInfo.SetValue(editedUser, convertedPropertyValue);
                    }
                    catch (FormatException ex)
                    {
                        MessageBox.Show(ex.Message, "Error saving new value", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    repoService.EditUserAsync(new JavaScriptSerializer().Serialize(editedUser));
                }
            }
        }

        private void repoDataGrid_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                deleteSelectedMenuItem_Click(sender, e);
        }
    }
}
