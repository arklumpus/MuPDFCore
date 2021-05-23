using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using MuPDFCore.MuPDFRenderer;
using Avalonia.Data.Converters;
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VectSharp.PDF;
using System.Text.RegularExpressions;
using System.Linq;

namespace PDFViewerDemo
{
    public class MainWindow : Window
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
        /// The <see cref="MuPDFContext"/> holding the cache and exception stack.
        /// </summary>
        private MuPDFContext Context;

        /// <summary>
        /// The current <see cref="MuPDFDocument"/>.
        /// </summary>
        private MuPDFDocument Document;

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

            ocrLanguageBox.Items = ocrLanguageItems;
            ocrLanguageBox.SelectedIndex = 0;

            Watcher.Changed += FileChanged;
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

            gpr.FillText(400, 92, "Move the mouse with", new VectSharp.Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 148, "the left button pressed", new VectSharp.Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 204, "to pan around or to", new VectSharp.Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 260, "select text", new VectSharp.Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);

            gpr.FillText(400, 530, "Use the mouse wheel", new VectSharp.Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);
            gpr.FillText(400, 586, "to zoom in/out", new VectSharp.Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 35), VectSharp.Colours.Gray);

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
                        }, DispatcherPriority.MinValue);
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
                Document.Render(this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber, bounds, zoom, PixelFormats.RGBA, fb.Address);
            }

            return bmp;
        }

        /// <summary>
        /// Invoked when the window is opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowOpened(object sender, EventArgs e)
        {
            //Render the initial PDF and initialise the PDFRenderer with it.
            MemoryStream ms = RenderInitialPDF();
            Context = new MuPDFContext();
            Document = new MuPDFDocument(Context, ref ms, InputFileTypes.PDF);

            MaxPageNumber = 1;
            this.FindControl<PDFRenderer>("MuPDFRenderer").Initialize(Document, ocrLanguage: GetCurrentLanguage(this.FindControl<ComboBox>("OCRLanguageBox").SelectedIndex));
            this.FindControl<Image>("PageAreaImage").Source = GenerateThumbnail();

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
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                //Keep track of the current DisplayArea.
                Rect displayArea = this.FindControl<PDFRenderer>("MuPDFRenderer").DisplayArea;

                //Close the current document and reopen it.
                this.FindControl<PDFRenderer>("MuPDFRenderer").ReleaseResources();
                Document?.Dispose();
                Document = new MuPDFDocument(Context, e.FullPath);
                MaxPageNumber = Document.Pages.Count;
                this.FindControl<PDFRenderer>("MuPDFRenderer").Initialize(Document, ocrLanguage: GetCurrentLanguage(this.FindControl<ComboBox>("OCRLanguageBox").SelectedIndex));
                this.FindControl<Image>("PageAreaImage").Source = GenerateThumbnail();

                //Restore the DisplayArea.
                this.FindControl<PDFRenderer>("MuPDFRenderer").SetDisplayAreaNow(displayArea);
            });
        }

        /// <summary>
        /// Invoked by the "Open file" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OpenFileClicked(object sender, RoutedEventArgs e)
        {
            //Show a dialog with the supported file types.
            OpenFileDialog dialog = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Open document...",
            };

            string[] result = await dialog.ShowAsync(this);

            if (result.Length == 1)
            {
                //First of all we need the PDFRenderer to stop doing anything with the Document.
                this.FindControl<PDFRenderer>("MuPDFRenderer").ReleaseResources();

                //Now we can dispose the document.
                Document?.Dispose();

                //Create a new document and initialise the PDFRenderer with it.
                Document = new MuPDFDocument(Context, result[0]);

                MaxPageNumber = Document.Pages.Count;
                this.FindControl<PDFRenderer>("MuPDFRenderer").Initialize(Document, ocrLanguage: GetCurrentLanguage(this.FindControl<ComboBox>("OCRLanguageBox").SelectedIndex));
                this.FindControl<Image>("PageAreaImage").Source = GenerateThumbnail();

                //Set up the FileWatcher to keep track of any changes to the file.
                Watcher.EnableRaisingEvents = false;
                Watcher.Path = Path.GetDirectoryName(result[0]);
                Watcher.Filter = Path.GetFileName(result[0]);
                Watcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Invoked when the value of the NumericUpDown containing the page number is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageNumberChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            //Only act if the page is actually different from the one displayed by the renderer. The -1 is due to the fact that the page numbers used by the library are 0-based, while we show them as 1-based indices.
            if ((int)e.NewValue - 1 != this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber)
            {
                //We need to re-initialise the renderer. No need to ask it to release resources here because it will do it on its own (and we don't need to dispose the Document).
                this.FindControl<PDFRenderer>("MuPDFRenderer").Initialize(Document, pageNumber: (int)e.NewValue - 1, ocrLanguage: GetCurrentLanguage(this.FindControl<ComboBox>("OCRLanguageBox").SelectedIndex));
                this.FindControl<Image>("PageAreaImage").Source = GenerateThumbnail();
            }
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
                //We need to re-initialise the renderer. No need to ask it to release resources here because it will do it on its own (and we don't need to dispose the Document).
                await this.FindControl<PDFRenderer>("MuPDFRenderer").InitializeAsync(Document, pageNumber: this.FindControl<PDFRenderer>("MuPDFRenderer").PageNumber, ocrLanguage: GetCurrentLanguage(this.FindControl<ComboBox>("OCRLanguageBox").SelectedIndex));
                this.FindControl<Image>("PageAreaImage").Source = GenerateThumbnail();
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
            string selection = this.FindControl<PDFRenderer>("MuPDFRenderer").GetSelectedText() ?? "";
            await Application.Current.Clipboard.SetTextAsync(selection);
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
                await Application.Current.Clipboard.SetTextAsync(selection);
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
    }

    /// <summary>
    /// Used when converting page numbers: the page numbers are 0-based, but most people would expect the first page to be page number 1.
    /// </summary>
    class IncreaseByOne : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) - 1;
        }
    }
}
