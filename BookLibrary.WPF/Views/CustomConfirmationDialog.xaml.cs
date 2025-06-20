using System;
using System.Windows;

namespace BookLibrary.WPF.Views
{
    /// <summary>
    /// Interaction logic for CustomConfirmationDialog.xaml
    /// </summary>
    public partial class CustomConfirmationDialog : Window
    {
        public bool Result { get; private set; }

        public CustomConfirmationDialog()
        {
            InitializeComponent();
            Result = false;
        }

        public CustomConfirmationDialog(string message) : this()
        {
            MessageText.Text = message;
        }

        public static bool Show(Window owner, string message)
        {
            var dialog = new CustomConfirmationDialog(message)
            {
                Owner = owner
            };
            dialog.ShowDialog();
            return dialog.Result;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
