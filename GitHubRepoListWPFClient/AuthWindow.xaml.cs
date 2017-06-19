using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;

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
            try
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
            catch (TargetInvocationException)
            {
                signInButton.Content = "Sign in";
                signInButton.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
                MessageBox.Show("Connection lost! Please try again later.", "Connection lost", MessageBoxButton.OK, MessageBoxImage.Error);
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
