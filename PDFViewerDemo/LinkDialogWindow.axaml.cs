using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace PDFViewerDemo;

public partial class LinkDialogWindow : Window
{
    /// <summary>
    /// The link Uri.
    /// </summary>
    public string Uri { get; }

    /// <summary>
    /// If the user closes this window by clicking on the "Yes" button, this property will be <see langword="true"/>.
    /// </summary>
    public bool Result { get; private set; } = false;

    /// <summary>
    /// Whether the "Do not ask again" check box has been checked.
    /// </summary>
    public bool DoNotAskAgain => this.FindControl<CheckBox>("DoNotAskBox").IsChecked == true;

    public LinkDialogWindow()
    {
        InitializeComponent();
    }

    public LinkDialogWindow(string uri)
    {
        InitializeComponent();

        this.FindControl<TextBlock>("LinkContainer").Text = uri;
        this.Uri = uri;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Invoked when the user clicks on the "Yes" button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void YesButtonClicked(object sender, RoutedEventArgs e)
    {
        this.Result = true;
        this.Close();
    }

    /// <summary>
    /// Invoked when the user clicks on the "No" button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void NoButtonClicked(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}