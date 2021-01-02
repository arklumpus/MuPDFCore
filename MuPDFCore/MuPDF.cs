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
using System.Runtime.InteropServices;

namespace MuPDFCore
{
    /// <summary>
    /// Exit codes returned by native methods describing various errors that can occur.
    /// </summary>
    public  enum ExitCodes
    {
        /// <summary>
        /// An error occurred while creating the context object.
        /// </summary>
        ERR_CANNOT_CREATE_CONTEXT = 129,

        /// <summary>
        /// An error occurred while registering the default document handlers with the context.
        /// </summary>
        ERR_CANNOT_REGISTER_HANDLERS = 130,

        /// <summary>
        /// An error occurred while opening a file.
        /// </summary>
        ERR_CANNOT_OPEN_FILE = 131,

        /// <summary>
        /// An error occurred while determining the total number of pages in the document.
        /// </summary>
        ERR_CANNOT_COUNT_PAGES = 132,

        /// <summary>
        /// An error occurred while rendering the page.
        /// </summary>
        ERR_CANNOT_RENDER = 134,

        /// <summary>
        /// An error occurred while opening the stream.
        /// </summary>
        ERR_CANNOT_OPEN_STREAM = 135,

        /// <summary>
        /// An error occurred while loading the page.
        /// </summary>
        ERR_CANNOT_LOAD_PAGE = 136,

        /// <summary>
        /// An error occurred while computing the page bounds.
        /// </summary>
        ERR_CANNOT_COMPUTE_BOUNDS = 137,

        /// <summary>
        /// An error occurred while initialising the mutexes for the lock mechanism.
        /// </summary>
        ERR_CANNOT_INIT_MUTEX = 138,

        /// <summary>
        /// An error occurred while cloning the context.
        /// </summary>
        ERR_CANNOT_CLONE_CONTEXT = 139,

        /// <summary>
        /// An error occurred while saving the page to a raster image file.
        /// </summary>
        ERR_CANNOT_SAVE = 140,

        /// <summary>
        /// An error occurred while creating the output buffer.
        /// </summary>
        ERR_CANNOT_CREATE_BUFFER = 141,

        /// <summary>
        /// An error occurred while creating the document writer.
        /// </summary>
        ERR_CANNOT_CREATE_WRITER = 142,

        /// <summary>
        /// An error occurred while finalising the document file.
        /// </summary>
        ERR_CANNOT_CLOSE_DOCUMENT = 143,

        /// <summary>
        /// An error occurred while creating an empty structured text page.
        /// </summary>
        ERR_CANNOT_CREATE_PAGE = 144,

        /// <summary>
        /// An error occurred while populating the structured text page
        /// </summary>
        ERR_CANNOT_POPULATE_PAGE = 145,

        /// <summary>
        /// No error occurred. All is well.
        /// </summary>
        EXIT_SUCCESS = 0
    }

    /// <summary>
    /// File types supported in input by the library.
    /// </summary>
    public enum InputFileTypes
    {
        /// <summary>
        /// Portable Document Format.
        /// </summary>
        PDF = 0,

        /// <summary>
        /// XML Paper Specification document.
        /// </summary>
        XPS = 1,

        /// <summary>
        /// Comic book archive file (ZIP archive containing page scans).
        /// </summary>
        CBZ = 2,

        /// <summary>
        /// Portable Network Graphics format.
        /// </summary>
        PNG = 3,

        /// <summary>
        /// Joint Photographic Experts Group image.
        /// </summary>
        JPEG = 4,

        /// <summary>
        /// Bitmap image.
        /// </summary>
        BMP = 5,

        /// <summary>
        /// Graphics Interchange Format.
        /// </summary>
        GIF = 6,

        /// <summary>
        /// Tagged Image File Format.
        /// </summary>
        TIFF = 7,

        /// <summary>
        /// Portable aNyMap graphics format.
        /// </summary>
        PNM = 8,

        /// <summary>
        /// Portable Arbitrary Map graphics format.
        /// </summary>
        PAM = 9,

        /// <summary>
        /// Electronic PUBlication document.
        /// </summary>
        EPUB = 10,

