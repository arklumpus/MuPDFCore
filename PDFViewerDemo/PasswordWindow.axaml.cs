using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PDFViewerDemo
{
    public partial class PasswordWindow : Window
    {
        public PasswordWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Invoked when the user clicks on the "OK" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close(this.FindControl<TextBox>("PasswordBox").Text);
        }

        /// <summary>
        /// Invoked when the user clicks on the "Cancel" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close(null);
        }
    }
}
