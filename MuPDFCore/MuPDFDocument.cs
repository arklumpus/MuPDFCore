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
using System.IO;
using System.Runtime.InteropServices;

namespace MuPDFCore
{
    /// <summary>
    /// A wrapper over a MuPDF document object, which contains possibly multiple pages.
    /// </summary>
    public class MuPDFDocument : IDisposable
    {
        /// <summary>
        /// If the document is an image, the horizontal resolution of the image. Otherwise, 72.
        /// </summary>
        internal double ImageXRes = double.NaN;

        /// <summary>
        /// If the document is an image, the vertical resolution of the image. Otherwise, 72.
        /// </summary>
        internal double ImageYRes = double.NaN;

        /// <summary>
        /// File extensions corresponding to the supported input formats.
        /// </summary>
        private static readonly string[] FileTypeMagics = new[]
        {
            ".pdf",
            ".xps",
            ".cbz",
            ".png",
            ".jpg",
            ".bmp",
            ".gif",
            ".tif",
            ".pnm",
            ".pam",
            ".epub",
            ".fb2"
        };

        /// <summary>
        /// An <see cref="IDisposable"/> with a value of null.
        /// </summary>
        private static IDisposable NullDataHolder = null;

        /// <summary>
        /// The context that owns this document.
        /// </summary>
        private readonly MuPDFContext OwnerContext;

        /// <summary>
        /// A pointer to the native document object.
        /// </summary>
        internal readonly IntPtr NativeDocument;

        /// <summary>
        /// A pointer to the native stream that was used to create this document (if any).
        /// </summary>
        private readonly IntPtr NativeStream = IntPtr.Zero;

        /// <summary>
        /// The number of pages in the document.
        /// </summary>
        private readonly int PageCount;

        /// <summary>
        /// An <see cref="IDisposable"/> that will be disposed together with this object.
        /// </summary>
        private readonly IDisposable DataHolder = null;

        /// <summary>
        /// A <see cref="GCHandle"/> that will be freed when this object is disposed.
        /// </summary>
        private GCHandle? DataHandle = null;

        /// <summary>
        /// An array of <see cref="MuPDFDisplayList"/>, one for each page in the document.
        /// </summary>
        private readonly MuPDFDisplayList[] DisplayLists;

        /// <summary>
        /// The pages contained in the document.
        /// </summary>
        public MuPDFPageCollection Pages { get; }

        /// <summary>
        /// Defines whether the images resulting from rendering operations should be clipped to the page boundaries.
        /// </summary>
        public bool ClipToPageBounds { get; set; } = true;

        /// <summary>
        /// Create a new <see cref="MuPDFDocument"/> from data bytes accessible through the specified pointer.
        /// </summary>
        /// <param name="context">The context that will own this document.</param>
        /// <param name="dataAddress">A pointer to the data bytes that make up the document.</param>
        /// <param name="dataLength">The number of bytes to read from the specified address.</param>
        /// <param name="fileType">The type of the document to read.</param>
        public MuPDFDocument(MuPDFContext context, IntPtr dataAddress, long dataLength, InputFileTypes fileType) : this(context, dataAddress, dataLength, fileType, ref NullDataHolder) { }

