using System.Windows;

namespace GitHubRepoListWPFClient
{
    /// <summary>
    /// Interaction logic for ImportFromGitHubWindow.xaml
    /// </summary>
    public partial class ImportFromGitHubWindow : Window
    {
        public string GitHubUsername;
        public ImportFromGitHubWindow()
        {
            InitializeComponent();
        }

        private void importFromGitHubButton_Click(object sender, RoutedEventArgs e)
        {
            GitHubUsername = gitHubUsernameTextBox.Text;
            this.DialogResult = true;
        }
    }
}
