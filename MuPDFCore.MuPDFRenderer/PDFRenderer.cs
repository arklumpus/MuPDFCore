/*
    MuPDFCore.MuPDFRenderer - A control to display documents in Avalonia using MuPDFCore.
    Copyright (C) 2020  Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MuPDFCore.MuPDFRenderer
{
    /// <summary>
    /// A control to render PDF documents (and other formats), potentally using multiple threads.
    /// </summary>
    public partial class PDFRenderer : Control
    {
        /// <summary>
        /// If this is true, the <see cref="Context"/> and <see cref="Document"/> will be disposed when this object is detached from the logical tree.
        /// </summary>
        private bool OwnsContextAndDocument = true;

        /// <summary>
        /// The <see cref="MuPDFContext"/> using which the <see cref="Document"/> was created.
        /// </summary>
        protected MuPDFContext Context;

        /// <summary>
        /// The <see cref="MuPDFDocument"/> from which the <see cref="Renderer"/> was created.
        /// </summary>
        protected MuPDFDocument Document;

        /// <summary>
        /// The <see cref="MuPDFMultiThreadedPageRenderer"/> that renders the dynamic tiles.
        /// </summary>
        private MuPDFMultiThreadedPageRenderer Renderer;

        /// <summary>
        /// The static renderisation of the page.
        /// </summary>
        private WriteableBitmap FixedCanvasBitmap;

        /// <summary>
        /// The area covered by the <see cref="FixedCanvasBitmap"/>. It should be equal to the <see cref="PageSize"/>, but doesn't have to.
        /// </summary>
        private Rectangle FixedArea;

        /// <summary>
        /// The position and size of the dynamic tiles.
        /// </summary>
        private RoundedRectangle[] DynamicImagesBounds;

        /// <summary>
        /// The dynamic tiles.
        /// </summary>
        private WriteableBitmap[] DynamicBitmaps;

        /// <summary>
        /// If this is true, the <see cref="DynamicBitmaps"/> have been rendered and can be drawn on screen.
        /// </summary>
        private bool AreDynamicBitmapsReady = false;

        /// <summary>
        /// If this is true, the <see cref="DynamicBitmaps"/> will be rendered again immediately after the current rendering operation finishes.
        /// </summary>
        private bool RenderQueued = false;

        /// <summary>
        /// A <see cref="Mutex"/> to synchronise rendering operations. If someone else is holding this mutex, you can assume that it's not safe to access the <see cref="DynamicBitmaps"/>.
        /// </summary>
        private Mutex RenderMutex;

        /// <summary>
        /// A <see cref="Geometry"/> holding the icon that is displayed in the top-right corner when the <see cref="DynamicBitmaps"/> are not available.
        /// </summary>
        private PathGeometry RefreshingGeometry;

        /// <summary>
        /// The thread that is in charge of responding to the rendering requests and either starting a new rendering of the <see cref="DynamicBitmaps"/>, or queueing it.
        /// </summary>
        private Thread RenderDynamicCanvasOuterThread;

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that signals a request for rendering to the <see cref="RenderDynamicCanvasOuterThread"/>.
        /// </summary>
        private readonly EventWaitHandle RenderDynamicCanvasOuterHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// The thread that is in charge of rendering the <see cref="DynamicBitmaps"/>.
        /// </summary>
        private Thread RenderDynamicCanvasInnerThread;

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that signals a request for rendering to the <see cref="RenderDynamicCanvasInnerThread"/>.
        /// </summary>
        private readonly EventWaitHandle RenderDynamicCanvasInnerHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that signals to the <see cref="RenderDynamicCanvasOuterThread"/> that the <see cref="RenderDynamicCanvasInnerThread"/> has acquired the <see cref="RenderMutex"/> and is starting rendering.
        /// </summary>
        private readonly EventWaitHandle RenderDynamicCanvasInnerStartedHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that signals to the <see cref="RenderDynamicCanvasOuterThread"/> and the <see cref="RenderDynamicCanvasInnerThread"/> to cease all operation because this <see cref="PDFRenderer"/> is being detached from the logical tree.
        /// </summary>
        private readonly EventWaitHandle RendererDisposedHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        /// <summary>
        /// The current rendering resolution (in screen units) that is used by the renderer when rendering the <see cref="DynamicBitmaps"/>.
        /// </summary>
        private readonly int[] RenderSize = new int[2];

        /// <summary>
        /// The area on the page that will be rendered by the renderer in the <see cref="DynamicBitmaps"/>.
        /// </summary>
        private Rect RenderDisplayArea;

        /// <summary>
        /// A lock to prevent race conditions when multiple rendering passes are queued consecutively.
        /// </summary>
        private readonly object RenderDisplayAreaLock = new object();

        /// <summary>
        /// Whether a PointerPressed event has fired.
        /// </summary>
        private bool IsMouseDown = false;

        /// <summary>
        /// The point at which the PointerPressed event fired.
        /// </summary>
        private Point MouseDownPoint;

        /// <summary>
        /// The <see cref="DisplayArea"/> when the PointerPressed event fired.
        /// </summary>
        private Rect MouseDownDisplayArea;

        /// <summary>
        /// A structured text representation of the current page, used for selection and search highlight.
        /// </summary>
        protected MuPDFStructuredTextPage StructuredTextPage;

        /// <summary>
        /// A list of <see cref="Quad"/>s that cover the selected text region.
        /// </summary>
        protected List<Quad> SelectionQuads;

        /// <summary>
        /// A list of <see cref="Quad"/>s that cover the highlighted regions.
        /// </summary>
        protected List<Quad> HighlightQuads;

        /// <summary>
        /// Defines the current mouse operation.
        /// </summary>
        private enum CurrentMouseOperations
        {
            /// <summary>
            /// The mouse is being used to pan around the page.
            /// </summary>
            Pan,

            /// <summary>
            /// The mouse is being used to highlight text
            /// </summary>
            Highlight
        }

        /// <summary>
        /// The current mouse operation.
        /// </summary>
        private CurrentMouseOperations CurrentMouseOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="PDFRenderer"/> class.
        /// </summary>
        public PDFRenderer()
        {
            this.InitializeComponent();

            this.PropertyChanged += ControlPropertyChanged;
            this.DetachedFromLogicalTree += ControlDetachedFromLogicalTree;
            this.PointerPressed += ControlPointerPressed;
            this.PointerReleased += ControlPointerReleased;
            this.PointerMoved += ControlPointerMoved;
            this.PointerWheelChanged += ControlPointerWheelChanged;
        }

        /// <summary>
        /// Initializes inner components of the <see cref="PDFRenderer"/>.
        /// </summary>
        private void InitializeComponent()
        {
            PathFigure arrow1 = new PathFigure
            {
                StartPoint = new Point(16 * Math.Cos(Math.PI / 4), 16 * Math.Sin(Math.PI / 4))
            };
            arrow1.Segments.Add(new ArcSegment() { Point = new Point(-16, 0), IsLargeArc = false, RotationAngle = 0, Size = new Avalonia.Size(16, 16), SweepDirection = SweepDirection.Clockwise });
            arrow1.Segments.Add(new LineSegment() { Point = new Point(-7.2727, 0) });
            arrow1.Segments.Add(new LineSegment() { Point = new Point(-21.8181, -17.4545) });
            arrow1.Segments.Add(new LineSegment() { Point = new Point(-36.3636, 0) });
            arrow1.Segments.Add(new LineSegment() { Point = new Point(-27.6363, 0) });
            arrow1.Segments.Add(new ArcSegment() { Point = new Point(27.6363 * Math.Cos(Math.PI / 4), 27.6363 * Math.Sin(Math.PI / 4)), IsLargeArc = false, RotationAngle = 0, Size = new Avalonia.Size(27.6363, 27.6363), SweepDirection = SweepDirection.CounterClockwise });
            arrow1.IsClosed = true;

            PathFigure arrow2 = new PathFigure
            {
                StartPoint = new Point(16 * Math.Cos(5 * Math.PI / 4), 16 * Math.Sin(5 * Math.PI / 4))
            };
            arrow2.Segments.Add(new ArcSegment() { Point = new Point(16, 0), IsLargeArc = false, RotationAngle = 0, Size = new Avalonia.Size(16, 16), SweepDirection = SweepDirection.Clockwise });
            arrow2.Segments.Add(new LineSegment() { Point = new Point(7.2727, 0) });
            arrow2.Segments.Add(new LineSegment() { Point = new Point(21.8181, 17.4545) });
            arrow2.Segments.Add(new LineSegment() { Point = new Point(36.3636, 0) });
            arrow2.Segments.Add(new LineSegment() { Point = new Point(27.6363, 0) });
            arrow2.Segments.Add(new ArcSegment() { Point = new Point(27.6363 * Math.Cos(5 * Math.PI / 4), 27.6363 * Math.Sin(5 * Math.PI / 4)), IsLargeArc = false, RotationAngle = 0, Size = new Avalonia.Size(27.6363, 27.6363), SweepDirection = SweepDirection.CounterClockwise });
            arrow2.IsClosed = true;

            RefreshingGeometry = new PathGeometry();
            RefreshingGeometry.Figures.Add(arrow1);
            RefreshingGeometry.Figures.Add(arrow2);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a <see cref="MuPDFDocument"/>.
        /// </summary>
        /// <param name="document">The <see cref="MuPDFDocument"/> to render.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public void Initialize(MuPDFDocument document, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            if (IsViewerInitialized)
            {
                ReleaseResources();
            }

            OwnsContextAndDocument = false;

            Document = document;
            Context = null;

            ContinueInitialization(threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a <see cref="MuPDFDocument"/>. The OCR step is run asynchronously, in order not to block the UI thread.
        /// </summary>
        /// <param name="document">The <see cref="MuPDFDocument"/> to render.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public async Task InitializeAsync(MuPDFDocument document, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            if (IsViewerInitialized)
            {
                ReleaseResources();
            }

            OwnsContextAndDocument = false;

            Document = document;
            Context = null;

            await ContinueInitializationAsync(threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a document that will be loaded from disk.
        /// </summary>
        /// <param name="fileName">The path to the document that should be opened.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public void Initialize(string fileName, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            if (IsViewerInitialized)
            {
                ReleaseResources();
            }

            OwnsContextAndDocument = true;

            Context = new MuPDFContext();
            Document = new MuPDFDocument(Context, fileName);

            ContinueInitialization(threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a document that will be loaded from disk. The OCR step is run asynchronously, in order not to block the UI thread.
        /// </summary>
        /// <param name="fileName">The path to the document that should be opened.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public async Task InitializeAsync(string fileName, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            if (IsViewerInitialized)
            {
                ReleaseResources();
            }

            OwnsContextAndDocument = true;

            Context = new MuPDFContext();
            Document = new MuPDFDocument(Context, fileName);

            await ContinueInitializationAsync(threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a document that will be loaded from a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> containing the document that should be opened. This can be safely disposed after this method returns.</param>
        /// <param name="fileType">The format of the document.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public void Initialize(MemoryStream ms, InputFileTypes fileType, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            //Get the byte array that underlies the MemoryStream.
            int origin = (int)ms.Seek(0, SeekOrigin.Begin);
            long dataLength = ms.Length;
            byte[] dataBytes = ms.GetBuffer();

            Initialize(dataBytes, fileType, origin, (int)dataLength, threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a document that will be loaded from a <see cref="MemoryStream"/>. The OCR step is run asynchronously, in order not to block the UI thread.
        /// </summary>
        /// <param name="ms">The <see cref="MemoryStream"/> containing the document that should be opened. This can be safely disposed after this method returns.</param>
        /// <param name="fileType">The format of the document.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public async Task InitializeAsync(MemoryStream ms, InputFileTypes fileType, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            //Get the byte array that underlies the MemoryStream.
            int origin = (int)ms.Seek(0, SeekOrigin.Begin);
            long dataLength = ms.Length;
            byte[] dataBytes = ms.GetBuffer();

            await InitializeAsync(dataBytes, fileType, origin, (int)dataLength, threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a document that will be loaded from an array of <see cref="byte"/>s.
        /// </summary>
        /// <param name="dataBytes">The bytes of the document that should be opened. The array will be copied and can be safely discarded/altered after this method returns.</param>
        /// <param name="fileType">The format of the document.</param>
        /// <param name="offset">The offset in the byte array at which the document starts.</param>
        /// <param name="length">The length of the document in bytes. If this is &lt; 0, the whole array is used.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public void Initialize(byte[] dataBytes, InputFileTypes fileType, int offset = 0, int length = -1, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            if (IsViewerInitialized)
            {
                ReleaseResources();
            }

            if (length < 0)
            {
                length = dataBytes.Length - offset;
            }

            //Copy the bytes to unmanaged memory, so that we don't depend on the original array.
            IntPtr pointer = Marshal.AllocHGlobal(length);
            Marshal.Copy(dataBytes, offset, pointer, length);

            //Wrap the pointer into a disposable container.
            IDisposable disposer = new DisposableIntPtr(pointer);

            OwnsContextAndDocument = true;

            Context = new MuPDFContext();

            //Create a new document, passing the wrapped pointer so that it can be released when the Document is disposed.
            Document = new MuPDFDocument(Context, pointer, length, fileType, ref disposer);

            ContinueInitialization(threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Set up the <see cref="PDFRenderer"/> to display a page of a document that will be loaded from an array of <see cref="byte"/>s. The OCR step is run asynchronously, in order not to block the UI thread.
        /// </summary>
        /// <param name="dataBytes">The bytes of the document that should be opened. The array will be copied and can be safely discarded/altered after this method returns.</param>
        /// <param name="fileType">The format of the document.</param>
        /// <param name="offset">The offset in the byte array at which the document starts.</param>
        /// <param name="length">The length of the document in bytes. If this is &lt; 0, the whole array is used.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        public async Task InitializeAsync(byte[] dataBytes, InputFileTypes fileType, int offset = 0, int length = -1, int threadCount = 0, int pageNumber = 0, double resolutionMultiplier = 1, bool includeAnnotations = true, TesseractLanguage ocrLanguage = null)
        {
            if (IsViewerInitialized)
            {
                ReleaseResources();
            }

            if (length < 0)
            {
                length = dataBytes.Length - offset;
            }

            //Copy the bytes to unmanaged memory, so that we don't depend on the original array.
            IntPtr pointer = Marshal.AllocHGlobal(length);
            Marshal.Copy(dataBytes, offset, pointer, length);

            //Wrap the pointer into a disposable container.
            IDisposable disposer = new DisposableIntPtr(pointer);

            OwnsContextAndDocument = true;

            Context = new MuPDFContext();

            //Create a new document, passing the wrapped pointer so that it can be released when the Document is disposed.
            Document = new MuPDFDocument(Context, pointer, length, fileType, ref disposer);

            await ContinueInitializationAsync(threadCount, pageNumber, resolutionMultiplier, includeAnnotations, ocrLanguage);
        }

        /// <summary>
        /// Common steps in the initialization process that will be performed regardless of how the <see cref="Document"/> was obtained.
        /// </summary>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        private void ContinueInitialization(int threadCount, int pageNumber, double resolutionMultiplier, bool includeAnnotations, TesseractLanguage ocrLanguage = null)
        {
            //Initialise threads and locking mechanics.
            if (RenderMutex == null)
            {
                RenderMutex = new Mutex(false);

                this.RenderDynamicCanvasOuterThread = new Thread(() =>
                {
                    RenderDynamicCanvasOuterAction();
                });

                this.RenderDynamicCanvasInnerThread = new Thread(() =>
                {
                    RenderDynamicCanvasInnerAction();
                });

                RenderDynamicCanvasOuterThread.Start();
                RenderDynamicCanvasInnerThread.Start();
            }

            //Choose an appropriate number of threads based on the number of processors in the computer. We have an upper limit of 8 threads because more threads apparently caused reduced performance due to the synchronisation overhead.
            if (threadCount <= 0)
            {
                threadCount = Math.Max(1, Math.Min(8, Environment.ProcessorCount - 2));
            }

            //Create the structured text representation.
            this.StructuredTextPage = Document.GetStructuredTextPage(pageNumber, ocrLanguage, includeAnnotations);

            //Create the multithreaded renderer.
            Renderer = Document.GetMultiThreadedRenderer(pageNumber, threadCount, includeAnnotations);

            //Set up the properties of this control.
            RenderThreadCount = Renderer.ThreadCount;
            Rectangle bounds = Document.Pages[pageNumber].Bounds;
            PageSize = new Rect(new Point(bounds.X0, bounds.Y0), new Point(bounds.X1, bounds.Y1));
            PageNumber = pageNumber;

            //Render the static canvas (which is used when the DynamicBitmaps are not available).
            RenderFixedCanvas(resolutionMultiplier);

            //Initialize the dynamic canvas.
            InitializeDynamicCanvas();

            //Set initial display area to include the whole page.
            double widthRatio = PageSize.Width / (this.Bounds.Width * resolutionMultiplier);
            double heightRatio = PageSize.Height / (this.Bounds.Height * resolutionMultiplier);

            double containingWidth = Math.Max(widthRatio, heightRatio) * this.Bounds.Width * resolutionMultiplier;
            double containingHeight = Math.Max(widthRatio, heightRatio) * this.Bounds.Height * resolutionMultiplier;

            SetDisplayAreaNowInternal(new Rect(new Point(-(containingWidth - FixedArea.Width) * 0.5, -(containingHeight - FixedArea.Height) * 0.5), new Avalonia.Size(containingWidth, containingHeight)));

            //We are ready!
            IsViewerInitialized = true;

            //Queue a render of the DynamicBitmaps (on another thread).
            RenderDynamicCanvas();
        }


        /// <summary>
        /// Common steps in the initialization process that will be performed regardless of how the <see cref="Document"/> was obtained. The OCR step is run asynchronously, in order not to block the UI thread.
        /// </summary>
        /// <param name="threadCount">The number of threads to use in the rendering. If this is 0, an appropriate number of threads based on the number of processors in the computer will be used. Otherwise, this must be factorisable using only powers of 2, 3, 5 or 7. If this is not the case, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageNumber">The index of the page that should be rendered. The first page has index 0.</param>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the rendering. Otherwise, only the page contents are included.</param>
        /// <param name="ocrLanguage">The language to use for optical character recognition (OCR). If this is null, no OCR is performed.</param>
        private async Task ContinueInitializationAsync(int threadCount, int pageNumber, double resolutionMultiplier, bool includeAnnotations, TesseractLanguage ocrLanguage = null)
        {
            //Initialise threads and locking mechanics.
            if (RenderMutex == null)
            {
                RenderMutex = new Mutex(false);

                this.RenderDynamicCanvasOuterThread = new Thread(() =>
                {
                    RenderDynamicCanvasOuterAction();
                });

                this.RenderDynamicCanvasInnerThread = new Thread(() =>
                {
                    RenderDynamicCanvasInnerAction();
                });

                RenderDynamicCanvasOuterThread.Start();
                RenderDynamicCanvasInnerThread.Start();
            }

            //Choose an appropriate number of threads based on the number of processors in the computer. We have an upper limit of 8 threads because more threads apparently caused reduced performance due to the synchronisation overhead.
            if (threadCount <= 0)
            {
                threadCount = Math.Max(1, Math.Min(8, Environment.ProcessorCount - 2));
            }

            //Create the structured text representation.
            this.StructuredTextPage = await Document.GetStructuredTextPageAsync(pageNumber, ocrLanguage, includeAnnotations);

            //Create the multithreaded renderer.
            Renderer = Document.GetMultiThreadedRenderer(pageNumber, threadCount, includeAnnotations);

            //Set up the properties of this control.
            RenderThreadCount = Renderer.ThreadCount;
            Rectangle bounds = Document.Pages[pageNumber].Bounds;
            PageSize = new Rect(new Point(bounds.X0, bounds.Y0), new Point(bounds.X1, bounds.Y1));
            PageNumber = pageNumber;

            //Render the static canvas (which is used when the DynamicBitmaps are not available).
            RenderFixedCanvas(resolutionMultiplier);

            //Initialize the dynamic canvas.
            InitializeDynamicCanvas();

            //Set initial display area to include the whole page.
            double widthRatio = PageSize.Width / (this.Bounds.Width * resolutionMultiplier);
            double heightRatio = PageSize.Height / (this.Bounds.Height * resolutionMultiplier);

            double containingWidth = Math.Max(widthRatio, heightRatio) * this.Bounds.Width * resolutionMultiplier;
            double containingHeight = Math.Max(widthRatio, heightRatio) * this.Bounds.Height * resolutionMultiplier;

            SetDisplayAreaNowInternal(new Rect(new Point(-(containingWidth - FixedArea.Width) * 0.5, -(containingHeight - FixedArea.Height) * 0.5), new Avalonia.Size(containingWidth, containingHeight)));

            //We are ready!
            IsViewerInitialized = true;

            //Queue a render of the DynamicBitmaps (on another thread).
            RenderDynamicCanvas();
        }

        /// <summary>
        /// Release resources held by this PDFRenderer. This is not an irreversible step: using one of the Initialize overloads after calling this method will restore functionality.
        /// </summary>
        public void ReleaseResources()
        {
            IsViewerInitialized = false;
            this.Renderer?.Dispose();
            this.StructuredTextPage = null;
            this.Selection = null;
            this.HighlightedRegions = null;

            if (OwnsContextAndDocument)
            {
                this.Document?.Dispose();
                this.Context?.Dispose();
            }
        }

        /// <summary>
        /// Called when the PDFRenderer is removed from the logical tree (e.g. it is removed from the window, or the window containing it is closed). We assume that this renderer is not needed anymore. This is irreversible!
        /// </summary>
        private void ControlDetachedFromLogicalTree(object sender, LogicalTreeAttachmentEventArgs e)
        {
            RendererDisposedHandle.Set();
            ReleaseResources();
        }


        /// <summary>
        /// Set the current display area to the specified <paramref name="value"/>, skipping all transitions. This also skips sanity checks of the <paramref name="value"/>, since the calling methods will already have performed them.
        /// </summary>
        /// <param name="value">The new display area.</param>
        private void SetDisplayAreaNowInternal(Rect value)
        {
            Transitions prevTransitions = this.Transitions;
            this.Transitions = null;
            SetValue(DisplayAreaProperty, value);
            this.Transitions = prevTransitions;
        }

        /// <summary>
        /// Set the current display area to the specified <paramref name="value"/>, skipping all transitions.
        /// </summary>
        /// <param name="value">The new display area.</param>
        public void SetDisplayAreaNow(Rect value)
        {
            Transitions prevTransitions = this.Transitions;
            this.Transitions = null;
            this.DisplayArea = value;
            this.Transitions = prevTransitions;
        }

        /// <summary>
        /// Zoom around a point.
        /// </summary>
        /// <param name="count">Number of steps to zoom. Positive values indicate a zoom in, negative values a zoom out.</param>
        /// <param name="center">The point around which to center the zoom operation. If this is null, the center of the control is used.</param>
        public void ZoomStep(double count, Point? center = null)
        {
            if (center == null)
            {
                center = new Point(this.Bounds.Width * 0.5, this.Bounds.Height * 0.5);
            }

            double currZoomX = FixedArea.Width / DisplayArea.Width;
            double currZoomY = FixedArea.Height / DisplayArea.Height;

            currZoomX *= Math.Pow(ZoomIncrement, count);
            currZoomY *= Math.Pow(ZoomIncrement, count);

            double currWidth = FixedArea.Width / currZoomX;
            double currHeight = FixedArea.Height / currZoomY;

            double deltaW = currWidth - DisplayArea.Width;
            double deltaH = currHeight - DisplayArea.Height;

            SetValue(DisplayAreaProperty, new Rect(new Point(DisplayArea.X - deltaW * center.Value.X / this.Bounds.Width, DisplayArea.Y - deltaH * center.Value.Y / this.Bounds.Height), new Point(DisplayArea.Right + deltaW * (1 - center.Value.X / this.Bounds.Width), DisplayArea.Bottom + deltaH * (1 - center.Value.Y / this.Bounds.Height))));
        }

        /// <summary>
        /// Alter the display area so that the whole page fits on screen.
        /// </summary>
        public void Contain()
        {
            //This will be sanitised by the property setter.
            this.DisplayArea = this.PageSize;
        }

        /// <summary>
        /// Alter the display area so that the page covers the whole surface of the <see cref="PDFRenderer"/> (even though parts of the page may be outside it).
        /// </summary>
        public void Cover()
        {
            double widthRatio = this.PageSize.Width / (this.Bounds.Width);
            double heightRatio = this.PageSize.Height / (this.Bounds.Height);

            double containingWidth = Math.Min(widthRatio, heightRatio) * this.Bounds.Width;
            double containingHeight = Math.Min(widthRatio, heightRatio) * this.Bounds.Height;

            double deltaW = (containingWidth - this.PageSize.Width) * 0.5;
            double deltaH = (containingHeight - this.PageSize.Height) * 0.5;

            Rect newDispArea = new Rect(new Point(this.PageSize.X - deltaW, this.PageSize.Y - deltaH), new Point(this.PageSize.Right + deltaW, this.PageSize.Bottom + deltaH));

            //Skip sanitation.
            SetValue(DisplayAreaProperty, newDispArea);
        }

        /// <summary>
        /// Get the current rendering progress.
        /// </summary>
        /// <returns>A <see cref="RenderProgress"/> object with information about the rendering progress of each thread.</returns>
        public RenderProgress GetProgress()
        {
            return Renderer.GetProgress();
        }

        /// <summary>
        /// Get the currently selected text.
        /// </summary>
        /// <returns>The currently selected text.</returns>
        public string GetSelectedText()
        {
            return this.StructuredTextPage.GetText(this.Selection);
        }

        /// <summary>
        /// Selects all the text in the document.
        /// </summary>
        public void SelectAll()
        {
            if (this.StructuredTextPage.Count > 0)
            {
                int maxBlock = this.StructuredTextPage.Count - 1;
                int maxLine = this.StructuredTextPage[maxBlock].Count - 1;
                int maxCharacter = this.StructuredTextPage[maxBlock][maxLine].Count - 1;

                this.Selection = new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(0, 0, 0), new MuPDFStructuredTextAddress(maxBlock, maxLine, maxCharacter));
            }
            else
            {
                this.Selection = null;
            }
        }

        /// <summary>
        /// Highlights all matches of the specified <see cref="Regex"/> in the text and returns the number of matches found. Matches cannot span multiple lines.
        /// </summary>
        /// <param name="needle">The <see cref="Regex"/> to search for.</param>
        /// <returns>The number of matches that have been found.</returns>
        public int Search(Regex needle)
        {
            List<MuPDFStructuredTextAddressSpan> spans = this.StructuredTextPage.Search(needle).ToList();
            this.HighlightedRegions = spans;
            return spans.Count;
        }

        /// <summary>
        /// Render the <see cref="FixedCanvasBitmap"/>. 
        /// </summary>
        /// <param name="resolutionMultiplier">This value can be used to increase or decrease the resolution at which the static renderisation of the page will be produced. If <paramref name="resolutionMultiplier"/> is 1, the resolution will match the size (in screen units) of the <see cref="PDFRenderer"/>.</param>
        private void RenderFixedCanvas(double resolutionMultiplier)
        {
            //Take into account DPI scaling.
            resolutionMultiplier *= (VisualRoot as ILayoutRoot)?.LayoutScaling ?? 1;

            double widthRatio = PageSize.Width / (this.Bounds.Width * resolutionMultiplier);
            double heightRatio = PageSize.Height / (this.Bounds.Height * resolutionMultiplier);

            double zoom = 1 / Math.Min(widthRatio, heightRatio);

            //Render the whole page
            Rectangle origin = new Rectangle(0, 0, PageSize.Width, PageSize.Height);

            FixedArea = origin;

            RoundedRectangle roundedOrigin = origin.Round(zoom);

            RoundedSize targetSize = new RoundedSize(roundedOrigin.Width, roundedOrigin.Height);
            if (FixedCanvasBitmap == null)
            {
                FixedCanvasBitmap = new WriteableBitmap(new PixelSize(targetSize.Width, targetSize.Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Unpremul);
            }
            else
            {
                if (FixedCanvasBitmap.PixelSize.Width != targetSize.Width || FixedCanvasBitmap.PixelSize.Height != targetSize.Height)
                {
                    FixedCanvasBitmap = new WriteableBitmap(new PixelSize(targetSize.Width, targetSize.Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Unpremul);
                }
            }

            //Render the page to the FixedCanvasBitmap (without marshaling).
            using (ILockedFramebuffer fb = FixedCanvasBitmap.Lock())
            {
                Document.Render(PageNumber, origin, zoom, PixelFormats.RGBA, fb.Address);
            }
        }

        /// <summary>
        /// Set up the <see cref="DynamicBitmaps"/> array with an appropriate number of <see cref="WriteableBitmap"/> of the appropriate size.
        /// </summary>
        private void InitializeDynamicCanvas()
        {
            //Take into account DPI scaling.
            double scale = (VisualRoot as ILayoutRoot)?.LayoutScaling ?? 1;

            //Acquire the render mutex (we don't want anyone to touch the DynamicBitmaps while we are resizing them!)
            RenderMutex.WaitOne();
            RoundedSize targetSize = new RoundedSize((int)Math.Ceiling(this.Bounds.Width * scale), (int)Math.Ceiling(this.Bounds.Height * scale));

            //Split the target size into an appropriate number of tiles.
            RoundedRectangle[] splitSizes = targetSize.Split(RenderThreadCount);

            DynamicImagesBounds = splitSizes;

            if (DynamicBitmaps == null || DynamicBitmaps.Length != RenderThreadCount)
            {
                DynamicBitmaps = new WriteableBitmap[RenderThreadCount];
                for (int i = 0; i < splitSizes.Length; i++)
                {
                    DynamicBitmaps[i] = new WriteableBitmap(new PixelSize(splitSizes[i].Width, splitSizes[i].Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Unpremul);
                }
            }
            else
            {
                for (int i = 0; i < splitSizes.Length; i++)
                {
                    if (DynamicBitmaps[i].PixelSize.Width != splitSizes[i].Width || DynamicBitmaps[i].PixelSize.Height != splitSizes[i].Height)
                    {
                        DynamicBitmaps[i] = new WriteableBitmap(new PixelSize(splitSizes[i].Width, splitSizes[i].Height), new Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888, AlphaFormat.Unpremul);
                    }
                }
            }

            //Release the render mutex.
            RenderMutex.ReleaseMutex();
        }

        /// <summary>
        /// The outer loop that is executed by the <see cref="RenderDynamicCanvasOuterThread"/>, which is in charge of responding to the rendering requests and either starting a new rendering of the <see cref="DynamicBitmaps"/>, or queueing it.
        /// </summary>
        private void RenderDynamicCanvasOuterAction()
        {
            EventWaitHandle[] handles = new EventWaitHandle[] { RenderDynamicCanvasOuterHandle, RendererDisposedHandle };

            while (true)
            {
                int result = EventWaitHandle.WaitAny(handles);

                if (result == 0)
                {
                    //So that we don't lose consecutive requests.
                    RenderDynamicCanvasOuterHandle.Reset();

                    //Check if the rendering is already in progress.
                    if (RenderMutex.WaitOne(0))
                    {
                        //Start a new rendering
                        AreDynamicBitmapsReady = false;

                        //This handle will be set by the inner thread once it starts rendering.
                        RenderDynamicCanvasInnerStartedHandle.Reset();

                        //Tell the inner thread to start rendering.
                        RenderDynamicCanvasInnerHandle.Set();

                        //Release the mutex so that the inner thread can start rendering.
                        RenderMutex.ReleaseMutex();

                        //Wait until the inner thread has acuired the mutex and started rendering.
                        RenderDynamicCanvasInnerStartedHandle.WaitOne();
                    }
                    else
                    {
                        if (!RenderQueued)
                        {
                            //Queue another rendering pass.
                            RenderQueued = true;

                            //Abort the current rendering pass.
                            Renderer.Abort();
                        }
                    }
                }
                else
                {
                    //The renderer is being disposed, we need to quit!
                    break;
                }
            }
        }

        /// <summary>
        /// The inner loop that is executed by the <see cref="RenderDynamicCanvasInnerThread"/>, which renders the <see cref="DynamicBitmaps"/>.
        /// </summary>
        private void RenderDynamicCanvasInnerAction()
        {
            EventWaitHandle[] handles = new EventWaitHandle[] { RenderDynamicCanvasInnerHandle, RendererDisposedHandle };

            bool ownsMutex = false;

            while (true)
            {
                int result = EventWaitHandle.WaitAny(handles);

                if (result == 0)
                {
                    //So that we don't lose consecutive requests.
                    RenderDynamicCanvasInnerHandle.Reset();

                    //Acquire the mutex only if have not acquired it yet.
                    if (!ownsMutex)
                    {
                        RenderMutex.WaitOne();
                        ownsMutex = true;
                    }

                    //Signal to the outer thread that we have acquired the mutex. Even if the outer thread is not waiting for this signal, it will reset it before waiting for it.
                    RenderDynamicCanvasInnerStartedHandle.Set();

                    //Set up the pointers to the contents of the DynamicBitmaps
                    IntPtr[] destinations = new IntPtr[RenderThreadCount];
                    ILockedFramebuffer[] fbs = new ILockedFramebuffer[RenderThreadCount];

                    for (int i = 0; i < RenderThreadCount; i++)
                    {
                        fbs[i] = DynamicBitmaps[i].Lock();
                        destinations[i] = fbs[i].Address;
                    }

                    //Prevent race conditions.
                    Rectangle target;
                    int width;
                    int height;
                    lock (RenderDisplayAreaLock)
                    {
                        target = new Rectangle(RenderDisplayArea.X, RenderDisplayArea.Y, RenderDisplayArea.Right, RenderDisplayArea.Bottom);
                        width = RenderSize[0];
                        height = RenderSize[1];
                    }

                    //Start the multithreaded rendering and wait until it finishes.
                    Renderer.Render(new RoundedSize(width, height), target, destinations, PixelFormats.RGBA);

                    //Free the pointers.
                    for (int i = 0; i < RenderThreadCount; i++)
                    {
                        fbs[i].Dispose();
                    }

                    //Check whether another rendering request has been queued.
                    if (!RenderQueued)
                    {
                        //No other rendering requests have been queued.
                        AreDynamicBitmapsReady = true;

                        //This should always be true. Release the rendering mutex.
                        if (ownsMutex)
                        {
                            RenderMutex.ReleaseMutex();
                            ownsMutex = false;
                        }

                        //Signal to the UI that a repaint is required.
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            this.InvalidateVisual();
                        });
                    }
                    else
                    {
                        //Another rendering request has been queued, we can assume that whatever we have rendered until now is useless (maybe because the rendering has been aborted).
                        RenderQueued = false;

                        //Self-signal.
                        RenderDynamicCanvasInnerHandle.Set();
                    }
                }
                else
                {
                    //The renderer is being disposed, we need to quit!
                    break;
                }
            }
        }

        /// <summary>
        /// Signal to the <see cref="RenderDynamicCanvasOuterThread"/> that a rendering has been requested.
        /// </summary>
        private void RenderDynamicCanvas()
        {
            //Take into account DPI scaling.
            double scale = (VisualRoot as ILayoutRoot).LayoutScaling;

            //Set up rendering size
            lock (RenderDisplayAreaLock)
            {
                RenderSize[0] = (int)Math.Ceiling(this.Bounds.Width * scale);
                RenderSize[1] = (int)Math.Ceiling(this.Bounds.Height * scale);
                RenderDisplayArea = DisplayArea;
            }

            //Send the signal.
            RenderDynamicCanvasOuterHandle.Set();
        }

        /// <summary>
        /// Resizes the <see cref="DynamicBitmaps"/> when the size of the control changes and queues a repaint when the <see cref="DisplayArea"/> changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == UserControl.BoundsProperty)
            {
                if (IsViewerInitialized)
                {
                    //Resize the display area
                    Rect oldBounds = (Rect)e.OldValue;
                    Rect newBounds = (Rect)e.NewValue;

                    double deltaW = (newBounds.Width - oldBounds.Width) / oldBounds.Width * DisplayArea.Width;
                    double deltaH = (newBounds.Height - oldBounds.Height) / oldBounds.Height * DisplayArea.Height;

                    Rect target = new Rect(new Point(DisplayArea.X - deltaW * 0.5, DisplayArea.Y - deltaH * 0.5), new Point(DisplayArea.Right + deltaW * 0.5, DisplayArea.Bottom + deltaH * 0.5));

                    //Resize the DynamicBitmaps
                    InitializeDynamicCanvas();

                    //Set the new DisplayArea, skipping any animation.
                    SetDisplayAreaNowInternal(target);
                }
            }
            else if (e.Property == PDFRenderer.DisplayAreaProperty)
            {
                if (IsViewerInitialized)
                {
                    //Update the value of the Zoom property.
                    ComputeZoom();

                    //Signal that a repaint is needed
                    this.InvalidateVisual();

                    //Queue a new rendering of the DynamicBitmaps
                    RenderDynamicCanvas();
                }
            }
            else if (e.Property == PDFRenderer.SelectionProperty && this.StructuredTextPage != null)
            {
                //Update the selection quads to reflect the new selection
                this.SelectionQuads = this.StructuredTextPage.GetHighlightQuads(this.Selection, false).ToList();
                this.InvalidateVisual();
            }
            else if (e.Property == PDFRenderer.HighlightedRegionsProperty && this.StructuredTextPage != null)
            {
                //Update the highlight quads to reflect the new highlighted regions
                this.HighlightQuads = new List<Quad>();

                if (this.HighlightedRegions != null)
                {
                    foreach (MuPDFStructuredTextAddressSpan span in this.HighlightedRegions)
                    {
                        this.HighlightQuads.AddRange(this.StructuredTextPage.GetHighlightQuads(span, false));
                    }
                }

                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// Default handler for the PointerPressed event (start panning).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (PointerEventHandlersType == PointerEventHandlers.Pan)
            {
                if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
                {
                    IsMouseDown = true;
                    MouseDownPoint = e.GetPosition(this);
                    MouseDownDisplayArea = DisplayArea;
                    this.Cursor = new Cursor(StandardCursorType.SizeAll);
                }
            }
            else if (PointerEventHandlersType == PointerEventHandlers.Highlight)
            {
                if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
                {
                    Point point = e.GetPosition(this);

                    IsMouseDown = true;
                    MouseDownPoint = point;
                    MouseDownDisplayArea = DisplayArea;

                    PointF pagePoint = new PointF((float)(point.X / this.Bounds.Width * DisplayArea.Width + DisplayArea.Left), (float)(point.Y / this.Bounds.Height * DisplayArea.Height + DisplayArea.Top));

                    MuPDFStructuredTextAddress? address = StructuredTextPage?.GetHitAddress(pagePoint, false);

                    if (address != null)
                    {
                        this.Selection = new MuPDFStructuredTextAddressSpan(address.Value, null);
                    }
                    else
                    {
                        this.Selection = null;
                    }
                }
            }
            else if (PointerEventHandlersType == PointerEventHandlers.PanHighlight)
            {
                Point point = e.GetPosition(this);
                PointF pagePoint = new PointF((float)(point.X / this.Bounds.Width * DisplayArea.Width + DisplayArea.Left), (float)(point.Y / this.Bounds.Height * DisplayArea.Height + DisplayArea.Top));
                MuPDFStructuredTextAddress? address = StructuredTextPage?.GetHitAddress(pagePoint, false);

                if (address == null)
                {
                    if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
                    {
                        IsMouseDown = true;
                        MouseDownPoint = e.GetPosition(this);
                        MouseDownDisplayArea = DisplayArea;
                        this.Cursor = new Cursor(StandardCursorType.SizeAll);
                        CurrentMouseOperation = CurrentMouseOperations.Pan;
                    }
                }
                else
                {
                    if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
                    {
                        IsMouseDown = true;
                        MouseDownPoint = point;
                        MouseDownDisplayArea = DisplayArea;

                        this.Selection = new MuPDFStructuredTextAddressSpan(address.Value, null);
                        CurrentMouseOperation = CurrentMouseOperations.Highlight;
                    }
                }
            }
        }

        /// <summary>
        /// Default handler for the PointerReleased event (finish panning).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (PointerEventHandlersType == PointerEventHandlers.Pan)
            {
                if (e.InitialPressMouseButton == MouseButton.Left)
                {
                    IsMouseDown = false;
                    this.Cursor = new Cursor(StandardCursorType.Arrow);
                }
            }
            else if (PointerEventHandlersType == PointerEventHandlers.Highlight)
            {
                if (e.InitialPressMouseButton == MouseButton.Left)
                {
                    IsMouseDown = false;
                    if (e.GetPosition(this).Equals(MouseDownPoint))
                    {
                        this.Selection = null;
                    }
                }
            }
            else if (PointerEventHandlersType == PointerEventHandlers.PanHighlight)
            {
                if (e.InitialPressMouseButton == MouseButton.Left)
                {
                    IsMouseDown = false;
                    if (CurrentMouseOperation == CurrentMouseOperations.Pan)
                    {
                        this.Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                    if (e.GetPosition(this).Equals(MouseDownPoint))
                    {
                        this.Selection = null;
                    }
                }
            }
        }

        /// <summary>
        /// Default handler for the PointerMoved event (pan).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlPointerMoved(object sender, PointerEventArgs e)
        {
            if (IsMouseDown)
            {
                if (PointerEventHandlersType == PointerEventHandlers.Pan || (PointerEventHandlersType == PointerEventHandlers.PanHighlight && CurrentMouseOperation == CurrentMouseOperations.Pan))
                {

                    Point point = e.GetPosition(this);

                    double deltaX = (-point.X + MouseDownPoint.X) / this.Bounds.Width * DisplayArea.Width;
                    double deltaY = (-point.Y + MouseDownPoint.Y) / this.Bounds.Height * DisplayArea.Height;

                    Rect target = new Rect(new Point(this.MouseDownDisplayArea.X + deltaX, this.MouseDownDisplayArea.Y + deltaY), new Point(this.MouseDownDisplayArea.Right + deltaX, this.MouseDownDisplayArea.Bottom + deltaY));

                    SetDisplayAreaNowInternal(target);
                    this.Cursor = new Cursor(StandardCursorType.SizeAll);

                }
                else if (PointerEventHandlersType == PointerEventHandlers.Highlight || (PointerEventHandlersType == PointerEventHandlers.PanHighlight && CurrentMouseOperation == CurrentMouseOperations.Highlight))
                {

                    Point point = e.GetPosition(this);

                    PointF pagePoint = new PointF((float)(point.X / this.Bounds.Width * DisplayArea.Width + DisplayArea.Left), (float)(point.Y / this.Bounds.Height * DisplayArea.Height + DisplayArea.Top));

                    MuPDFStructuredTextAddress? address = StructuredTextPage?.GetClosestHitAddress(pagePoint, false);

                    this.Selection = new MuPDFStructuredTextAddressSpan(this.Selection.Start, address);

                    if (address != null)
                    {
                        this.Cursor = new Cursor(StandardCursorType.Ibeam);
                    }
                    else
                    {
                        this.Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                }
            }
            else
            {
                if (PointerEventHandlersType == PointerEventHandlers.Highlight || PointerEventHandlersType == PointerEventHandlers.PanHighlight)
                {
                    Point point = e.GetPosition(this);

                    PointF pagePoint = new PointF((float)(point.X / this.Bounds.Width * DisplayArea.Width + DisplayArea.Left), (float)(point.Y / this.Bounds.Height * DisplayArea.Height + DisplayArea.Top));

                    MuPDFStructuredTextAddress? address = StructuredTextPage?.GetHitAddress(pagePoint, false);

                    if (address != null)
                    {
                        this.Cursor = new Cursor(StandardCursorType.Ibeam);
                    }
                    else
                    {
                        this.Cursor = new Cursor(StandardCursorType.Arrow);
                    }
                }
                else
                {
                    this.Cursor = new Cursor(StandardCursorType.Arrow);
                }
            }
        }

        /// <summary>
        /// Default handler for the PointerWheelChanged event (zoom in/out).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ControlPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (ZoomEnabled)
            {
                ZoomStep(e.Delta.Y, e.GetPosition(this));
            }
        }

        /// <summary>
        /// Compute the current value of the <see cref="Zoom"/> property.
        /// </summary>
        private void ComputeZoom()
        {
            //Take into account DPI scaling.
            double scale = (VisualRoot as ILayoutRoot).LayoutScaling;
            SetAndRaise(ZoomProperty, ref _Zoom, this.Bounds.Width / DisplayArea.Width * 72 / 96 * scale);
        }

        /// <summary>
        /// Draw the rendered document.
        /// </summary>
        /// <param name="context">The drawing context on which to draw.</param>
        public override void Render(DrawingContext context)
        {
            //Take into account DPI scaling.
            double scale = (VisualRoot as ILayoutRoot)?.LayoutScaling ?? 1;

            context.FillRectangle(Background, this.Bounds);

            //Page boundaries (used to draw the page background).
            double minX = Math.Max(PageSize.Left, DisplayArea.Left);
            double maxX = Math.Min(PageSize.Right, DisplayArea.Right);
            double minY = Math.Max(PageSize.Top, DisplayArea.Top);
            double maxY = Math.Min(PageSize.Bottom, DisplayArea.Bottom);


            if (IsViewerInitialized)
            {
                bool renderedDynamic = false;

                //Check if someone is holding the mutex without blocking.
                if (RenderMutex.WaitOne(0))
                {
                    //Check if the DynamicBitmaps are ready
                    if (AreDynamicBitmapsReady)
                    {
                        //Page background
                        context.FillRectangle(PageBackground, new Rect(new Point((minX - DisplayArea.Left) / DisplayArea.Width * this.Bounds.Width, (minY - DisplayArea.Top) / DisplayArea.Height * this.Bounds.Height), new Point((maxX - DisplayArea.Left) / DisplayArea.Width * this.Bounds.Width, (maxY - DisplayArea.Top) / DisplayArea.Height * this.Bounds.Height)));

                        //Draw the DynamicBitmaps.
                        for (int i = 0; i < DynamicImagesBounds.Length; i++)
                        {
                            context.DrawImage(DynamicBitmaps[i], new Rect(new Point(0, 0), DynamicBitmaps[i].PixelSize.ToSize(1)), new Rect(DynamicImagesBounds[i].X0 / scale, DynamicImagesBounds[i].Y0 / scale, DynamicImagesBounds[i].Width / scale, DynamicImagesBounds[i].Height / scale));
                        }

                        //Signal that we don't need to draw the static image.
                        renderedDynamic = true;
                    }

                    //Release the mutex.
                    RenderMutex.ReleaseMutex();
                }

                //If the DynamicBitmaps have not been drawn, we fall back to drawing the static image (which will probably be ugly and pixelated, but better than nothing).
                if (!renderedDynamic)
                {
                    //Page background
                    context.FillRectangle(PageBackground, new Rect(new Point((minX - DisplayArea.Left) / DisplayArea.Width * this.Bounds.Width, (minY - DisplayArea.Top) / DisplayArea.Height * this.Bounds.Height), new Point((maxX - DisplayArea.Left) / DisplayArea.Width * this.Bounds.Width, (maxY - DisplayArea.Top) / DisplayArea.Height * this.Bounds.Height)));

                    //Top left corner of the DisplayArea in FixedCanvasBitmap coordinates.
                    Point topLeft = new Point((DisplayArea.X - FixedArea.X0) / FixedArea.Width * FixedCanvasBitmap.PixelSize.Width, (DisplayArea.Y - FixedArea.Y0) / FixedArea.Height * FixedCanvasBitmap.PixelSize.Height);

                    //Size of the DisplayArea in FixedCanvasBitmap coordinates.
                    Avalonia.Size size = new Avalonia.Size(DisplayArea.Width / FixedArea.Width * FixedCanvasBitmap.PixelSize.Width, DisplayArea.Height / FixedArea.Height * FixedCanvasBitmap.PixelSize.Height);

                    //Draw the FixedCanvasBitmap
                    context.DrawImage(FixedCanvasBitmap, new Rect(topLeft, size), new Rect(0, 0, this.Bounds.Width, this.Bounds.Height));

                    //Draw the icon signaling that the DynamicBitmaps are still being rendered.
                    RefreshingGeometry.Transform = new TranslateTransform(this.Bounds.Width - 38, 32);
                    context.DrawGeometry(new SolidColorBrush(Color.FromRgb(119, 170, 221)), null, RefreshingGeometry);
                }

                //Draw the highlight quads
                if (this.HighlightQuads != null && this.HighlightQuads.Count > 0)
                {
                    PathGeometry highlightGeometry = new PathGeometry() { FillRule = FillRule.NonZero };

                    for (int i = 0; i < this.HighlightQuads.Count; i++)
                    {
                        Point ll = new Point((this.HighlightQuads[i].LowerLeft.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.HighlightQuads[i].LowerLeft.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);
                        Point ul = new Point((this.HighlightQuads[i].UpperLeft.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.HighlightQuads[i].UpperLeft.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);
                        Point ur = new Point((this.HighlightQuads[i].UpperRight.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.HighlightQuads[i].UpperRight.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);
                        Point lr = new Point((this.HighlightQuads[i].LowerRight.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.HighlightQuads[i].LowerRight.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);

                        PathFigure quad = new PathFigure() { StartPoint = ll, IsClosed = true, IsFilled = true };
                        quad.Segments.Add(new LineSegment() { Point = ul });
                        quad.Segments.Add(new LineSegment() { Point = ur });
                        quad.Segments.Add(new LineSegment() { Point = lr });

                        highlightGeometry.Figures.Add(quad);
                    }

                    context.DrawGeometry(this.HighlightBrush, null, highlightGeometry);
                }

                //Draw the selection quads
                if (this.SelectionQuads != null && this.SelectionQuads.Count > 0)
                {
                    PathGeometry selectionGeometry = new PathGeometry() { FillRule = FillRule.NonZero };

                    for (int i = 0; i < this.SelectionQuads.Count; i++)
                    {
                        Point ll = new Point((this.SelectionQuads[i].LowerLeft.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.SelectionQuads[i].LowerLeft.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);
                        Point ul = new Point((this.SelectionQuads[i].UpperLeft.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.SelectionQuads[i].UpperLeft.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);
                        Point ur = new Point((this.SelectionQuads[i].UpperRight.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.SelectionQuads[i].UpperRight.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);
                        Point lr = new Point((this.SelectionQuads[i].LowerRight.X - this.DisplayArea.Left) * this.Bounds.Width / this.DisplayArea.Width, (this.SelectionQuads[i].LowerRight.Y - this.DisplayArea.Top) * this.Bounds.Height / this.DisplayArea.Height);

                        PathFigure quad = new PathFigure() { StartPoint = ll, IsClosed = true, IsFilled = true };
                        quad.Segments.Add(new LineSegment() { Point = ul });
                        quad.Segments.Add(new LineSegment() { Point = ur });
                        quad.Segments.Add(new LineSegment() { Point = lr });

                        selectionGeometry.Figures.Add(quad);
                    }

                    context.DrawGeometry(this.SelectionBrush, null, selectionGeometry);
                }
            }
        }
    }
}
