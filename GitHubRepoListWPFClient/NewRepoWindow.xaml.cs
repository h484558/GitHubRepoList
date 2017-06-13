using GitHubRepoList.Models;
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
    /// Interaction logic for NewRepoWindow.xaml
    /// </summary>
    public partial class NewRepoWindow : Window
    {
        public Repo NewRepo { get; set; }
        private string _login = "";

        public NewRepoWindow()
        {
            InitializeComponent();
        }

        public NewRepoWindow(string login) : this()
        {
            _login = login;

            fullNameTextBox.Text = string.Format("{0}/", _login);
            urlTextBox.Text = string.Format("https://github.com/{0}/", _login);
            createdAtTextBox.Text = DateTime.Now.ToString();
        }

        private void nameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            fullNameTextBox.Text = string.Format("{0}/{1}", _login, nameTextBox.Text);
            urlTextBox.Text = string.Format("https://github.com/{0}/{1}", _login, nameTextBox.Text);
        }

        private void createRepoButton_Click(object sender, RoutedEventArgs e)
        {
            NewRepo = new Repo { name = nameTextBox.Text, full_name = fullNameTextBox.Text, url = urlTextBox.Text, created_at = createdAtTextBox.Text };
            this.DialogResult = true;
        }
    }
}
