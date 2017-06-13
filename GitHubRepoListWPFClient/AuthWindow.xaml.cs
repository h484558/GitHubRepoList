using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GitHubRepoListWPFClient
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthenticationDetails AuthDetails { get; set; }

        GitHubRepoListService.GitHubRepoListService repoService = new GitHubRepoListService.GitHubRepoListService();
        GitHubRepoListService.AuthenticationHeader authHeader;

        public AuthWindow()
        {
            InitializeComponent();
            repoService.AuthenticateCompleted += RepoService_AuthenticateCompleted;
        }

        private void RepoService_AuthenticateCompleted(object sender, GitHubRepoListService.AuthenticateCompletedEventArgs e)
        {
            if (e.Result == string.Empty)
            {
                signInButton.Content = "Sign in";
                signInButton.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
                MessageBox.Show("Please check your authentication credentials!", "Login failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                AuthDetails = new JavaScriptSerializer().Deserialize<AuthenticationDetails>(e.Result);
                this.DialogResult = true;
            }
        }

        private void signInButton_Click(object sender, RoutedEventArgs e)
        {
            authHeader = new GitHubRepoListService.AuthenticationHeader { Login = authLoginTextBox.Text, Password = authPasswordBox.Password };
            repoService.AuthenticationHeaderValue = authHeader;

            // Show user that we're doing something
            signInButton.Content = "Signing in...";
            signInButton.IsEnabled = false;
            this.Cursor = Cursors.Wait;

            repoService.AuthenticateAsync();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void authLoginTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            authLoginTextBox.SelectAll();
        }

        private void authLoginTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            authLoginTextBox.SelectAll();
        }

        private void authLoginTextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            authLoginTextBox.SelectAll();
        }

        private void authPasswordBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            authPasswordBox.SelectAll();
        }

        private void authPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            authPasswordBox.SelectAll();
        }

        private void authPasswordBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            authPasswordBox.SelectAll();
        }
    }
}
