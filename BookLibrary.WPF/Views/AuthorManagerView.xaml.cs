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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BookLibrary.ViewModels.AuthorManagement;

namespace BookLibrary.WPF.Views
{
    /// <summary>
    /// Interaction logic for AuthorManagerView.xaml
    /// </summary>
    public partial class AuthorManagerView : UserControl
    {
        public AuthorManagerView()
        {
            InitializeComponent();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Refresh the authors list
            if (DataContext is AuthorManagerViewModel viewModel)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    
                    if (viewModel.LoadAuthorsCommand.CanExecute(null))
                    {
                        viewModel.LoadAuthorsCommand.Execute(null);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error refreshing list: {ex.Message}", "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}
