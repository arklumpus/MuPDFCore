using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;

namespace PDFViewerDemo
{
    public partial class OCRProgressWindow : Window
    {
        /// <summary>
        /// Event invoked when the user clicks on the "Cancel" button.
        /// </summary>
        public event EventHandler CancelClicked;

        public OCRProgressWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Set the current progress value.
        /// </summary>
        /// <param name="progress">The progress value (ranging from 0 to 1).</param>
        public void SetProgress(double progress)
        {
            this.FindControl<ProgressBar>("ProgressBar").Value = progress;
            this.FindControl<TextBlock>("ProgressText").Text = progress.ToString("0%");
        }

        /// <summary>
        /// Invoked when the user clicks on the "Cancel" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            // Invoke the CancelClicked event.
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
