/*
    MuPDFCore - A set of multiplatform .NET Core bindings for MuPDF.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace MuPDFCore
{
    /// <summary>
    /// A reusable thread to render part of an image.
    /// </summary>
    internal class RenderingThread : IDisposable
    {
        /// <summary>
        /// A class to hold the data used internally by the rendering thread.
        /// </summary>
        private class RenderData
        {
            public IntPtr Context;
            public MuPDFDisplayList DisplayList;
            public Rectangle Region;
            public float Zoom;
            public IntPtr PixelStorage;
            public Rectangle PageBounds;
            public PixelFormats PixelFormat;
            public bool ClipToPageBounds;
        }

        /// <summary>
        /// A lock object to prevent race conditions.
        /// </summary>
        private readonly object RenderDataLock;

        /// <summary>
        /// The data used internally by the rendering thread.
        /// </summary>
        private readonly RenderData CurrentRenderData;

        /// <summary>
        /// The actual thread that does the rendering.
        /// </summary>
        private readonly Thread Thread;

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that is set by the <see cref="Thread"/> when it finished rendering.
        /// </summary>
        private readonly EventWaitHandle SignalFromThread;

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that signals to the <see cref="Thread"/> that it should start rendering.
        /// </summary>
        private readonly EventWaitHandle SignalToThread;

        /// <summary>
        /// An <see cref="EventWaitHandle"/> that signals that the object is being disposed and all activity should cease.
        /// </summary>
        private readonly EventWaitHandle DisposeSignal;

        /// <summary>
        /// A pointer to a <see cref="Cookie"/> object that can be used to monitor the progress of the rendering or to abort it.
        /// </summary>
        private readonly IntPtr Cookie;

        /// <summary>
        /// Performs the actual rendering.
        /// </summary>
        private void RenderAction()
        {
            ExitCodes result = (ExitCodes)NativeMethods.RenderSubDisplayList(this.CurrentRenderData.Context, this.CurrentRenderData.DisplayList.NativeDisplayList, this.CurrentRenderData.Region.X0, this.CurrentRenderData.Region.Y0, this.CurrentRenderData.Region.X1, this.CurrentRenderData.Region.Y1, this.CurrentRenderData.Zoom, (int)this.CurrentRenderData.PixelFormat, this.CurrentRenderData.PixelStorage, Cookie);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            RoundedRectangle roundedRegion = this.CurrentRenderData.Region.Round(this.CurrentRenderData.Zoom);
            RoundedSize roundedSize = new RoundedSize(roundedRegion.Width, roundedRegion.Height);

            if (this.CurrentRenderData.PixelFormat == PixelFormats.RGBA || this.CurrentRenderData.PixelFormat == PixelFormats.BGRA)
            {
                Utils.UnpremultiplyAlpha(this.CurrentRenderData.PixelStorage, roundedSize);
            }

            if (this.CurrentRenderData.ClipToPageBounds && !this.CurrentRenderData.PageBounds.Contains(this.CurrentRenderData.DisplayList.Bounds.Intersect(this.CurrentRenderData.Region)))
            {
                Utils.ClipImage(this.CurrentRenderData.PixelStorage, roundedSize, this.CurrentRenderData.Region, this.CurrentRenderData.PageBounds, this.CurrentRenderData.PixelFormat);
            }
        }

        /// <summary>
        /// Create a new <see cref="RenderingThread"/> instance.
        /// </summary>
        public RenderingThread()
        {
            //Initialize fields
            SignalFromThread = new EventWaitHandle(false, EventResetMode.ManualReset);
            SignalToThread = new EventWaitHandle(false, EventResetMode.ManualReset);
            DisposeSignal = new EventWaitHandle(false, EventResetMode.ManualReset);

            CurrentRenderData = new RenderData();
            RenderDataLock = new object();

            //Allocate unmanaged memory to hold the cookie.
            Cookie = Marshal.AllocHGlobal(Marshal.SizeOf<MuPDFCore.Cookie>());

            //Initialise the rendering thread.
            this.Thread = new Thread(() =>
            {
                EventWaitHandle[] handles = new EventWaitHandle[] { SignalToThread, DisposeSignal };

                while (true)
                {
                    int handle = EventWaitHandle.WaitAny(handles);

                    if (handle == 0)
                    {
                        SignalToThread.Reset();

                        lock (RenderDataLock)
                        {
                            this.RenderAction();
                        }

                        SignalFromThread.Set();
                    }
                    else
                    {
                        SignalFromThread.Set();
                        break;
                    }
                }
            });

            //Start the rendering thread.
            this.Thread.Start();
        }

        /// <summary>
        /// Start rendering a region of a display list to the specified destination.
        /// </summary>
        /// <param name="context">The rendering context.</param>
        /// <param name="displayList">The native display list that should be rendered.</param>
        /// <param name="region">The region that should be rendered.</param>
        /// <param name="zoom">The scale at which the region will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelStorage">The address of the buffer where the pixel data will be written. There must be enough space available to write the values for all the pixels, otherwise this will fail catastrophically!</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="pageBounds">The bounds of the page being rendererd.</param>
        /// <param name="clipToPageBounds">A boolean value indicating whether the rendered image should be clipped to the original page's bounds. This can be relevant if the page has been "cropped" by altering its mediabox, but otherwise leaving the contents untouched.</param>
        public void Render(IntPtr context, MuPDFDisplayList displayList, Rectangle region, float zoom, IntPtr pixelStorage, PixelFormats pixelFormat, Rectangle pageBounds, bool clipToPageBounds)
        {
            lock (RenderDataLock)
            {
                //Reset the cookie.
                unsafe
                {
                    Cookie* cookie = (Cookie*)Cookie;

                    cookie->abort = 0;
                    cookie->errors = 0;
                    cookie->incomplete = 0;
                    cookie->progress = 0;
                    cookie->progress_max = 0;
                }

                //Set up all the rendering data.
                CurrentRenderData.Context = context;
                CurrentRenderData.DisplayList = displayList;
                CurrentRenderData.Region = region;
                CurrentRenderData.Zoom = zoom;
                CurrentRenderData.PixelStorage = pixelStorage;
                CurrentRenderData.PixelFormat = pixelFormat;
                CurrentRenderData.PageBounds = pageBounds;
                CurrentRenderData.ClipToPageBounds = clipToPageBounds;
                SignalToThread.Set();
            }
        }

        /// <summary>
        /// Wait until the current rendering operation finishes.
        /// </summary>
        public void WaitForRendering()
        {
            EventWaitHandle[] handles = new EventWaitHandle[] { SignalFromThread, DisposeSignal };
            int result = EventWaitHandle.WaitAny(handles);
            if (result == 0)
            {
                SignalFromThread.Reset();
            }
        }

        /// <summary>
        /// Abort the current rendering operation.
        /// </summary>
        public void AbortRendering()
        {
            lock (RenderDataLock)
            {
                unsafe
                {
                    Cookie* cookie = (Cookie*)Cookie;
                    cookie->abort = 1;
                }
            }
        }

        /// <summary>
        /// Get the progress of the current rendering operation.
        /// </summary>
        /// <returns>A <see cref="RenderProgress.ThreadRenderProgress"/> object containing the progress of the current rendering operation.</returns>
        public RenderProgress.ThreadRenderProgress GetProgress()
        {
            int progress;
            ulong maxProgress;

            unsafe
            {
                Cookie* cookie = (Cookie*)Cookie;

                progress = cookie->progress;
                maxProgress = cookie->progress_max;
            }

            return new RenderProgress.ThreadRenderProgress(progress, maxProgress);
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeSignal.Set();
                    Thread.Join();
                }

                Marshal.FreeHGlobal(Cookie);

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// A class that holds the necessary resources to render a page of a MuPDF document using multiple threads.
    /// </summary>
    public class MuPDFMultiThreadedPageRenderer : IDisposable
    {
        /// <summary>
        /// The display list that is rendered by this renderer.
        /// </summary>
        private readonly MuPDFDisplayList DisplayList;

        /// <summary>
        /// The cloned contexts that are used by the <see cref="RenderingThreads"/> to render the display list.
        /// </summary>
        private readonly MuPDFContext[] Contexts;

        /// <summary>
        /// The <see cref="RenderingThreads"/> that are in charge of the actual rendering.
        /// </summary>
        private readonly RenderingThread[] RenderingThreads;

        /// <summary>
        /// The bounds of the page being rendered.
        /// </summary>
        private readonly Rectangle PageBounds;

        /// <summary>
        /// Whether the rendered images should be clipped to the bounds of the page being rendered.
        /// </summary>
        private readonly bool ClipToPageBounds;

        /// <summary>
        /// The number of threads that are used to render the image.
        /// </summary>
        public int ThreadCount { get; }

        /// <summary>
        /// If the document is an image, the horizontal resolution of the image. Otherwise, 72.
        /// </summary>
        internal double ImageXRes = double.NaN;

        /// <summary>
        /// If the document is an image, the vertical resolution of the image. Otherwise, 72.
        /// </summary>
        internal double ImageYRes = double.NaN;

        /// <summary>
        /// Create a new <see cref="MuPDFMultiThreadedPageRenderer"/> from a specified display list using the specified number of threads.
        /// </summary>
        /// <param name="context">The context that owns the document from which the display list was extracted.</param>
        /// <param name="displayList">The display list to render.</param>
        /// <param name="threadCount">The number of threads to use in the rendering. This must be factorisable using only powers of 2, 3, 5 or 7. Otherwise, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <param name="pageBounds">The bounds of the page being rendererd.</param>
        /// <param name="clipToPageBounds">A boolean value indicating whether the rendered image should be clipped to the original page's bounds. This can be relevant if the page has been "cropped" by altering its mediabox, but otherwise leaving the contents untouched.</param>
        /// <param name="imageXRes">If the document is an image, the horizontal resolution of the image. Otherwise, 72.</param>
        /// <param name="imageYRes">If the document is an image, the vertical resolution of the image. Otherwise, 72.</param>
        internal MuPDFMultiThreadedPageRenderer(MuPDFContext context, MuPDFDisplayList displayList, int threadCount, Rectangle pageBounds, bool clipToPageBounds, double imageXRes, double imageYRes)
        {
            threadCount = Utils.GetAcceptableNumber(threadCount);

            this.ThreadCount = threadCount;
            this.DisplayList = displayList;
            this.PageBounds = pageBounds;
            this.ClipToPageBounds = clipToPageBounds;

            this.ImageXRes = imageXRes;
            this.ImageYRes = imageYRes;

            IntPtr[] contexts = new IntPtr[threadCount];
            GCHandle contextsHandle = GCHandle.Alloc(contexts, GCHandleType.Pinned);

            try
            {
                ExitCodes result = (ExitCodes)NativeMethods.CloneContext(context.NativeContext, threadCount, contextsHandle.AddrOfPinnedObject());

                switch (result)
                {
                    case ExitCodes.EXIT_SUCCESS:
                        break;
                    case ExitCodes.ERR_CANNOT_INIT_MUTEX:
                        throw new MuPDFException("Cannot initalize mutex objects", result);
                    case ExitCodes.ERR_CANNOT_CREATE_CONTEXT:
                        throw new MuPDFException("Cannot create master context", result);
                    case ExitCodes.ERR_CANNOT_CLONE_CONTEXT:
                        throw new MuPDFException("Cannot create context clones", result);
                    default:
                        throw new MuPDFException("Unknown error", result);
                }

                Contexts = new MuPDFContext[threadCount];
                RenderingThreads = new RenderingThread[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    Contexts[i] = new MuPDFContext(contexts[i]);
                    RenderingThreads[i] = new RenderingThread();
                }
            }
            finally
            {
                contextsHandle.Free();
            }
        }

        /// <summary>
        /// Render the specified region to an image of the specified size, split in a number of tiles equal to the number of threads used by this <see cref="MuPDFMultiThreadedPageRenderer"/>, without marshaling. This method will not return until all the rendering threads have finished.
        /// </summary>
        /// <param name="targetSize">The total size of the image that should be rendered.</param>
        /// <param name="region">The region in page units that should be rendered.</param>
        /// <param name="destinations">An array containing the addresses of the buffers where the rendered tiles will be written. There must be enough space available in each buffer to write the values for all the pixels of the tile, otherwise this will fail catastrophically!
        /// As long as the <paramref name="targetSize"/> is the same, the size in pixel of the tiles is guaranteed to also be the same.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        public void Render(RoundedSize targetSize, Rectangle region, IntPtr[] destinations, PixelFormats pixelFormat)
        {
            if (destinations.Length != Contexts.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(destinations), destinations.Length, "The number of destinations must be equal to the number of rendering threads!");
            }

            RoundedRectangle[] targets = targetSize.Split(destinations.Length);

            float zoomX = targetSize.Width / region.Width;
            float zoomY = targetSize.Height / region.Height;

            float zoom = (float)Math.Sqrt(zoomX * zoomY);

            Rectangle actualPageArea = new Rectangle(region.X0, region.Y0, region.X0 + targetSize.Width / zoom, region.Y0 + targetSize.Height / zoom);

            Rectangle[] origins = actualPageArea.Split(destinations.Length);

            //Make sure that each tile has the expected size in pixel, rounding errors notwithstanding.
            for (int i = 0; i < origins.Length; i++)
            {
                int countBlanks = 0;
                RoundedRectangle roundedOrigin = origins[i].Round(zoom);
                while (roundedOrigin.Width != targets[i].Width || roundedOrigin.Height != targets[i].Height)
                {
                    RoundedRectangle oldRoundedOrigin = roundedOrigin;

                    if (roundedOrigin.Width > targets[i].Width)
                    {
                        if (origins[i].X0 > actualPageArea.X0)
                        {
                            origins[i] = new Rectangle(origins[i].X0 + 0.5 / zoom, origins[i].Y0, origins[i].X1, origins[i].Y1);
                        }
                        else
                        {
                            origins[i] = new Rectangle(origins[i].X0, origins[i].Y0, origins[i].X1 - 0.5 / zoom, origins[i].Y1);
                        }
                    }
                    else if (roundedOrigin.Width < targets[i].Width)
                    {
                        if (origins[i].X1 < actualPageArea.X1)
                        {
                            origins[i] = new Rectangle(origins[i].X0, origins[i].Y0, origins[i].X1 + 0.5 / zoom, origins[i].Y1);
                        }
                        else
                        {
                            origins[i] = new Rectangle(origins[i].X0 - 0.5 / zoom, origins[i].Y0, origins[i].X1, origins[i].Y1);
                        }
                    }


                    if (roundedOrigin.Height > targets[i].Height)
                    {
                        if (origins[i].Y0 > actualPageArea.Y0)
                        {
                            origins[i] = new Rectangle(origins[i].X0, origins[i].Y0 + 0.5 / zoom, origins[i].X1, origins[i].Y1);
                        }
                        else
                        {
                            origins[i] = new Rectangle(origins[i].X0, origins[i].Y0, origins[i].X1, origins[i].Y1 - 0.5 / zoom);
                        }
                    }
                    else if (roundedOrigin.Height < targets[i].Height)
                    {
                        if (origins[i].X1 < actualPageArea.X1)
                        {
                            origins[i] = new Rectangle(origins[i].X0, origins[i].Y0, origins[i].X1, origins[i].Y1 + 0.5 / zoom);
                        }
                        else
                        {
                            origins[i] = new Rectangle(origins[i].X0, origins[i].Y0 - 0.5 / zoom, origins[i].X1, origins[i].Y1);
                        }
                    }

                    roundedOrigin = origins[i].Round(zoom);

                    if (roundedOrigin.Width == oldRoundedOrigin.Width && roundedOrigin.Height == oldRoundedOrigin.Height && (roundedOrigin.Width != targets[i].Width || roundedOrigin.Height != targets[i].Height))
                    {
                        countBlanks++;
                    }

                    if (countBlanks >= 100)
                    {
                        //It seems that we can't coerce the expected size and the actual size to be the same. Give up.
                        return;
                    }
                }
            }

            //Start each rendering thread.
            for (int i = 0; i < destinations.Length; i++)
            {
                double dzoom = zoom;
                Rectangle origin = origins[i];
                if (this.ImageXRes != 72 || this.ImageYRes != 72)
                {
                    dzoom *= Math.Sqrt(this.ImageXRes * this.ImageYRes) / 72;
                    origin = new Rectangle(origin.X0 * 72 / this.ImageXRes, origin.Y0 * 72 / this.ImageYRes, origin.X1 * 72 / this.ImageXRes, origin.Y1 * 72 / this.ImageYRes);
                }

                RenderingThreads[i].Render(Contexts[i].NativeContext, DisplayList, origin, (float)dzoom, destinations[i], pixelFormat, this.PageBounds, ClipToPageBounds);
            }

            //Wait until all the rendering threads have finished.
            for (int i = 0; i < destinations.Length; i++)
            {
                RenderingThreads[i].WaitForRendering();
            }
        }

        /// <summary>
        /// Gets an element from a collection of <see cref="System.Span{T}">Span</see>&lt;<see cref="byte"/>&gt;
        /// </summary>
        /// <param name="index">The index of the element to get.</param>
        /// <returns>An element from a collection of <see cref="System.Span{T}">Span</see>&lt;<see cref="byte"/>&gt;</returns>
        public delegate Span<byte> GetSpanItem(int index);

        /// <summary>
        /// Render the specified region to an image of the specified size, split in a number of tiles equal to the number of threads used by this <see cref="MuPDFMultiThreadedPageRenderer"/>, without marshaling. This method will not return until all the rendering threads have finished.
        /// Since creating an array of <see cref="Span{T}"/> is not allowed, this method returns a delegate that accepts an integer parameter (representing the index of the span in the "array") and returns the <see cref="Span{T}"/> corresponding to that index.
        /// </summary>
        /// <param name="targetSize">The total size of the image that should be rendered.</param>
        /// <param name="region">The region in page units that should be rendered.</param>
        /// <param name="disposables">A collection of <see cref="IDisposable"/>s that can be used to free the memory where the rendered tiles are stored. You should keep track of these and dispose them when you have finished working with the images.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <returns>A delegate that accepts an integer parameter (representing the index of the span in the "array") and returns the <see cref="Span{T}"/> corresponding to that index.</returns>
        public GetSpanItem Render(RoundedSize targetSize, Rectangle region, out IDisposable[] disposables, PixelFormats pixelFormat)
        {
            RoundedRectangle[] targets = targetSize.Split(this.ThreadCount);

            IntPtr[] destinations = new IntPtr[targets.Length];
            disposables = new IDisposable[targets.Length];

            bool hasAlpha = pixelFormat == PixelFormats.RGBA || pixelFormat == PixelFormats.BGRA;

            int[] sizes = new int[destinations.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                int allocSize = targets[i].Width * targets[i].Height * (hasAlpha ? 4 : 3);

                sizes[i] = allocSize;
                destinations[i] = Marshal.AllocHGlobal(allocSize);
                disposables[i] = new DisposableIntPtr(destinations[i], allocSize);
            }

            this.Render(targetSize, region, destinations, pixelFormat);

            return i =>
            {
                unsafe
                {
                    return new Span<byte>((void*)destinations[i], sizes[i]);
                }
            };
        }

        /// <summary>
        /// Signal to the rendering threads that they should abort rendering as soon as possible.
        /// </summary>
        public void Abort()
        {
            for (int i = 0; i < RenderingThreads.Length; i++)
            {
                RenderingThreads[i].AbortRendering();
            }
        }

        /// <summary>
        /// Get the current rendering progress of all the threads.
        /// </summary>
        /// <returns>A <see cref="RenderProgress"/> object containing the rendering progress of all the threads.</returns>
        public RenderProgress GetProgress()
        {
            return new RenderProgress((from el in RenderingThreads select el.GetProgress()).ToArray());
        }

        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Abort();

                    if (RenderingThreads != null)
                    {
                        for (int i = 0; i < RenderingThreads.Length; i++)
                        {
                            RenderingThreads[i].Dispose();
                        }
                    }

                    if (Contexts != null)
                    {
                        for (int i = 0; i < Contexts.Length; i++)
                        {
                            Contexts[i].Dispose();
                        }
                    }
                }
                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