        /// <summary>
        /// FictionBook document.
        /// </summary>
        FB2 = 11
    }

    /// <summary>
    /// Raster image file types supported in output by the library.
    /// </summary>
    public enum RasterOutputFileTypes
    {
        /// <summary>
        /// Portable aNyMap graphics format.
        /// </summary>
        PNM = 0,

        /// <summary>
        /// Portable Arbitrary Map graphics format.
        /// </summary>
        PAM = 1,

        /// <summary>
        /// Portable Network Graphics format.
        /// </summary>
        PNG = 2,

        /// <summary>
        /// PhotoShop Document format.
        /// </summary>
        PSD = 3
    };

    /// <summary>
    /// Document file types supported in output by the library.
    /// </summary>
    public enum DocumentOutputFileTypes
    {
        /// <summary>
        /// Portable Document Format.
        /// </summary>
        PDF = 0,

        /// <summary>
        /// Scalable Vector Graphics.
        /// </summary>
        SVG = 1,

        /// <summary>
        /// Comic book archive format.
        /// </summary>
        CBZ = 2
    };

    /// <summary>
    /// Pixel formats supported by the library.
    /// </summary>
    public enum PixelFormats
    {
        /// <summary>
        /// 24bpp RGB format.
        /// </summary>
        RGB = 0,

        /// <summary>
        /// 32bpp RGBA format.
        /// </summary>
        RGBA = 1,

        /// <summary>
        /// 24bpp BGR format.
        /// </summary>
        BGR = 2,

        /// <summary>
        /// 32bpp BGRA format.
        /// </summary>
        BGRA = 3
    }

    /// <summary>
    /// A struct to hold information about the current rendering process and to abort rendering as needed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Cookie
    {
        public int abort;
        public int progress;
        public ulong progress_max;
        public int errors;
        public int incomplete;
    }

    /// <summary>
    /// Holds a summery of the progress of the current rendering operation.
    /// </summary>
    public class RenderProgress
    {
        /// <summary>
        /// Holds the progress of a single thread.
        /// </summary>
        public struct ThreadRenderProgress
        {
            /// <summary>
            /// The current progress.
            /// </summary>
            public int Progress;

            /// <summary>
            /// The maximum progress. If this is 0, this value could not be determined (yet).
            /// </summary>
            public long MaxProgress;

            internal ThreadRenderProgress(int progress, ulong maxProgress)
            {
                this.Progress = progress;
                this.MaxProgress = (long)maxProgress;
            }
        }

        /// <summary>
        /// Contains the progress of all the threads used in rendering the document.
        /// </summary>
        public ThreadRenderProgress[] ThreadRenderProgresses { get; private set; }

        internal RenderProgress(ThreadRenderProgress[] threadRenderProgresses)
        {
            ThreadRenderProgresses = threadRenderProgresses;
        }
    }

    /// <summary>
    /// An <see cref="IDisposable"/> wrapper around an <see cref="IntPtr"/> that frees the allocated memory when it is disposed.
    /// </summary>
    public class DisposableIntPtr : IDisposable
    {
        /// <summary>
        /// The pointer to the unmanaged memory.
        /// </summary>
        private readonly IntPtr InternalPointer;

        /// <summary>
        /// Create a new DisposableIntPtr.
        /// </summary>
        /// <param name="pointer">The pointer that should be freed upon disposing of this object.</param>
        public DisposableIntPtr(IntPtr pointer)
        {
            this.InternalPointer = pointer;
        }

        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Marshal.FreeHGlobal(InternalPointer);
                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~DisposableIntPtr()
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

    /// <summary>
    /// The exception that is thrown when a MuPDF operation fails.
    /// </summary>
    public class MuPDFException : Exception
    {
        /// <summary>
        /// The <see cref="ExitCodes"/> returned by the native function.
        /// </summary>
        public readonly ExitCodes ErrorCode;

