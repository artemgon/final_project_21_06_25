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

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the ViewModel and manually trigger the UpdateCanSaveAuthor check
            if (DataContext is AuthorManagerViewModel viewModel)
            {
                var selectedAuthor = viewModel.SelectedAuthor;
                if (selectedAuthor != null)
                {
                    // Display current field values
                    MessageBox.Show($"Field Values:\n" +
                                    $"FirstName: '{selectedAuthor.FirstName}'\n" +
                                    $"LastName: '{selectedAuthor.LastName}'\n" +
                                    $"Biography: '{selectedAuthor.Biography}'\n" +
                                    $"CanSaveAuthor: {viewModel.CanSaveAuthor}",
                                    "Debug Info");
                    
                    // Manually trigger the update
                    viewModel.ManuallyUpdateCanSave();
                }
                else
                {
                    MessageBox.Show("No author selected!", "Debug Info");
                }
            }
        }

        private void TestAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Test the Add Author functionality
            if (DataContext is AuthorManagerViewModel viewModel)
            {
                // Show current Add Author field values and command state
                MessageBox.Show($"Add Author Debug Info:\n" +
                                $"NewAuthorFirstName: '{viewModel.NewAuthorFirstName}'\n" +
                                $"NewAuthorLastName: '{viewModel.NewAuthorLastName}'\n" +
                                $"NewAuthorBiography: '{viewModel.NewAuthorBiography}'\n" +
                                $"AddAuthorCommand.CanExecute: {viewModel.AddAuthorCommand.CanExecute(null)}",
                                "Add Author Debug");
                
                // Try to manually trigger the AddAuthorCommand CanExecute check
                viewModel.AddAuthorCommand.NotifyCanExecuteChanged();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Refresh the authors list with better error handling
            if (DataContext is AuthorManagerViewModel viewModel)
            {
                try
                {
                    // Show loading indicator
                    Mouse.OverrideCursor = Cursors.Wait;
                    
                    // Execute the LoadAuthorsCommand to refresh the list
                    if (viewModel.LoadAuthorsCommand.CanExecute(null))
                    {
                        viewModel.LoadAuthorsCommand.Execute(null);
                        MessageBox.Show($"List refreshed! Found {viewModel.Authors.Count} authors.", "Refresh Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Cannot refresh list at this time. Please try again.", "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error refreshing list: {ex.Message}", "Refresh Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Restore cursor
                    Mouse.OverrideCursor = null;
                }
            }
            else
            {
                MessageBox.Show("ViewModel not found. Cannot refresh list.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ForceSyncButton_Click(object sender, RoutedEventArgs e)
        {
            // Force database synchronization
            if (DataContext is AuthorManagerViewModel viewModel)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    
                    MessageBox.Show("Starting database synchronization...", "Force Sync", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Clear current list
                    viewModel.Authors.Clear();
                    
                    // Force reload from database
                    await Task.Delay(500); // Small delay to ensure UI updates
                    
                    if (viewModel.LoadAuthorsCommand.CanExecute(null))
                    {
                        viewModel.LoadAuthorsCommand.Execute(null);
                        
                        // Wait a moment for the async operation to complete
                        await Task.Delay(1000);
                        
                        MessageBox.Show($"Database sync complete!\n" +
                                       $"Authors found: {viewModel.Authors.Count}\n" +
                                       $"If you still don't see your authors, there might be a database connection issue.",
                                       "Sync Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Cannot perform database sync at this time.", "Sync Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during database sync: {ex.Message}", "Sync Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private async void TestDbButton_Click(object sender, RoutedEventArgs e)
        {
            // Test database connection and query directly
            if (DataContext is AuthorManagerViewModel viewModel)
            {
                try
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    
                    // Use the enhanced database test method
                    var result = await viewModel.TestDatabaseConnectionAsync();
                    
                    MessageBox.Show(result, "Database Connection Test", MessageBoxButton.OK, 
                                  result.StartsWith("✅") ? MessageBoxImage.Information : MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Test failed with exception: {ex.Message}", "Test Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }
    }
}