        /// <summary>
        /// Create a new <see cref="MuPDFDocument"/> from data bytes accessible through the specified pointer.
        /// </summary>
        /// <param name="context">The context that will own this document.</param>
        /// <param name="dataAddress">A pointer to the data bytes that make up the document.</param>
        /// <param name="dataLength">The number of bytes to read from the specified address.</param>
        /// <param name="fileType">The type of the document to read.</param>
        /// <param name="dataHolder">An <see cref="IDisposable"/> that will be disposed when the <see cref="MuPDFDocument"/> is disposed.</param>
        public MuPDFDocument(MuPDFContext context, IntPtr dataAddress, long dataLength, InputFileTypes fileType, ref IDisposable dataHolder)
        {
            bool isImage = fileType == InputFileTypes.BMP || fileType == InputFileTypes.GIF || fileType == InputFileTypes.JPEG || fileType == InputFileTypes.PAM || fileType == InputFileTypes.PNG || fileType == InputFileTypes.PNM || fileType == InputFileTypes.TIFF;

            this.OwnerContext = context;

            float xRes = 0;
            float yRes = 0;

            ExitCodes result = (ExitCodes)NativeMethods.CreateDocumentFromStream(context.NativeContext, dataAddress, (ulong)dataLength, FileTypeMagics[(int)fileType], isImage ? 1 : 0, ref NativeDocument, ref NativeStream, ref PageCount, ref xRes, ref yRes);

            if (xRes > 72)
            {
                this.ImageXRes = xRes;
            }
            else
            {
                this.ImageXRes = 72;
            }

            if (yRes > 72)
            {
                this.ImageYRes = yRes;
            }
            else
            {
                this.ImageYRes = 72;
            }

            this.DataHolder = dataHolder;

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_OPEN_STREAM:
                    throw new MuPDFException("Cannot open data stream", result);
                case ExitCodes.ERR_CANNOT_OPEN_FILE:
                    throw new MuPDFException("Cannot open document", result);
                case ExitCodes.ERR_CANNOT_COUNT_PAGES:
                    throw new MuPDFException("Cannot count pages", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            Pages = new MuPDFPageCollection(context, this, PageCount);
            DisplayLists = new MuPDFDisplayList[PageCount];
        }

        /// <summary>
        /// Create a new <see cref="MuPDFDocument"/> from an array of bytes.
        /// </summary>
        /// <param name="context">The context that will own this document.</param>
        /// <param name="data">An array containing the data bytes that make up the document. This must not be altered until after the <see cref="MuPDFDocument"/> has been disposed!
        /// The address of the array will be pinned, which may cause degradation in the Garbage Collector's performance, and is thus only advised for short-lived documents. To avoid this issue, marshal the bytes to unmanaged memory and use one of the <see cref="IntPtr"/> constructors.</param>
        /// <param name="fileType">The type of the document to read.</param>
        public MuPDFDocument(MuPDFContext context, byte[] data, InputFileTypes fileType)
        {
            bool isImage = fileType == InputFileTypes.BMP || fileType == InputFileTypes.GIF || fileType == InputFileTypes.JPEG || fileType == InputFileTypes.PAM || fileType == InputFileTypes.PNG || fileType == InputFileTypes.PNM || fileType == InputFileTypes.TIFF;

            this.OwnerContext = context;

            DataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            IntPtr dataAddress = DataHandle.Value.AddrOfPinnedObject();
            ulong dataLength = (ulong)data.Length;

            float xRes = 0;
            float yRes = 0;

            ExitCodes result = (ExitCodes)NativeMethods.CreateDocumentFromStream(context.NativeContext, dataAddress, dataLength, FileTypeMagics[(int)fileType], isImage ? 1 : 0, ref NativeDocument, ref NativeStream, ref PageCount, ref xRes, ref yRes);

            if (xRes > 72)
            {
                this.ImageXRes = xRes;
            }
            else
            {
                this.ImageXRes = 72;
            }

            if (yRes > 72)
            {
                this.ImageYRes = yRes;
            }
            else
            {
                this.ImageYRes = 72;
            }

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_OPEN_STREAM:
                    throw new MuPDFException("Cannot open data stream", result);
                case ExitCodes.ERR_CANNOT_OPEN_FILE:
                    throw new MuPDFException("Cannot open document", result);
                case ExitCodes.ERR_CANNOT_COUNT_PAGES:
                    throw new MuPDFException("Cannot count pages", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            Pages = new MuPDFPageCollection(context, this, PageCount);
            DisplayLists = new MuPDFDisplayList[PageCount];
        }

        /// <summary>
        /// Create a new <see cref="MuPDFDocument"/> from a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="context">The context that will own this document.</param>
        /// <param name="data">The <see cref="MemoryStream"/> containing the data that makes up the document. This will be disposed when the <see cref="MuPDFDocument"/> has been disposed and must not be disposed externally!
        /// The address of the <see cref="MemoryStream"/>'s buffer will be pinned, which may cause degradation in the Garbage Collector's performance, and is thus only advised for short-lived documents. To avoid this issue, marshal the bytes to unmanaged memory and use one of the <see cref="IntPtr"/> constructors.</param>
        /// <param name="fileType">The type of the document to read.</param>
        public MuPDFDocument(MuPDFContext context, ref MemoryStream data, InputFileTypes fileType)
        {
            bool isImage = fileType == InputFileTypes.BMP || fileType == InputFileTypes.GIF || fileType == InputFileTypes.JPEG || fileType == InputFileTypes.PAM || fileType == InputFileTypes.PNG || fileType == InputFileTypes.PNM || fileType == InputFileTypes.TIFF;

            this.OwnerContext = context;

            int origin = (int)data.Seek(0, SeekOrigin.Begin);
            ulong dataLength = (ulong)data.Length;
            byte[] dataBytes = data.GetBuffer();

            DataHandle = GCHandle.Alloc(dataBytes, GCHandleType.Pinned);
            IntPtr dataAddress = IntPtr.Add(DataHandle.Value.AddrOfPinnedObject(), origin);

            DataHolder = data;

            float xRes = 0;
            float yRes = 0;

            ExitCodes result = (ExitCodes)NativeMethods.CreateDocumentFromStream(context.NativeContext, dataAddress, dataLength, FileTypeMagics[(int)fileType], isImage ? 1 : 0, ref NativeDocument, ref NativeStream, ref PageCount, ref xRes, ref yRes);

            if (xRes > 72)
            {
                this.ImageXRes = xRes;
            }
            else
            {
                this.ImageXRes = 72;
            }

            if (yRes > 72)
            {
                this.ImageYRes = yRes;
            }
            else
            {
                this.ImageYRes = 72;
            }


            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_OPEN_STREAM:
                    throw new MuPDFException("Cannot open data stream", result);
                case ExitCodes.ERR_CANNOT_OPEN_FILE:
                    throw new MuPDFException("Cannot open document", result);
                case ExitCodes.ERR_CANNOT_COUNT_PAGES:
                    throw new MuPDFException("Cannot count pages", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            Pages = new MuPDFPageCollection(context, this, PageCount);
            DisplayLists = new MuPDFDisplayList[PageCount];
        }

        /// <summary>
        /// Create a new <see cref="MuPDFDocument"/> from a file.
        /// </summary>
        /// <param name="context">The context that will own this document.</param>
        /// <param name="fileName">The path to the file to open.</param>
        public MuPDFDocument(MuPDFContext context, string fileName)
        {
            bool isImage;

            string extension = Path.GetExtension(fileName).ToLowerInvariant();

            switch (extension)
            {
                case ".bmp":
                case ".dib":

                case ".gif":

                case ".jpg":
                case ".jpeg":
                case ".jpe":
                case ".jif":
                case ".jfif":
                case ".jfi":

                case ".pam":
                case ".pbm":
                case ".pgm":
                case ".ppm":
                case ".pnm":

                case ".png":

                case ".tif":
                case ".tiff":
                    isImage = true;
                    break;
                default:
                    isImage = false;
                    break;
            }


            this.OwnerContext = context;

            float xRes = 0;
            float yRes = 0;

            ExitCodes result = (ExitCodes)NativeMethods.CreateDocumentFromFile(context.NativeContext, fileName, isImage ? 1 : 0, ref NativeDocument, ref PageCount, ref xRes, ref yRes);


            if (xRes > 72)
            {
                this.ImageXRes = xRes;
            }
            else
            {
                this.ImageXRes = 72;
            }

            if (yRes > 72)
            {
                this.ImageYRes = yRes;
            }
            else
            {
                this.ImageYRes = 72;
            }

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_OPEN_FILE:
                    throw new MuPDFException("Cannot open document", result);
                case ExitCodes.ERR_CANNOT_COUNT_PAGES:
                    throw new MuPDFException("Cannot count pages", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            Pages = new MuPDFPageCollection(context, this, PageCount);
            DisplayLists = new MuPDFDisplayList[PageCount];
        }

        /// <summary>
        /// Discard all the display lists that have been loaded from the document, possibly freeing some memory in the case of a huge document.
        /// </summary>
        public void ClearCache()
        {
            for (int i = 0; i < PageCount; i++)
            {
                DisplayLists[i]?.Dispose();
                DisplayLists[i] = null;
            }
        }

        /// <summary>
        /// Render (part of) a page to an array of bytes.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="region">The region of the page to render in page units.</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        /// <returns>A byte array containing the raw values for the pixels of the rendered image.</returns>
        public byte[] Render(int pageNumber, Rectangle region, double zoom, PixelFormats pixelFormat, bool includeAnnotations = true)
        {
            int bufferSize = MuPDFDocument.GetRenderedSize(region, zoom, pixelFormat);

            byte[] buffer = new byte[bufferSize];

            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPointer = bufferHandle.AddrOfPinnedObject();

            try
            {
                Render(pageNumber, region, zoom, pixelFormat, bufferPointer, includeAnnotations);
            }
            finally
            {
                bufferHandle.Free();
            }

            return buffer;
        }

        /// <summary>
        /// Render a page to an array of bytes.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        /// <returns>A byte array containing the raw values for the pixels of the rendered image.</returns>
        public byte[] Render(int pageNumber, double zoom, PixelFormats pixelFormat, bool includeAnnotations = true)
        {
            Rectangle region = this.Pages[pageNumber].Bounds;
            return Render(pageNumber, region, zoom, pixelFormat, includeAnnotations);
        }

        /// <summary>
        /// Render (part of) a page to the specified destination.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="region">The region of the page to render in page units.</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="destination">The address of the buffer where the pixel data will be written. There must be enough space available to write the values for all the pixels, otherwise this will fail catastrophically!</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public void Render(int pageNumber, Rectangle region, double zoom, PixelFormats pixelFormat, IntPtr destination, bool includeAnnotations = true)
        {
            if (DisplayLists[pageNumber] == null)
            {
                DisplayLists[pageNumber] = new MuPDFDisplayList(this.OwnerContext, this.Pages[pageNumber], includeAnnotations);
            }

            if (zoom < 0.000001 | zoom * region.Width <= 0.001 || zoom * region.Height <= 0.001)
            {
                throw new ArgumentOutOfRangeException(nameof(zoom), zoom, "The zoom factor is too small!");
            }

            if (this.ImageXRes != 72 || this.ImageYRes != 72)
            {
                zoom *= Math.Sqrt(this.ImageXRes * this.ImageYRes) / 72;
                region = new Rectangle(region.X0 * 72 / this.ImageXRes, region.Y0 * 72 / this.ImageYRes, region.X1 * 72 / this.ImageXRes, region.Y1 * 72 / this.ImageYRes);
            }

            float fzoom = (float)zoom;

            ExitCodes result = (ExitCodes)NativeMethods.RenderSubDisplayList(OwnerContext.NativeContext, DisplayLists[pageNumber].NativeDisplayList, region.X0, region.Y0, region.X1, region.Y1, fzoom, (int)pixelFormat, destination, IntPtr.Zero);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            RoundedRectangle roundedRegion = region.Round(fzoom);
            RoundedSize roundedSize = new RoundedSize(roundedRegion.Width, roundedRegion.Height);

            if (pixelFormat == PixelFormats.RGBA || pixelFormat == PixelFormats.BGRA)
            {
                Utils.UnpremultiplyAlpha(destination, roundedSize);
            }

            if (this.ClipToPageBounds && !Pages[pageNumber].Bounds.Contains(DisplayLists[pageNumber].Bounds.Intersect(region)))
            {
                Utils.ClipImage(destination, roundedSize, region, Pages[pageNumber].Bounds, pixelFormat);
            }
        }

        /// <summary>
        /// Render a page to the specified destination.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="destination">The address of the buffer where the pixel data will be written. There must be enough space available to write the values for all the pixels, otherwise this will fail catastrophically!</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public void Render(int pageNumber, double zoom, PixelFormats pixelFormat, IntPtr destination, bool includeAnnotations = true)
        {
            Rectangle region = this.Pages[pageNumber].Bounds;
            Render(pageNumber, region, zoom, pixelFormat, destination, includeAnnotations);
        }

        /// <summary>
        /// Create a new <see cref="MuPDFMultiThreadedPageRenderer"/> that renders the specified page with the specified number of threads.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="threadCount">The number of threads to use. This must be factorisable using only powers of 2, 3, 5 or 7. Otherwise, the biggest number smaller than <paramref name="threadCount"/> that satisfies this condition is used.</param>
        /// <returns>A <see cref="MuPDFMultiThreadedPageRenderer"/> that can be used to render the specified page with the specified number of threads.</returns>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public MuPDFMultiThreadedPageRenderer GetMultiThreadedRenderer(int pageNumber, int threadCount, bool includeAnnotations = true)
        {
            if (DisplayLists[pageNumber] == null)
            {
                DisplayLists[pageNumber] = new MuPDFDisplayList(this.OwnerContext, this.Pages[pageNumber], includeAnnotations);
            }

            return new MuPDFMultiThreadedPageRenderer(OwnerContext, DisplayLists[pageNumber], threadCount, Pages[pageNumber].Bounds, this.ClipToPageBounds, this.ImageXRes, this.ImageYRes);
        }

        /// <summary>
        /// Determine how many bytes will be necessary to render the specified page at the specified zoom level, using the the specified pixel format.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixels data.</param>
        /// <returns>An integer representing the number of bytes that will be necessary to store the pixel data of the rendered image.</returns>
        public int GetRenderedSize(int pageNumber, double zoom, PixelFormats pixelFormat)
        {
            return GetRenderedSize(Pages[pageNumber].Bounds, zoom, pixelFormat);
        }

        /// <summary>
        /// Determine how many bytes will be necessary to render the specified region in page units at the specified zoom level, using the the specified pixel format.
        /// </summary>
        /// <param name="region">The region that will be rendered.</param>
        /// <param name="zoom">The scale at which the region will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixels data.</param>
        /// <returns>An integer representing the number of bytes that will be necessary to store the pixel data of the rendered image.</returns>
        public static int GetRenderedSize(Rectangle region, double zoom, PixelFormats pixelFormat)
        {
            float x0 = region.X0 * (float)zoom;
            float y0 = region.Y0 * (float)zoom;
            float x1 = region.X1 * (float)zoom;
            float y1 = region.Y1 * (float)zoom;

            Rectangle scaledRect = new Rectangle(x0, y0, x1, y1);
            RoundedRectangle bounds = scaledRect.Round();

            int width = bounds.Width;
            int height = bounds.Height;

            switch (pixelFormat)
            {
                case PixelFormats.RGB:
                case PixelFormats.BGR:
                    return width * height * 3;
                case PixelFormats.RGBA:
                case PixelFormats.BGRA:
                    return width * height * 4;
            }

            return -1;
        }



        /// <summary>
        /// Save (part of) a page to an image file in the specified format.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="region">The region of the page to render in page units.</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="fileName">The path to the output file.</param>
        /// <param name="fileType">The output format of the file.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public void SaveImage(int pageNumber, Rectangle region, double zoom, PixelFormats pixelFormat, string fileName, RasterOutputFileTypes fileType, bool includeAnnotations = true)
        {
            if (pixelFormat == PixelFormats.RGBA && fileType == RasterOutputFileTypes.PNM)
            {
                throw new ArgumentException("Cannot save an image with alpha channel in PNM format!", nameof(fileType));
            }

            if (DisplayLists[pageNumber] == null)
            {
                DisplayLists[pageNumber] = new MuPDFDisplayList(this.OwnerContext, this.Pages[pageNumber], includeAnnotations);
            }

            if (zoom < 0.000001 | zoom * region.Width <= 0.001 || zoom * region.Height <= 0.001)
            {
                throw new ArgumentOutOfRangeException(nameof(zoom), zoom, "The zoom factor is too small!");
            }

            if (this.ImageXRes != 72 || this.ImageYRes != 72)
            {
                zoom *= Math.Sqrt(this.ImageXRes * this.ImageYRes) / 72;
                region = new Rectangle(region.X0 * 72 / this.ImageXRes, region.Y0 * 72 / this.ImageYRes, region.X1 * 72 / this.ImageXRes, region.Y1 * 72 / this.ImageYRes);
            }

            float fzoom = (float)zoom;

            ExitCodes result = (ExitCodes)NativeMethods.SaveImage(OwnerContext.NativeContext, DisplayLists[pageNumber].NativeDisplayList, region.X0, region.Y0, region.X1, region.Y1, fzoom, (int)pixelFormat, fileName, (int)fileType);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                case ExitCodes.ERR_CANNOT_SAVE:
                    throw new MuPDFException("Cannot save to the output file", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }
        }

        /// <summary>
        /// Save a page to an image file in the specified format.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="fileName">The path to the output file.</param>
        /// <param name="fileType">The output format of the file.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public void SaveImage(int pageNumber, double zoom, PixelFormats pixelFormat, string fileName, RasterOutputFileTypes fileType, bool includeAnnotations = true)
        {
            Rectangle region = this.Pages[pageNumber].Bounds;
            SaveImage(pageNumber, region, zoom, pixelFormat, fileName, fileType, includeAnnotations);
        }

        /// <summary>
        /// Write (part of) a page to an image stream in the specified format.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="region">The region of the page to render in page units.</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="outputStream">The stream to which the image data will be written.</param>
        /// <param name="fileType">The output format of the image.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public void WriteImage(int pageNumber, Rectangle region, double zoom, PixelFormats pixelFormat, Stream outputStream, RasterOutputFileTypes fileType, bool includeAnnotations = true)
        {
            if (pixelFormat == PixelFormats.RGBA && fileType == RasterOutputFileTypes.PNM)
            {
                throw new ArgumentException("Cannot save an image with alpha channel in PNM format!", nameof(fileType));
            }

            if (DisplayLists[pageNumber] == null)
            {
                DisplayLists[pageNumber] = new MuPDFDisplayList(this.OwnerContext, this.Pages[pageNumber], includeAnnotations);
            }

            if (zoom < 0.000001 | zoom * region.Width <= 0.001 || zoom * region.Height <= 0.001)
            {
                throw new ArgumentOutOfRangeException(nameof(zoom), zoom, "The zoom factor is too small!");
            }

            if (this.ImageXRes != 72 || this.ImageYRes != 72)
            {
                zoom *= Math.Sqrt(this.ImageXRes * this.ImageYRes) / 72;
                region = new Rectangle(region.X0 * 72 / this.ImageXRes, region.Y0 * 72 / this.ImageYRes, region.X1 * 72 / this.ImageXRes, region.Y1 * 72 / this.ImageYRes);
            }

            float fzoom = (float)zoom;

            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            ExitCodes result = (ExitCodes)NativeMethods.WriteImage(OwnerContext.NativeContext, DisplayLists[pageNumber].NativeDisplayList, region.X0, region.Y0, region.X1, region.Y1, fzoom, (int)pixelFormat, (int)fileType, ref outputBuffer, ref outputData, ref outputDataLength);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                case ExitCodes.ERR_CANNOT_CREATE_CONTEXT:
                    throw new MuPDFException("Cannot create the output buffer", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            byte[] buffer = new byte[1024];

            while (outputDataLength > 0)
            {
                int bytesToCopy = (int)Math.Min(buffer.Length, (long)outputDataLength);
                Marshal.Copy(outputData, buffer, 0, bytesToCopy);
                outputData = IntPtr.Add(outputData, bytesToCopy);
                outputStream.Write(buffer, 0, bytesToCopy);
                outputDataLength -= (ulong)bytesToCopy;
            }

            NativeMethods.DisposeBuffer(OwnerContext.NativeContext, outputBuffer);
        }

        /// <summary>
        /// Write a page to an image stream in the specified format.
        /// </summary>
        /// <param name="pageNumber">The number of the page to render (starting at 0).</param>
        /// <param name="zoom">The scale at which the page will be rendered. This will determine the size in pixel of the image.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        /// <param name="outputStream">The stream to which the image data will be written.</param>
        /// <param name="fileType">The output format of the image.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public void WriteImage(int pageNumber, double zoom, PixelFormats pixelFormat, Stream outputStream, RasterOutputFileTypes fileType, bool includeAnnotations = true)
        {
            Rectangle region = this.Pages[pageNumber].Bounds;
            WriteImage(pageNumber, region, zoom, pixelFormat, outputStream, fileType, includeAnnotations);
        }

        /// <summary>
        /// Create a new document containing the specified (parts of) pages from other documents.
        /// </summary>
        /// <param name="context">The context that was used to open the documents.</param>
        /// <param name="fileName">The output file name.</param>
        /// <param name="fileType">The output file format.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
        public static void CreateDocument(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, bool includeAnnotations = true, params (MuPDFPage page, Rectangle region, float zoom)[] pages)
        {
            if (fileType == DocumentOutputFileTypes.SVG && pages.Length > 1)
            {
                //Actually, you can, but the library creates multiple files appending numbers after each name (e.g. page1.svg, page2.svg, ...), which is ugly and may have unintended consequences.
                //If you really want to do this, you can call this method multiple times.
                throw new ArgumentException("You cannot create an SVG document with more than one page!", nameof(pages));
            }

            string originalFileName = fileName;

            if (fileType == DocumentOutputFileTypes.SVG)
            {
                //For SVG documents, the library annoyingly alters the output file name, appending a "1" just before the extension (e.g. document.svg -> document1.svg). Since users may not be expecting this, it is best to render to a temporary file and then move it to the specified location.
                fileName = Path.GetTempFileName();
            }

            IntPtr documentWriter = IntPtr.Zero;

            //Initialise document writer.
            ExitCodes result = (ExitCodes)NativeMethods.CreateDocumentWriter(context.NativeContext, fileName, (int)fileType, ref documentWriter);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_CREATE_WRITER:
                    throw new MuPDFException("Cannot create the document writer", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            //Write pages.
            for (int i = 0; i < pages.Length; i++)
            {
                MuPDFDocument doc = pages[i].page.OwnerDocument;
                int pageNum = pages[i].page.PageNumber;

                if (doc.DisplayLists[pageNum] == null)
                {
                    doc.DisplayLists[pageNum] = new MuPDFDisplayList(doc.OwnerContext, doc.Pages[pageNum], includeAnnotations);
                }

                Rectangle region = pages[i].region;
                double zoom = pages[i].zoom;

                if (pages[i].page.OwnerDocument.ImageXRes != 72 || pages[i].page.OwnerDocument.ImageYRes != 72)
                {
                    zoom *= Math.Sqrt(pages[i].page.OwnerDocument.ImageXRes * pages[i].page.OwnerDocument.ImageYRes) / 72;
                    region = new Rectangle(region.X0 * 72 / pages[i].page.OwnerDocument.ImageXRes, region.Y0 * 72 / pages[i].page.OwnerDocument.ImageYRes, region.X1 * 72 / pages[i].page.OwnerDocument.ImageXRes, region.Y1 * 72 / pages[i].page.OwnerDocument.ImageYRes);
                }

                result = (ExitCodes)NativeMethods.WriteSubDisplayListAsPage(context.NativeContext, doc.DisplayLists[pageNum].NativeDisplayList, region.X0, region.Y0, region.X1, region.Y1, (float)zoom, documentWriter);

                switch (result)
                {
                    case ExitCodes.EXIT_SUCCESS:
                        break;
                    case ExitCodes.ERR_CANNOT_RENDER:
                        throw new MuPDFException("Cannot render page " + i.ToString(), result);
                    default:
                        throw new MuPDFException("Unknown error", result);
                }
            }

            //Close and dispose the document writer.
            result = (ExitCodes)NativeMethods.FinalizeDocumentWriter(context.NativeContext, documentWriter);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_CLOSE_DOCUMENT:
                    throw new MuPDFException("Cannot finalise the document", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            if (fileType == DocumentOutputFileTypes.SVG)
            {
                //Move the temporary file to the location specified by the user.
                //The library has altered the temporary file name by appending a "1" before the extension.
                string tempFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "1" + Path.GetExtension(fileName));

                //Overwrite existing file.
                if (File.Exists(originalFileName))
                {
                    File.Delete(originalFileName);
                }

                File.Move(tempFileName, originalFileName);
            }
        }

        /// <summary>
        /// Create a new document containing the specified pages from other documents.
        /// </summary>
        /// <param name="context">The context that was used to open the documents.</param>
        /// <param name="fileName">The output file name.</param>
        /// <param name="fileType">The output file format.</param>
        /// <param name="pages">The pages to include in the document.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public static void CreateDocument(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, bool includeAnnotations = true, params MuPDFPage[] pages)
        {
            (MuPDFPage, Rectangle, float)[] boundedPages = new (MuPDFPage, Rectangle, float)[pages.Length];

            for (int i = 0; i < pages.Length; i++)
            {
                boundedPages[i] = (pages[i], pages[i].Bounds, 1);
            }

            CreateDocument(context, fileName, fileType, includeAnnotations, boundedPages);
        }


        /// <summary>
        /// Creates a new <see cref="MuPDFStructuredTextPage"/> from the specified page. This contains information about the text layout that can be used for highlighting and searching. The reading order is taken from the order the text is drawn in the source file, so may not be accurate.
        /// </summary>
        /// <param name="pageNumber">The number of the page (starting at 0)</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included. Otherwise, only the page contents are included.</param>
        /// <returns>A <see cref="MuPDFStructuredTextPage"/> containing a structured text representation of the page.</returns>
        public MuPDFStructuredTextPage GetStructuredTextPage(int pageNumber, bool includeAnnotations = true)
        {
            if (DisplayLists[pageNumber] == null)
            {
                DisplayLists[pageNumber] = new MuPDFDisplayList(this.OwnerContext, this.Pages[pageNumber], includeAnnotations);
            }

            return new MuPDFStructuredTextPage(this.OwnerContext, this.DisplayLists[pageNumber]);
        }



        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Pages.Dispose();
                    foreach (MuPDFDisplayList list in DisplayLists)
                    {
                        list?.Dispose();
                    }
                    DataHandle?.Free();
                    DataHolder?.Dispose();
                }

                NativeMethods.DisposeDocument(OwnerContext.NativeContext, NativeDocument);

                if (NativeStream != IntPtr.Zero)
                {
                    NativeMethods.DisposeStream(OwnerContext.NativeContext, NativeStream);
                }

                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~MuPDFDocument()
        {
            Dispose(disposing: false);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
