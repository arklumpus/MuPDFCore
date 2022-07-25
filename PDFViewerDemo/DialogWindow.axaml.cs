using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PDFViewerDemo
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        public DialogWindow(string text)
        {
            InitializeComponent();

            this.FindControl<TextBlock>("MessageContainer").Text = text;
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
            this.Close();
        }
    }
}
