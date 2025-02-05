﻿using Avalonia;
using Avalonia.Controls;
using MuPDFCore.MuPDFRenderer;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using MuPDFCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VectSharp.PDF;
using System.Text.RegularExpressions;
using System.Linq;
using Avalonia.Platform.Storage;
using MuPDFCore.StructuredText;
using System.Text.Json.Serialization;
using System.Reflection.Emit;

namespace PDFViewerDemo
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Defines the <see cref="MaxPageNumber"/> property.
        /// </summary>
        public static readonly DirectProperty<MainWindow, int> MaxPageNumberProperty = AvaloniaProperty.RegisterDirect<MainWindow, int>(nameof(MaxPageNumber), o => o.MaxPageNumber);
        /// <summary>
        /// Backing field for teh <see cref="MaxPageNumber"/> property.
        /// </summary>
        private int _MaxPageNumber;
        /// <summary>
        /// Holds the number of pages in the document. Defined here as an <see cref="AvaloniaProperty"/> so that we can bind to it from XAML.
        /// </summary>
        public int MaxPageNumber
        {
            get
            {
                return _MaxPageNumber;
            }

            private set
            {
                SetAndRaise(MaxPageNumberProperty, ref _MaxPageNumber, value);
            }
        }

        /// <summary>
        /// Whether a confirmation should be sought from the user before opening external links or not.
        /// </summary>
        public bool AskBeforeOpeningLiks { get; set; } = true;

        /// <summary>
        /// The <see cref="MuPDFContext"/> holding the cache and exception stack.
        /// </summary>
        private MuPDFContext Context;

        /// <summary>
        /// The current <see cref="MuPDFDocument"/>.
        /// </summary>
        private MuPDFDocument Document;

        /// <summary>
        /// Cached user password to decrypt the document.
        /// </summary>
        private string UserPassword;

        /// <summary>
        /// Cached owner password to remove document restrictions.
        /// </summary>
        private string OwnerPassword;

        /// <summary>
        /// A watcher that raises an event if the file that has been opened is overwritten.
        /// </summary>
        private readonly FileSystemWatcher Watcher = new FileSystemWatcher();

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that signals to the <see cref="UIUpdater"/> thread that the window has been closed.
        /// </summary>
        private readonly EventWaitHandle ClosedHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// Determines whether the mouse left button has been pressed on the NavigatorCanvas.
        /// </summary>
        private bool IsMouseDown = false;

        /// <summary>
        /// The point where the mouse left button was pressed on the NavigatorCanvas.
        /// </summary>
        private Point MouseDownPoint;

        /// <summary>
        /// The display area at the time when the left button was pressed on the NavigatorCanvas.
        /// </summary>
        private Rect MouseDownDisplayArea;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Populate the ComboBox with the OCR languages.
            ComboBox ocrLanguageBox = this.FindControl<ComboBox>("OCRLanguageBox");

            // Enumerate the "fast" languages.
            TesseractLanguage.Fast[] fastLanguages = (TesseractLanguage.Fast[])Enum.GetValues(typeof(TesseractLanguage.Fast));

            // Enumerate the "fast" scripts.
            TesseractLanguage.FastScripts[] fastScripts = (TesseractLanguage.FastScripts[])Enum.GetValues(typeof(TesseractLanguage.FastScripts));

            // Store the language names in a list.
            List<string> ocrLanguageItems = new List<string>(fastLanguages.Length + fastScripts.Length + 1) { "None" };
            ocrLanguageItems.AddRange(from el in fastLanguages select el.ToString());
            ocrLanguageItems.AddRange(from el in fastScripts select el.ToString());

            ocrLanguageBox.ItemsSource = ocrLanguageItems;
            ocrLanguageBox.SelectedIndex = 0;

            Watcher.Changed += FileChanged;

            // Since bindings don't seem to be working properly anymore, we need our own "manual bindings".
            this.FindControl<PDFRenderer>("MuPDFRenderer").PropertyChanged += (s, e) =>
            {
                if (e.Property == PDFRenderer.PageNumberProperty)
                {
                    this.FindControl<NumericUpDown>("PageNumberBox").Value = e.GetNewValue<int>() + 1;
                }
                else if (e.Property == PDFRenderer.ZoomProperty)
                {
                    // Prevent the zoom NumericUpDown from re-updating the zoom while we are changing it.
                    UpdatingZoomFromRenderer = true;
                    this.FindControl<NumericUpDown>("ZoomBox").Value = (decimal)e.GetNewValue<double>();
                    UpdatingZoomFromRenderer = false;
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Obtains a <see cref="TesseractLanguage"/> corresponding to the selected OCR language.
        /// </summary>
        /// <param name="index">The index of the language selected in the OCR language box.</param>
        /// <returns>A <see cref="TesseractLanguage"/> corresponding to the selected OCR language.</returns>
        private TesseractLanguage GetCurrentLanguage(int index)
        {
            if (index == 0)
            {
                return null;
            }
            else
            {
                // Enumerate the "fast" languages.
                TesseractLanguage.Fast[] fastLanguages = (TesseractLanguage.Fast[])Enum.GetValues(typeof(TesseractLanguage.Fast));

                if (index - 1 < fastLanguages.Length)
                {
                    return new TesseractLanguage(fastLanguages[index - 1]);
                }
                else
                {
                    // Enumerate the "fast" scripts.
                    TesseractLanguage.FastScripts[] fastScripts = (TesseractLanguage.FastScripts[])Enum.GetValues(typeof(TesseractLanguage.FastScripts));
                    return new TesseractLanguage(fastScripts[index - fastLanguages.Length - 1]);
                }
            }
        }

        /// <summary>
        /// Render the initial PDF document that is shown before a file is opened to a <see cref="MemoryStream"/>.
        /// </summary>
        /// <returns></returns>
        private MemoryStream RenderInitialPDF()
        {
            VectSharp.Document doc = new VectSharp.Document();

            doc.Pages.Add(new VectSharp.Page(800, 700));

            VectSharp.Graphics gpr = doc.Pages[0].Graphics;

            gpr.Save();
            gpr.Scale(4, 4);
            gpr.Translate(40, 35);

            gpr.FillPath(new VectSharp.GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5).Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(), VectSharp.Colour.FromCSSString("#e8f0ff").Value);
            gpr.FillPath(new VectSharp.GraphicsPath().MoveTo(0, 11).LineTo(7.5, 11).LineTo(7.5, 0).Arc(7.5, 7.5, 7.5, -Math.PI / 2, -Math.PI).Close(), VectSharp.Colour.FromCSSString("#ff9900").Value);
            gpr.StrokePath(new VectSharp.GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5).Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(), VectSharp.Colour.FromCSSString("#6f8ec6").Value);
            gpr.StrokePath(new VectSharp.GraphicsPath().MoveTo(0, 11).LineTo(15, 11).MoveTo(7.5, 0).LineTo(7.5, 11), VectSharp.Colour.FromCSSString("#6f8ec6").Value);
            gpr.StrokePath(new VectSharp.GraphicsPath().MoveTo(7.5, 4).LineTo(7.5, 8), VectSharp.Colour.FromCSSString("#6f8ec6").Value, 3, VectSharp.LineCaps.Round);

            gpr.FillPath(new VectSharp.GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5).LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(), VectSharp.Colour.FromRgb(180, 180, 180));

            gpr.Save();
            for (int i = 0; i < 3; i++)
            {
                gpr.RotateAt(Math.PI / 2, new VectSharp.Point(7.5, 12.5));
                gpr.FillPath(new VectSharp.GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5).LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(), VectSharp.Colour.FromRgb(180, 180, 180));
            }
            gpr.Restore();

            gpr.Restore();

            gpr.Save();
            gpr.Scale(4, 4);
            gpr.Translate(40, 135);

            gpr.FillPath(new VectSharp.GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5).Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(), VectSharp.Colour.FromCSSString("#e8f0ff").Value);
            gpr.StrokePath(new VectSharp.GraphicsPath().MoveTo(0, 7.5).Arc(7.5, 7.5, 7.5, Math.PI, 2 * Math.PI).LineTo(15, 17.5).Arc(7.5, 17.5, 7.5, 0, Math.PI).Close(), VectSharp.Colour.FromCSSString("#6f8ec6").Value);
            gpr.StrokePath(new VectSharp.GraphicsPath().MoveTo(0, 11).LineTo(15, 11).MoveTo(7.5, 0).LineTo(7.5, 11), VectSharp.Colour.FromCSSString("#6f8ec6").Value);
            gpr.StrokePath(new VectSharp.GraphicsPath().MoveTo(7.5, 4).LineTo(7.5, 8), VectSharp.Colour.FromCSSString("#ff9900").Value, 3, VectSharp.LineCaps.Round);

            gpr.Save();
            gpr.Scale(0.5, 0.5);
            gpr.Translate(7.5, 7);
            gpr.FillPath(new VectSharp.GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5).LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(), VectSharp.Colour.FromRgb(180, 180, 180));

            gpr.RotateAt(Math.PI, new VectSharp.Point(7.5, 5));
            gpr.FillPath(new VectSharp.GraphicsPath().MoveTo(2.5, -5).LineTo(12.5, -5).LineTo(12.5, -17.5).LineTo(17.5, -17.5).LineTo(7.5, -27.5).LineTo(-2.5, -17.5).LineTo(2.5, -17.5).Close(), VectSharp.Colour.FromRgb(180, 180, 180));

            gpr.Restore();

            gpr.Restore();

            gpr.FillText(400, 92, "Move the mouse with", new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 148, "the left button pressed", new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 204, "to pan around or to", new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 260, "select text", new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);

            gpr.FillText(400, 530, "Use the mouse wheel", new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 586, "to zoom in/out", new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);

            MemoryStream ms = new MemoryStream();
            doc.SaveAsPDF(ms);
            return ms;
        }

        /// <summary>
        /// Starts a thread that periodically polls the <see cref="Context"/> to determine how much memory it is using in the asset cache store.
        /// </summary>
        private void UIUpdater()
        {
            Grid cacheFillGrid = this.FindControl<Grid>("CacheFillGrid");
            Canvas cacheFillCanvas = this.FindControl<Canvas>("CacheFillCanvas");
            PDFRenderer renderer = this.FindControl<PDFRenderer>("MuPDFRenderer");

            Thread thr = new Thread(async () =>
            {
                //Keep running until the window is closed.
                while (!ClosedHandle.WaitOne(0))
                {
                    if (Context != null)
                    {
                        long currentSize = Context.StoreSize;
                        long maxSize = Context.StoreMaxSize;

                        double perc = (double)currentSize / (double)maxSize;

                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            cacheFillGrid.ColumnDefinitions[0] = new ColumnDefinition(perc, GridUnitType.Star);
                            cacheFillGrid.ColumnDefinitions[1] = new ColumnDefinition(1 - perc, GridUnitType.Star);

                            ToolTip.SetTip(cacheFillGrid, perc.ToString("0%") + " (" + Math.Round(currentSize / 1024.0 / 1024.0).ToString("0") + "/" + Math.Round(maxSize / 1024.0 / 1024.0).ToString("0") + "MiB)");

                            if (perc <= 0.5)
                            {
                                cacheFillCanvas.Background = new SolidColorBrush(Color.FromRgb(187, 204, 51));
                            }
                            else if (perc <= 0.75)
                            {
                                cacheFillCanvas.Background = new SolidColorBrush(Color.FromRgb(238, 221, 136));
                            }
                            else
                            {
                                cacheFillCanvas.Background = new SolidColorBrush(Color.FromRgb(238, 136, 102));
                            }

                            //The update does not need to have a high priority.
                        }, DispatcherPriority.ApplicationIdle);
                    }
                    else
                    {
                        cacheFillGrid.ColumnDefinitions[0] = new ColumnDefinition(0, GridUnitType.Star);
                        cacheFillGrid.ColumnDefinitions[1] = new ColumnDefinition(1, GridUnitType.Star);
                    }

                    //We don't need to keep polling too often.
                    Thread.Sleep(2000);
                }
            });

            thr.Start();
        }

        /// <summary>
        /// Generates a thumbnail of the page.
        /// </summary>
        /// <returns>A <see cref="WriteableBitmap"/> containing the thumbnail of the page.</returns>
        private WriteableBitmap GenerateThumbnail()
        {
            //Render the whole page.
            Rectangle bounds = Document.Pages[this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber].Bounds;

            //Determine the appropriate zoom factor to render a thumbnail of the right size for the NavigatorCanvas, taking into account DPI scaling
            double maxDimension = Math.Max(bounds.Width, bounds.Height);
            double zoom = 200 / maxDimension * ((VisualRoot as ILayoutRoot)?.LayoutScaling ?? 1);

            //Get the actual size in pixels of the image.
            RoundedRectangle roundedBounds = bounds.Round(zoom);

            //Initialize the image
            WriteableBitmap bmp = new WriteableBitmap(new PixelSize(roundedBounds.Width, roundedBounds.Height), new Vector(96, 96), Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Unpremul);

            //Render the page to the bitmap, without marshaling.
            using (ILockedFramebuffer fb = bmp.Lock())
            {
                Document.Render(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber, bounds, zoom, MuPDFCore.PixelFormats.RGBA, fb.Address);
            }

            return bmp;
        }

        /// <summary>
        /// Invoked when the window is opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WindowOpened(object sender, EventArgs e)
        {
            //Render the initial PDF and initialise the PDFRenderer with it.
            MemoryStream ms = RenderInitialPDF();
            Context = new MuPDFContext();
            Document = new MuPDFDocument(Context, ref ms, InputFileTypes.PDF);

            MaxPageNumber = 1;
            CurrentOptionalContentGroupConfiguration = Document.OptionalContentGroupData?.DefaultConfiguration;
            UpdateLeftControlPanel();
            await InitializeDocument(0);

            //Start the UI updater thread.
            UIUpdater();
        }

        /// <summary>
        /// Invoked when the window is closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowClosed(object sender, EventArgs e)
        {
            //Stop the UI updater thread.
            ClosedHandle.Set();

            //Dispose the Document and Context. The PDFRenderer will dispose itself when it detects that it has been detached from the logical tree.
            Document?.Dispose();
            Context?.Dispose();
        }

        /// <summary>
        /// Invoked when the file that has been opened changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileChanged(object sender, FileSystemEventArgs e)
        {
            Watcher.EnableRaisingEvents = false;

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(1000);

                //Keep track of the current DisplayArea.
                Rect displayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;

                //Close the current document and reopen it.
                this.FindControl<PDFRenderer>("MuPDFRenderer").ReleaseResources();
                Document?.Dispose();
                Document = new MuPDFDocument(Context, e.FullPath);

                //The document is encrypted: we need a password to decrypt it.
                if (Document.EncryptionState == EncryptionState.Encrypted)
                {
                    bool success;

                    //Try using the cached password.
                    if (!string.IsNullOrEmpty(UserPassword))
                    {
                        //Try unlocking the document with the cached user password.
                        bool unlockResult = Document.TryUnlock(UserPassword, out PasswordTypes pswType);

                        //Check if the password is correct.
                        if (unlockResult)
                        {
                            //The password is correct.

                            //Check that the cached user password is still the user password and that the document is thus unlocked.
                            if (pswType.HasFlag(PasswordTypes.User) && Document.EncryptionState == EncryptionState.Unlocked)
                            {
                                //All is fine.
                                success = true;
                            }
                            else
                            {
                                //The cached user password is now the owner password. We need a new password.
                                success = false;
                            }
                        }
                        else
                        {
                            //The cached user password is no longer correct. We need a new password.
                            success = false;
                        }
                    }
                    else
                    {
                        success = false;
                    }

                    //If we need a new password, ask the user for it.
                    if (!success)
                    {
                        //Ask the user for the password.
                        PasswordWindow pwdWin = new PasswordWindow();
                        string password = await pwdWin.ShowDialog<string>(this);

                        if (string.IsNullOrEmpty(password))
                        {
                            //The user did not provide a password. The document cannot be opened.
                            await new DialogWindow("The document cannot be opened without the user password!").ShowDialog(this);
                            success = false;
                        }
                        else
                        {
                            //The user provided a password. Try unlocking the document with the password.
                            bool unlockResult = Document.TryUnlock(password, out PasswordTypes pswType);

                            //Check if the password is correct.
                            if (unlockResult)
                            {
                                //The password is correct.

                                //Check that the user provided the user password (and not the owner password) and that the document is thus unlocked.
                                if (pswType.HasFlag(PasswordTypes.User) && Document.EncryptionState == EncryptionState.Unlocked)
                                {
                                    //All is fine.
                                    success = true;
                                    UserPassword = password;
                                }
                                else
                                {
                                    //The user provided the owner password instead of the user password. The document cannot be opened.
                                    await new DialogWindow("The password corresponds to the \"owner\" password for the document, but the \"user\" password is instead required!").ShowDialog(this);
                                    success = false;
                                }
                            }
                            else
                            {
                                //The user provided an incorrect password. The document cannot be opened.
                                await new DialogWindow("The password is incorrect!").ShowDialog(this);
                                success = false;
                            }
                        }

                        //If the document could not be unlocked, fall back to the default PDF.
                        if (!success)
                        {
                            MemoryStream ms = RenderInitialPDF();
                            Document = new MuPDFDocument(Context, ref ms, InputFileTypes.PDF);
                            Watcher.EnableRaisingEvents = false;
                        }
                    }
                }

                MaxPageNumber = Document.Pages.Count;
                CurrentOptionalContentGroupConfiguration = Document.OptionalContentGroupData?.DefaultConfiguration;
                UpdateLeftControlPanel();
                await InitializeDocument(0);

                //Restore the DisplayArea.
                this.FindControl<PDFRenderer>("MuPDFRenderer").SetDisplayAreaNow(displayArea);

                Watcher.EnableRaisingEvents = true;
            });
        }

        /// <summary>
        /// Invoked by the "Open file" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenFileClicked(object sender, RoutedEventArgs e)
        {
            //Show a dialog to select a file.
            IReadOnlyList<Avalonia.Platform.Storage.IStorageFile> result = await this.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions() { Title = "Open document...", AllowMultiple = false });

            string localPath = null;

            if (result != null && result.Count == 1)
            {
                // Get the file path.
                localPath = result[0].TryGetLocalPath();

                //First of all we need the PDFRenderer to stop doing anything with the Document.
                this.FindControl<PDFRenderer>("MuPDFRenderer").ReleaseResources();

                //Now we can dispose the document.
                Document?.Dispose();

                //Create a new document and initialise the PDFRenderer with it.
                Document = new MuPDFDocument(Context, localPath);

                //Reset the cached user password.
                UserPassword = null;

                //Reset the cached owner password.
                OwnerPassword = null;

                //The document is encrypted: we need a password to decrypt it.
                if (Document.EncryptionState == EncryptionState.Encrypted)
                {
                    //Ask the user for the password.
                    PasswordWindow pwdWin = new PasswordWindow();
                    string password = await pwdWin.ShowDialog<string>(this);

                    bool success;

                    if (string.IsNullOrEmpty(password))
                    {
                        //The user did not provide a password. The document cannot be opened.
                        await new DialogWindow("The document cannot be opened without the user password!").ShowDialog(this);
                        success = false;
                    }
                    else
                    {
                        //The user provided a password. Try unlocking the document with the password.
                        bool unlockResult = Document.TryUnlock(password, out PasswordTypes pswType);

                        //Check if the password is correct.
                        if (unlockResult)
                        {
                            //The password is correct.

                            //Check that the user provided the user password (and not the owner password) and that the document is thus unlocked.
                            if (pswType.HasFlag(PasswordTypes.User) && Document.EncryptionState == EncryptionState.Unlocked)
                            {
                                //All is fine.
                                success = true;
                                UserPassword = password;
                            }
                            else
                            {
                                //The user provided the owner password instead of the user password. The document cannot be opened.
                                await new DialogWindow("The password corresponds to the \"owner\" password for the document, but the \"user\" password is instead required!").ShowDialog(this);
                                success = false;
                            }
                        }
                        else
                        {
                            //The user provided an incorrect password. The document cannot be opened.
                            await new DialogWindow("The password is incorrect!").ShowDialog(this);
                            success = false;
                        }
                    }

                    //If the document could not be unlocked, fall back to the default PDF.
                    if (!success)
                    {
                        localPath = null;
                        MemoryStream ms = RenderInitialPDF();
                        Document = new MuPDFDocument(Context, ref ms, InputFileTypes.PDF);
                    }
                }

                MaxPageNumber = Document.Pages.Count;
                CurrentOptionalContentGroupConfiguration = Document.OptionalContentGroupData?.DefaultConfiguration;
                UpdateLeftControlPanel();
                await InitializeDocument(0);

                if (!string.IsNullOrEmpty(localPath))
                {
                    //Set up the FileWatcher to keep track of any changes to the file.
                    Watcher.EnableRaisingEvents = false;
                    Watcher.Path = Path.GetDirectoryName(localPath);
                    Watcher.Filter = Path.GetFileName(localPath);
                    Watcher.EnableRaisingEvents = true;
                }
                else
                {
                    Watcher.EnableRaisingEvents = false;
                }
            }
        }

        private void UpdateLeftControlPanel()
        {
            bool outlineVisible = UpdateOutline();
            bool ocgVisible = UpdateOCGs();

            if (outlineVisible)
            {
                this.FindControl<Grid>("OutlineGrid").IsVisible = true;
                this.FindControl<Grid>("LeftPanelGrid").RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                this.FindControl<Grid>("LeftPanelGrid").RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
                this.FindControl<StackPanel>("OutlineContainer").Children.Clear();
                this.FindControl<Grid>("OutlineGrid").IsVisible = false;
            }

            if (ocgVisible)
            {
                this.FindControl<Grid>("OCGGrid").IsVisible = true;
                this.FindControl<Grid>("LeftPanelGrid").RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                this.FindControl<Grid>("LeftPanelGrid").RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
                this.FindControl<StackPanel>("OCGContainer").Children.Clear();
                this.FindControl<Grid>("OCGGrid").IsVisible = false;
            }

            if (outlineVisible || ocgVisible)
            {
                this.FindControl<GridSplitter>("LeftPanelGridSplitter").IsVisible = true;
                this.FindControl<Grid>("MainGrid").ColumnDefinitions[0].Width = new GridLength(200, GridUnitType.Pixel);
                this.FindControl<GridSplitter>("LeftPanelVerticalGridSplitter").IsVisible = outlineVisible && ocgVisible;
            }
            else
            {
                this.FindControl<GridSplitter>("LeftPanelGridSplitter").IsVisible = false;
                this.FindControl<GridSplitter>("LeftPanelVerticalGridSplitter").IsVisible = false;
                this.FindControl<Grid>("MainGrid").ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
            }
        }

        private bool UpdateOutline()
        {
            if (Document.Outline.Count > 0)
            {
                Controls outlineContainer = this.FindControl<StackPanel>("OutlineContainer").Children;

                outlineContainer.Clear();

                foreach (MuPDFOutlineItem item in Document.Outline.Items)
                {
                    outlineContainer.Add(BuildOutlineItem(item, 0));
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private Control BuildOutlineItem(MuPDFOutlineItem item, int level)
        {
            if (item.Children.Count == 0)
            {
                TextBlock blockItem = new TextBlock() { Padding = new Thickness(Math.Min(21 * (level + 1), 42), 0, 5, 0), Text = item.Title, Cursor = new Cursor(StandardCursorType.Hand) };

                int destination = item.PageNumber;

                if (destination >= 0)
                {
                    blockItem.PointerPressed += async (s, e) =>
                    {
                        await InitializeDocument(destination);
                    };

                    blockItem.PointerEntered += (s, e) => blockItem.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                    blockItem.PointerExited += (s, e) => blockItem.Background = null;
                }

                return blockItem;
            }
            else
            {
                Expander exp = new Expander() { Margin = new Thickness(Math.Min(21 * level, 21), 0, 0, 0) };
                TextBlock blockItem = new TextBlock() { Text = item.Title, Cursor = new Cursor(StandardCursorType.Hand) };

                int destination = item.PageNumber;

                if (destination >= 0)
                {
                    blockItem.PointerPressed += async (s, e) =>
                    {
                        await InitializeDocument(destination);
                        e.Handled = true;
                    };

                    blockItem.PointerReleased += (s, e) => e.Handled = true;
                    blockItem.PointerEntered += (s, e) => exp.Background = new SolidColorBrush(Color.FromRgb(220, 220, 220));
                    blockItem.PointerExited += (s, e) => exp.Background = null;
                }

                exp.Header = blockItem;

                StackPanel contents = new StackPanel();
                exp.Content = contents;

                foreach (MuPDFOutlineItem child in item.Children)
                {
                    contents.Children.Add(BuildOutlineItem(child, level + 1));
                }

                return exp;
            }
        }

        private List<(MuPDFOptionalContentGroupSelectableUIItem ocgUI, CheckBox box, List<MuPDFOptionalContentGroupSelectableUIItem> parents)> OptionalContentGroupUIElements = new List<(MuPDFOptionalContentGroupSelectableUIItem ocgUI, CheckBox box, List<MuPDFOptionalContentGroupSelectableUIItem> parents)>();
        private bool UpdatingOCGUIElements = false;
        private MuPDFOptionalContentGroupConfiguration CurrentOptionalContentGroupConfiguration = null;

        private bool UpdateOCGs()
        {
            if (CurrentOptionalContentGroupConfiguration?.UI?.Length > 0 || Document.OptionalContentGroupData?.DefaultConfiguration != null || Document.OptionalContentGroupData?.AlternativeConfigurations?.Length > 0)
            {
                if (Document.OptionalContentGroupData?.DefaultConfiguration != null || Document.OptionalContentGroupData?.AlternativeConfigurations?.Length > 0)
                {
                    List<Control> configurations = new List<Control>();
                    Control selectedItem = null;

                    if (Document.OptionalContentGroupData?.DefaultConfiguration != null)
                    {
                        StackPanel pnl = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left };

                        TextBlock defaultBlk = new TextBlock() { Text = "(D)", Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)), Margin = new Thickness(0, 0, 5, 0), FontSize = this.FontSize };
                        pnl.Children.Add(defaultBlk);

                        TextBlock nameBlk = new TextBlock() { Text = Document.OptionalContentGroupData.DefaultConfiguration.Name, Margin = new Thickness(0, 0, 5, 0), FontSize = this.FontSize };
                        pnl.Children.Add(nameBlk);

                        TextBlock creatorBlk = new TextBlock() { Text = "(Creator: " + Document.OptionalContentGroupData.DefaultConfiguration.Creator + ")", Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)), FontSize = this.FontSize };
                        pnl.Children.Add(creatorBlk);

                        configurations.Add(pnl);

                        pnl.Tag = Document.OptionalContentGroupData.DefaultConfiguration;

                        if (CurrentOptionalContentGroupConfiguration == Document.OptionalContentGroupData.DefaultConfiguration)
                        {
                            selectedItem = pnl;
                        }
                    }

                    if (Document.OptionalContentGroupData?.AlternativeConfigurations?.Length != null)
                    {
                        for (int i = 0; i < Document.OptionalContentGroupData.AlternativeConfigurations.Length; i++)
                        {
                            StackPanel pnl = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Left };

                            TextBlock nameBlk = new TextBlock() { Text = Document.OptionalContentGroupData.AlternativeConfigurations[i].Name, Margin = new Thickness(0, 0, 5, 0), FontSize = this.FontSize };
                            pnl.Children.Add(nameBlk);

                            TextBlock creatorBlk = new TextBlock() { Text = "(" + Document.OptionalContentGroupData.AlternativeConfigurations[i].Creator + ")", Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)), FontSize = this.FontSize };
                            pnl.Children.Add(creatorBlk);

                            configurations.Add(pnl);

                            pnl.Tag = Document.OptionalContentGroupData.AlternativeConfigurations[i];

                            if (CurrentOptionalContentGroupConfiguration == Document.OptionalContentGroupData.AlternativeConfigurations[i])
                            {
                                selectedItem = pnl;
                            }
                        }
                    }

                    UpdatingOCGUIElements = true;
                    this.FindControl<ComboBox>("OCGConfigurationsBox").ItemsSource = configurations;
                    this.FindControl<ComboBox>("OCGConfigurationsBox").SelectedItem = selectedItem;
                    UpdatingOCGUIElements = false;
                }

                if (CurrentOptionalContentGroupConfiguration?.UI?.Length > 0)
                {
                    Controls ocgContainer = this.FindControl<StackPanel>("OCGContainer").Children;
                    ocgContainer.Clear();
                    OptionalContentGroupUIElements.Clear();

                    foreach (MuPDFOptionalContentGroupUIItem item in Document.OptionalContentGroupData.DefaultConfiguration.UI)
                    {
                        ocgContainer.Add(BuildOCGItem(item, 0, new List<MuPDFOptionalContentGroupSelectableUIItem>()));
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private Control BuildOCGItem(MuPDFOptionalContentGroupUIItem item, int level, List<MuPDFOptionalContentGroupSelectableUIItem> parents)
        {
            int actualLevel = item.Children.Length > 0 ? 0 : level;

            Control label = null;

            if (item is MuPDFOptionalContentGroupLabel)
            {
                label = new TextBlock() { Text = item.Label, Padding = new Thickness(Math.Min(21 * (actualLevel), 42), 0, 5, 0) };
            }
            else if (item is MuPDFOptionalContentGroupCheckbox cbx)
            {
                CheckBox cb = new CheckBox() { Content = cbx.Label, IsChecked = cbx.IsEnabled, IsEnabled = !cbx.IsLocked, Margin = new Thickness(Math.Min(21 * (actualLevel), 42), 0, 5, 0) };
                label = cb;
                OptionalContentGroupUIElements.Add((cbx, cb, parents));

                cb.Click += async (s, e) =>
                {
                    if (!UpdatingOCGUIElements)
                    {
                        Rect displayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;
                        cbx.IsEnabled = cb.IsChecked == true;
                        await InitializeDocument(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber);
                        this.FindControl<PDFRenderer>("MuPDFRenderer").SetDisplayAreaNow(displayArea);

                        for (int i = 0; i < OptionalContentGroupUIElements.Count; i++)
                        {
                            OptionalContentGroupUIElements[i].box.IsEnabled = !OptionalContentGroupUIElements[i].parents.Any(x => !x.IsEnabled);
                        }
                    }
                };
            }
            else if (item is MuPDFOptionalContentGroupRadioButton rbx)
            {
                CheckBox rb = new CheckBox() { Content = rbx.Label, IsChecked = rbx.IsEnabled, IsEnabled = !rbx.IsLocked, Margin = new Thickness(Math.Min(21 * (actualLevel), 42), 0, 5, 0), CornerRadius = new CornerRadius(9) };
                label = rb;
                OptionalContentGroupUIElements.Add((rbx, rb, parents));

                rb.Click += async (s, e) =>
                {
                    if (!UpdatingOCGUIElements)
                    {
                        Rect displayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;
                        rbx.IsEnabled = rb.IsChecked == true;
                        await InitializeDocument(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber);
                        this.FindControl<PDFRenderer>("MuPDFRenderer").SetDisplayAreaNow(displayArea);

                        UpdatingOCGUIElements = true;
                        for (int i = 0; i < OptionalContentGroupUIElements.Count; i++)
                        {
                            OptionalContentGroupUIElements[i].box.IsEnabled = !OptionalContentGroupUIElements[i].parents.Any(x => !x.IsEnabled);
                            OptionalContentGroupUIElements[i].box.IsChecked = OptionalContentGroupUIElements[i].ocgUI.IsEnabled;
                        }
                        UpdatingOCGUIElements = false;
                    }
                };
            }

            if (label != null && item.Children.Length > 0)
            {
                List<MuPDFOptionalContentGroupSelectableUIItem> parentList = new List<MuPDFOptionalContentGroupSelectableUIItem>(parents);

                if (item is MuPDFOptionalContentGroupSelectableUIItem selUi)
                {
                    parentList.Add(selUi);
                }

                StackPanel childContainer = new StackPanel();

                for (int i = 0; i < item.Children.Length; i++)
                {
                    childContainer.Children.Add(BuildOCGItem(item.Children[i], level + 1, parentList));
                }

                Expander exp = new Expander() { Header = label, Content = childContainer };

                return exp;
            }
            else
            {
                return label;
            }
        }

        /// <summary>
        /// Invoked when the value of the NumericUpDown containing the page number is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PageNumberChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            //Only act if the page is actually different from the one displayed by the renderer. The -1 is due to the fact that the page numbers used by the library are 0-based, while we show them as 1-based indices.
            if ((int)e.NewValue - 1 != this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber)
            {
                //We need to re-initialise the renderer. No need to ask it to release resources here because it will do it on its own (and we don't need to dispose the Document).
                await InitializeDocument((int)e.NewValue - 1);
            }
        }

        // We set this to true when the zoom value of the PDF renderer has changed independently of the NumericUpDown (e.g., because the user has used the mouse wheel).
        // In that case, we do not feed back the update to the PDF renderer, to avoid creating a loop.
        private bool UpdatingZoomFromRenderer = false;

        /// <summary>
        /// Invoked when the value of the NumericUpDown containing the zoom level is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            if (!UpdatingZoomFromRenderer)
            {
                this.FindControl<PDFRenderer>("MuPDFRenderer").Zoom = (double)(e.NewValue ?? 1);
            }
        }

        /// <summary>
        /// Initialize the document, showing a progress window for the OCR process, if necessary.
        /// </summary>
        private async Task InitializeDocument(int pageNumber)
        {
            // Currently selected OCR language.
            TesseractLanguage currentOcrLanguage = GetCurrentLanguage(this.FindControl<ComboBox>("OCRLanguageBox").SelectedIndex);

            // Check if we are actually performing OCR.
            if (currentOcrLanguage != null)
            {
                // Create a window to show the OCR progress.
                OCRProgressWindow progress = new OCRProgressWindow();

                // Create a new CancellationTokenSource to allow users to cancel the OCR operation.
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                // Handle the CancelClicked event from the progress dialog.
                progress.CancelClicked += (s, e) =>
                {
                    cancellationTokenSource.Cancel();
                };

                // Show the progress window as a dialog box, but don't await it - we need to run stuff in the background.
                _ = progress.ShowDialog(this);

                // Catch the OperationCanceledException, to handle OCR cancellation.
                try
                {
                    //We need to re-initialise the renderer. No need to ask it to release resources here because it will do it on its own (and we don't need to dispose the Document).
                    await this.FindControl<PDFRenderer>("MuPDFRenderer").InitializeAsync(Document, pageNumber: pageNumber, ocrLanguage: currentOcrLanguage,

                        // Use the cancellation token from the source created above.
                        ocrCancellationToken: cancellationTokenSource.Token,

                        // Use the progress callback to set the value in the progress window.
                        ocrProgress: new Progress<OCRProgressInfo>(prog => progress.SetProgress(prog.Progress))

                        );
                    this.FindControl<Image>("PageAreaImage").Source = GenerateThumbnail();

                    // Close the progress window.
                    progress.Close();
                }
                catch (OperationCanceledException)
                {
                    // Close the progress window.
                    progress.Close();

                    // Change the selected OCR language to "None". This will cause this method to be invoked again.
                    this.FindControl<ComboBox>("OCRLanguageBox").SelectedIndex = 0;
                }
            }
            // No OCR, thus no need to show the progress window.
            else
            {
                //We need to re-initialise the renderer. No need to ask it to release resources here because it will do it on its own (and we don't need to dispose the Document).
                await this.FindControl<PDFRenderer>("MuPDFRenderer").InitializeAsync(Document, pageNumber: pageNumber, ocrLanguage: null);
                this.FindControl<Image>("PageAreaImage").Source = GenerateThumbnail();
            }

            this.FindControl<NumericUpDown>("PageNumberBox").Value = this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber + 1;


            UpdatingZoomFromRenderer = true;
            this.FindControl<NumericUpDown>("ZoomBox").Value = (decimal)this.FindControl<PDFRenderer>("MuPDFRenderer").Zoom;
            UpdatingZoomFromRenderer = false;
        }

        /// <summary>
        /// Invoked when the value of the ComboBox containing the OCR language is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OCRLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Document != null)
            {
                await InitializeDocument(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber);
            }
        }

        /// <summary>
        /// Invoked when a property of the <see cref="PDFRenderer"/> changes; used to update the NavigatorCanvas when the DisplayArea changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RendererPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == PDFRenderer.DisplayAreaProperty)
            {
                PDFRenderer renderer = this.FindControl<PDFRenderer>("MuPDFRenderer");

                Rect displayArea = renderer.DisplayArea;
                Rect pageSize = renderer.PageSize;

                double minX = Math.Min(displayArea.Left, pageSize.Left);
                double minY = Math.Min(displayArea.Top, pageSize.Top);
                double maxX = Math.Max(displayArea.Right, pageSize.Right);
                double maxY = Math.Max(displayArea.Bottom, pageSize.Bottom);

                double width = maxX - minX;
                double height = maxY - minY;

                double size = Math.Max(width, height);

                minX -= (size - width) * 0.5;
                maxX += (size - width) * 0.5;
                minY -= (size - height) * 0.5;
                maxY += (size - height) * 0.5;

                Image pageRect = this.FindControl<Image>("PageAreaImage");
                Canvas pageCanavs = this.FindControl<Canvas>("PageAreaCanvas");
                Avalonia.Controls.Shapes.Rectangle displayRect = this.FindControl<Avalonia.Controls.Shapes.Rectangle>("DisplayAreaRectangle");

                pageRect.Width = pageSize.Width / (maxX - minX) * 200;
                pageRect.Height = pageSize.Height / (maxY - minY) * 200;

                pageCanavs.Width = pageSize.Width / (maxX - minX) * 200;
                pageCanavs.Height = pageSize.Height / (maxY - minY) * 200;
                pageCanavs.RenderTransform = new TranslateTransform((pageSize.Left - minX) / (maxX - minX) * 200, (pageSize.Top - minY) / (maxY - minY) * 200);

                displayRect.Width = displayArea.Width / (maxX - minX) * 200;
                displayRect.Height = displayArea.Height / (maxY - minY) * 200;
                displayRect.RenderTransform = new TranslateTransform((displayArea.Left - minX) / (maxX - minX) * 200, (displayArea.Top - minY) / (maxY - minY) * 200);

            }
        }

        /// <summary>
        /// Invoked when a mouse button is clicked on the NavigatorCanvas. Centers the DisplayArea around the clicked point and starts panning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigatorPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
            {
                IsMouseDown = true;
                MouseDownPoint = e.GetPosition(this.FindControl<Canvas>("NavigatorCanvas"));
                MouseDownDisplayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;

                Avalonia.Controls.Shapes.Rectangle displayRect = this.FindControl<Avalonia.Controls.Shapes.Rectangle>("DisplayAreaRectangle");

                double areaCenterX = ((TranslateTransform)displayRect.RenderTransform).X + displayRect.Width * 0.5;
                double areaCenterY = ((TranslateTransform)displayRect.RenderTransform).Y + displayRect.Height * 0.5;

                double deltaX = (-areaCenterX + MouseDownPoint.X) / displayRect.Width * MouseDownDisplayArea.Width;
                double deltaY = (-areaCenterY + MouseDownPoint.Y) / displayRect.Height * MouseDownDisplayArea.Height;

                MouseDownDisplayArea = new Rect(new Point(this.MouseDownDisplayArea.X + deltaX, this.MouseDownDisplayArea.Y + deltaY), new Point(this.MouseDownDisplayArea.Right + deltaX, this.MouseDownDisplayArea.Bottom + deltaY));

                this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea = MouseDownDisplayArea;
            }
        }

        /// <summary>
        /// Invoked when a mouse button is released on the NavigatorCanvas. Stops panning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigatorPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                IsMouseDown = false;
            }
        }

        /// <summary>
        /// Invoked when the mouse is moved around after the left button has been pressed on the NavigatorCanvas. Moves the DisplayArea around.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigatorPointerMoved(object sender, PointerEventArgs e)
        {
            if (IsMouseDown)
            {
                PDFRenderer renderer = this.FindControl<PDFRenderer>("MuPDFRenderer");
                Avalonia.Controls.Shapes.Rectangle displayRect = this.FindControl<Avalonia.Controls.Shapes.Rectangle>("DisplayAreaRectangle");

                Point point = e.GetPosition(this.FindControl<Canvas>("NavigatorCanvas"));

                double deltaX = -(-point.X + MouseDownPoint.X) / displayRect.Width * MouseDownDisplayArea.Width;
                double deltaY = -(-point.Y + MouseDownPoint.Y) / displayRect.Height * MouseDownDisplayArea.Height;

                renderer.SetDisplayAreaNow(new Rect(new Point(this.MouseDownDisplayArea.X + deltaX, this.MouseDownDisplayArea.Y + deltaY), new Point(this.MouseDownDisplayArea.Right + deltaX, this.MouseDownDisplayArea.Bottom + deltaY)));
            }
        }

        /// <summary>
        /// Invoked when the "Cover" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoverClicked(object sender, RoutedEventArgs e)
        {
            this.FindControl<PDFRenderer>("MuPDFRenderer").Cover();
        }

        /// <summary>
        /// Invoked when the "Contain" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContainClicked(object sender, RoutedEventArgs e)
        {
            this.FindControl<PDFRenderer>("MuPDFRenderer").Contain();
        }

        /// <summary>
        /// Invoked when the "Shrink" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShrinkStoreClicked(object sender, RoutedEventArgs e)
        {
            //Shrink the asset cache store by 50% of its current size.
            Context?.ShrinkStore(0.5);
        }

        /// <summary>
        /// Invoked when the "Clear" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearStoreClicked(object sender, RoutedEventArgs e)
        {
            //Clear the asset cache store.
            Context?.ClearStore();
        }

        /// <summary>
        /// Invoked when the "Copy selected text..." button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CopyClicked(object sender, RoutedEventArgs e)
        {
            //Check whether an owner password is required to allow users to copy content from the document.
            if (Document.RestrictionState == RestrictionState.Restricted && Document.Restrictions.HasFlag(DocumentRestrictions.Copy))
            {
                bool success;

                //Try using the cached password.
                if (!string.IsNullOrEmpty(OwnerPassword))
                {
                    //Try unlocking the document with the cached user password.
                    bool unlockResult = Document.TryUnlock(OwnerPassword, out PasswordTypes pswType);

                    //Check if the password is correct.
                    if (unlockResult)
                    {
                        //The password is correct.

                        //Check that the cached owner password is still the owner password and that the document is thus unlocked.
                        if (pswType.HasFlag(PasswordTypes.Owner) && Document.RestrictionState == RestrictionState.Unlocked)
                        {
                            //All is fine.
                            success = true;
                        }
                        else
                        {
                            //The cached owner password is now the user password. We need a new password.
                            success = false;
                        }
                    }
                    else
                    {
                        //The cached owner password is no longer correct. We need a new password.
                        success = false;
                    }
                }
                else
                {
                    success = false;
                }

                //If we need a new password, ask the user for it.
                if (!success)
                {
                    //Ask the user for the password.
                    PasswordWindow pwdWin = new PasswordWindow();
                    string password = await pwdWin.ShowDialog<string>(this);

                    if (string.IsNullOrEmpty(password))
                    {
                        //The user did not provide a password. The text cannot be copied.
                        await new DialogWindow("Text cannot be copied without the owner password!").ShowDialog(this);
                        return;
                    }
                    else
                    {
                        //The user provided a password. Try unlocking the document with the password.
                        bool unlockResult = Document.TryUnlock(password, out PasswordTypes pswType);

                        //Check if the password is correct.
                        if (unlockResult)
                        {
                            //The password is correct.

                            //Check that the user provided the owner password (and not the user password) and that the document is thus unlocked.
                            if (pswType.HasFlag(PasswordTypes.Owner) && Document.RestrictionState == RestrictionState.Unlocked)
                            {
                                //All is fine.
                                success = true;
                                OwnerPassword = password;
                            }
                            else
                            {
                                //The user provided the user password instead of the owner password. The text cannot be copied.
                                await new DialogWindow("The password corresponds to the \"user\" password for the document, but the \"owner\" password is instead required!").ShowDialog(this);
                                return;
                            }
                        }
                        else
                        {
                            //The user provided an incorrect password. The text cannot be copied.
                            await new DialogWindow("The password is incorrect!").ShowDialog(this);
                            return;
                        }
                    }
                }
            }

            string selection = this.FindControl<PDFRenderer>("MuPDFRenderer").GetSelectedText() ?? "";
            await GetTopLevel(this).Clipboard.SetTextAsync(selection);
        }

        /// <summary>
        /// Invoked when a key is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.C)
            {
                string selection = this.FindControl<PDFRenderer>("MuPDFRenderer").GetSelectedText() ?? "";
                await GetTopLevel(this).Clipboard.SetTextAsync(selection);
            }
            else if (e.KeyModifiers == KeyModifiers.Control && e.Key == Key.A)
            {
                this.FindControl<PDFRenderer>("MuPDFRenderer").SelectAll();
            }
        }

        /// <summary>
        /// Invoked when the "Search" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.FindControl<PDFRenderer>("MuPDFRenderer").Search(new Regex(this.FindControl<TextBox>("SearchBox").Text));
            }
            catch (ArgumentException) { }
        }

        /// <summary>
        /// Invoked when the "Clear" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearClicked(object sender, RoutedEventArgs e)
        {
            this.FindControl<PDFRenderer>("MuPDFRenderer").HighlightedRegions = null;
        }

        /// <summary>
        /// Invoked when the "Apply" button is clicked to apply an optional content group (layer) configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ApplyOCGConfiguration_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.CurrentOptionalContentGroupConfiguration != null)
            {
                Rect displayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;
                this.CurrentOptionalContentGroupConfiguration.Activate();

                await InitializeDocument(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber);
                this.FindControl<PDFRenderer>("MuPDFRenderer").SetDisplayAreaNow(displayArea);

                UpdatingOCGUIElements = true;
                for (int i = 0; i < OptionalContentGroupUIElements.Count; i++)
                {
                    OptionalContentGroupUIElements[i].box.IsEnabled = !OptionalContentGroupUIElements[i].parents.Any(x => !x.IsEnabled);
                    OptionalContentGroupUIElements[i].box.IsChecked = OptionalContentGroupUIElements[i].ocgUI.IsEnabled;
                }
                UpdatingOCGUIElements = false;
            }
        }

        private async void OCGConfigurationComboBox_SelectionChanged(object sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            if (!UpdatingOCGUIElements)
            {
                MuPDFOptionalContentGroupConfiguration config = ((Control)((ComboBox)sender)?.SelectedItem)?.Tag as MuPDFOptionalContentGroupConfiguration;

                if (config != null)
                {
                    this.CurrentOptionalContentGroupConfiguration = config;

                    Rect displayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;
                    config.Activate();

                    await InitializeDocument(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber);
                    this.FindControl<PDFRenderer>("MuPDFRenderer").SetDisplayAreaNow(displayArea);

                    UpdatingOCGUIElements = true;
                    for (int i = 0; i < OptionalContentGroupUIElements.Count; i++)
                    {
                        OptionalContentGroupUIElements[i].box.IsEnabled = !OptionalContentGroupUIElements[i].parents.Any(x => !x.IsEnabled);
                        OptionalContentGroupUIElements[i].box.IsChecked = OptionalContentGroupUIElements[i].ocgUI.IsEnabled;
                    }
                    UpdatingOCGUIElements = false;
                }
            }
        }

        private async void PDFRenderer_LinkClicked(object sender, MuPDFCore.MuPDFRenderer.MupdfLinkClickedEventArgs e)
        {
            switch (e.LinkDestination.Type)
            {
                case MuPDFLinkDestination.DestinationType.SetOCGState:
                    {
                        Rect displayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;
                        ((MuPDFSetOCGStateLinkDestination)e.LinkDestination).Activate();

                        await InitializeDocument(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber);
                        this.FindControl<PDFRenderer>("MuPDFRenderer").SetDisplayAreaNow(displayArea);

                        UpdatingOCGUIElements = true;
                        for (int i = 0; i < OptionalContentGroupUIElements.Count; i++)
                        {
                            OptionalContentGroupUIElements[i].box.IsEnabled = !OptionalContentGroupUIElements[i].parents.Any(x => !x.IsEnabled);
                            OptionalContentGroupUIElements[i].box.IsChecked = OptionalContentGroupUIElements[i].ocgUI.IsEnabled;
                        }
                        UpdatingOCGUIElements = false;
                    }
                    break;

                case MuPDFLinkDestination.DestinationType.External:
                    bool openLink = !this.AskBeforeOpeningLiks;
                    string uri = ((MuPDFExternalLinkDestination)e.LinkDestination).Uri;
                    if (!openLink)
                    {
                        LinkDialogWindow dialog = new LinkDialogWindow(uri);
                        await dialog.ShowDialog(this);
                        openLink = dialog.Result;
                        if (openLink && dialog.DoNotAskAgain)
                        {
                            this.AskBeforeOpeningLiks = false;
                        }
                    }
                    
                    if (openLink)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                        {
                            FileName = uri,
                            UseShellExecute = true
                        });
                    }

                    break;

                case MuPDFLinkDestination.DestinationType.Internal:
                    await InitializeDocument(((MuPDFInternalLinkDestination)e.LinkDestination).PageNumber);
                    break;
            }
        }
    }
}