        internal MuPDFException(string message, ExitCodes errorCode) : base(message)
        {
            this.ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Native methods.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Create a MuPDF context object with the specified store size.
        /// </summary>
        /// <param name="store_size">Maximum size in bytes of the resource store.</param>
        /// <param name="out_ctx">A pointer to the native context object.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateContext(ulong store_size, ref IntPtr out_ctx);

        /// <summary>
        /// Free a context and its global store.
        /// </summary>
        /// <param name="ctx">A pointer to the native context to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeContext(IntPtr ctx);

        /// <summary>
        /// Evict items from the store until the total size of the objects in the store is reduced to a given percentage of its current size.
        /// </summary>
        /// <param name="ctx">The context whose store should be shrunk.</param>
        /// <param name="perc">Fraction of current size to reduce the store to.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ShrinkStore(IntPtr ctx, uint perc);

        /// <summary>
        /// Evict every item from the store.
        /// </summary>
        /// <param name="ctx">The context whose store should be emptied.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int EmptyStore(IntPtr ctx);

        /// <summary>
        /// Get the current size of the store.
        /// </summary>
        /// <param name="ctx">The context whose store's size should be determined.</param>
        /// <returns>The current size in bytes of the store.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern long GetCurrentStoreSize(IntPtr ctx);

        /// <summary>
        /// Get the maximum size of the store.
        /// </summary>
        /// <param name="ctx">The context whose store's maximum size should be determined.</param>
        /// <returns>The maximum size in bytes of the store.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern long GetMaxStoreSize(IntPtr ctx);

        /// <summary>
        /// Create a display list from a page.
        /// </summary>
        /// <param name="ctx">A pointer to the context used to create the document.</param>
        /// <param name="page">A pointer to the page that should be used to create the display list.</param>
        /// <param name="annotations">An integer indicating whether annotations should be included in the display list (1) or not (any other value).</param>
        /// <param name="out_display_list">A pointer to the newly-created display list.</param>
        /// <param name="out_x0">The left coordinate of the display list's bounds.</param>
        /// <param name="out_y0">The top coordinate of the display list's bounds.</param>
        /// <param name="out_x1">The right coordinate of the display list's bounds.</param>
        /// <param name="out_y1">The bottom coordinate of the display list's bounds.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetDisplayList(IntPtr ctx, IntPtr page, int annotations, ref IntPtr out_display_list, ref float out_x0, ref float out_y0, ref float out_x1, ref float out_y1);

        /// <summary>
        /// Free a display list.
        /// </summary>
        /// <param name="ctx">The context that was used to create the display list.</param>
        /// <param name="list">The display list to dispose.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeDisplayList(IntPtr ctx, IntPtr list);

        /// <summary>
        /// Create a new document from a stream.
        /// </summary>
        /// <param name="ctx">The context to which the document will belong.</param>
        /// <param name="data">A pointer to a byte array containing the data that makes up the document.</param>
        /// <param name="data_length">The length in bytes of the data that makes up the document.</param>
        /// <param name="file_type">The type (extension) of the document.</param>
        /// <param name="out_doc">The newly created document.</param>
        /// <param name="out_str">The newly created stream (so that it can be disposed later).</param>
        /// <param name="out_page_count">The number of pages in the document.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateDocumentFromStream(IntPtr ctx, IntPtr data, ulong data_length, string file_type, ref IntPtr out_doc, ref IntPtr out_str, ref int out_page_count);

        /// <summary>
        /// Create a new document from a file name.
        /// </summary>
        /// <param name="ctx">The context to which the document will belong.</param>
        /// <param name="file_name">The path of the file to open.</param>
        /// <param name="out_doc">The newly created document.</param>
        /// <param name="out_page_count">The number of pages in the document.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateDocumentFromFile(IntPtr ctx, string file_name, ref IntPtr out_doc, ref int out_page_count);

        /// <summary>
        /// Free a stream and its associated resources.
        /// </summary>
        /// <param name="ctx">The context that was used while creating the stream.</param>
        /// <param name="str">The stream to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeStream(IntPtr ctx, IntPtr str);

        /// <summary>
        /// Free a document and its associated resources.
        /// </summary>
        /// <param name="ctx">The context that was used in creating the document.</param>
        /// <param name="doc">The document to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeDocument(IntPtr ctx, IntPtr doc);

        /// <summary>
        /// Render (part of) a display list to an array of bytes starting at the specified pointer.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
        /// <param name="colorFormat">The pixel data format.</param>
        /// <param name="pixel_storage">A pointer indicating where the pixel bytes will be written. There must be enough space available!</param>
        /// <param name="cookie">A pointer to a cookie object that can be used to track progress and/or abort rendering. Can be null.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int RenderSubDisplayList(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, IntPtr pixel_storage, IntPtr cookie);

        /// <summary>
        /// Load a page from a document.
        /// </summary>
        /// <param name="ctx">The context to which the document belongs.</param>
        /// <param name="doc">The document from which the page should be extracted.</param>
        /// <param name="page_number">The page number.</param>
        /// <param name="out_page">The newly extracted page.</param>
        /// <param name="out_x">The left coordinate of the page's bounds.</param>
        /// <param name="out_y">The top coordinate of the page's bounds.</param>
        /// <param name="out_w">The width of the page.</param>
        /// <param name="out_h">The height of the page.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int LoadPage(IntPtr ctx, IntPtr doc, int page_number, ref IntPtr out_page, ref float out_x, ref float out_y, ref float out_w, ref float out_h);

        /// <summary>
        /// Free a page and its associated resources.
        /// </summary>
        /// <param name="ctx">The context to which the document containing the page belongs.</param>
        /// <param name="page">The page to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposePage(IntPtr ctx, IntPtr page);

        /// <summary>
        /// Create cloned contexts that can be used in multithreaded rendering.
        /// </summary>
        /// <param name="ctx">The original context to clone.</param>
        /// <param name="count">The number of cloned contexts to create.</param>
        /// <param name="out_contexts">An array of pointers to the cloned contexts.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CloneContext(IntPtr ctx, int count, IntPtr out_contexts);

        /// <summary>
        /// Save (part of) a display list to an image file in the specified format.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
        /// <param name="colorFormat">The pixel data format.</param>
        /// <param name="file_name">The path to the output file.</param>
        /// <param name="output_format">An integer equivalent to <see cref="RasterOutputFileTypes"/> specifying the output format.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SaveImage(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, string file_name, int output_format);

        /// <summary>
        /// Write (part of) a display list to an image buffer in the specified format.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
        /// <param name="colorFormat">The pixel data format.</param>
        /// <param name="output_format">An integer equivalent to <see cref="RasterOutputFileTypes"/> specifying the output format.</param>
        /// <param name="out_buffer">The address of the buffer on which the data has been written (only useful for disposing the buffer later).</param>
        /// <param name="out_data">The address of the byte array where the data has been actually written.</param>
        /// <param name="out_length">The length in bytes of the image data.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int WriteImage(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, int output_format, ref IntPtr out_buffer, ref IntPtr out_data, ref ulong out_length);

        /// <summary>
        /// Free a native buffer and its associated resources.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="buf">The buffer to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeBuffer(IntPtr ctx, IntPtr buf);

        /// <summary>
        /// Create a new document writer object.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="file_name">The name of file that will hold the writer's output.</param>
        /// <param name="format">An integer equivalent to <see cref="DocumentOutputFileTypes"/> specifying the output format.</param>
        /// <param name="out_document_writer">A pointer to the new document writer object.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateDocumentWriter(IntPtr ctx, string file_name, int format, ref IntPtr out_document_writer);

        /// <summary>
        /// Render (part of) a display list as a page in the specified document writer.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This will determine the final size of the page.</param>
        /// <param name="writ">The document writer on which the page should be written.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int WriteSubDisplayListAsPage(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, IntPtr writ);

        /// <summary>
        /// Finalise a document writer, closing the file and freeing all resources.
        /// </summary>
        /// <param name="ctx">The context that was used to create the document writer.</param>
        /// <param name="writ">The document writer to finalise.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int FinalizeDocumentWriter(IntPtr ctx, IntPtr writ);

        /// <summary>
        /// Get the contents of a structured text character.
        /// </summary>
        /// <param name="character">The address of the character.</param>
        /// <param name="out_c">Unicode code point of the character.</param>
        /// <param name="out_color">An sRGB hex representation of the colour of the character.</param>
        /// <param name="out_origin_x">The x coordinate of the baseline origin of the character.</param>
        /// <param name="out_origin_y">The y coordinate of the baseline origin of the character.</param>
        /// <param name="out_size">The size in points of the character.</param>
        /// <param name="out_ll_x">The x coordinate of the lower left corner of the bounding quad.</param>
        /// <param name="out_ll_y">The y coordinate of the lower left corner of the bounding quad.</param>
        /// <param name="out_ul_x">The x coordinate of the upper left corner of the bounding quad.</param>
        /// <param name="out_ul_y">The y coordinate of the upper left corner of the bounding quad.</param>
        /// <param name="out_ur_x">The x coordinate of the upper right corner of the bounding quad.</param>
        /// <param name="out_ur_y">The y coordinate of the upper right corner of the bounding quad.</param>
        /// <param name="out_lr_x">The x coordinate of the lower right corner of the bounding quad.</param>
        /// <param name="out_lr_y">The y coordinate of the lower right corner of the bounding quad.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextChar(IntPtr character, ref int out_c, ref int out_color, ref float out_origin_x, ref float out_origin_y, ref float out_size, ref float out_ll_x, ref float out_ll_y, ref float out_ul_x, ref float out_ul_y, ref float out_ur_x, ref float out_ur_y, ref float out_lr_x, ref float out_lr_y);

        /// <summary>
        /// Get an array of structured text characters from a structured text line.
        /// </summary>
        /// <param name="line">The structured text line from which the characters should be extracted.</param>
        /// <param name="out_chars">An array of pointers to the structured text characters.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextChars(IntPtr line, IntPtr out_chars);

        /// <summary>
        /// Get the contents of a structured text line.
        /// </summary>
        /// <param name="line">The address of the line.</param>
        /// <param name="out_wmode">An integer equivalent to <see cref="MuPDFStructuredTextLine"/> representing the writing mode of the line.</param>
        /// <param name="out_x0">The left coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_y0">The top coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_x1">The right coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_y1">The bottom coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_x">The x component of the normalised direction of the baseline.</param>
        /// <param name="out_y">The y component of the normalised direction of the baseline.</param>
        /// <param name="out_char_count">The number of characters in the line.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextLine(IntPtr line, ref int out_wmode, ref float out_x0, ref float out_y0, ref float out_x1, ref float out_y1, ref float out_x, ref float out_y, ref int out_char_count);

        /// <summary>
        /// Get an array of structured text lines from a structured text block.
        /// </summary>
        /// <param name="block">The structured text block from which the lines should be extracted.</param>
        /// <param name="out_lines">An array of pointers to the structured text lines.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextLines(IntPtr block, IntPtr out_lines);

        /// <summary>
        /// Get the contents of a structured text block.
        /// </summary>
        /// <param name="block">The address of the block.</param>
        /// <param name="out_type">An integer equivalent to <see cref="MuPDFStructuredTextBlock.Types"/> representing the type of the block.</param>
        /// <param name="out_x0">The left coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_y0">The top coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_x1">The right coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_y1">The bottom coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_line_count">The number of lines in the block.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextBlock(IntPtr block, ref int out_type, ref float out_x0, ref float out_y0, ref float out_x1, ref float out_y1, ref int out_line_count);

        /// <summary>
        /// Get an array of structured text blocks from a structured text page.
        /// </summary>
        /// <param name="page">The structured text page from which the blocks should be extracted.</param>
        /// <param name="out_blocks">An array of pointers to the structured text blocks.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextBlocks(IntPtr page, IntPtr out_blocks);

        /// <summary>
        /// Get a structured text representation of a display list.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list whose structured text representation is sought.</param>
        /// <param name="out_page">The address of the structured text page.</param>
        /// <param name="out_stext_block_count">The number of structured text blocks in the page.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextPage(IntPtr ctx, IntPtr list, ref IntPtr out_page, ref int out_stext_block_count);

        /// <summary>
        /// Free a native structured text page and its associated resources.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="page"></param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeStructuredTextPage(IntPtr ctx, IntPtr page);
    }
}
