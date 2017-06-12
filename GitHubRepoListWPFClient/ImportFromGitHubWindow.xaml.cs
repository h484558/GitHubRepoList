using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

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
