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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MuPDFCore
{
    /// <summary>
    /// A wrapper over a MuPDF page object, which contains information about the page's boundaries.
    /// </summary>
    public class MuPDFPage : IDisposable
    {
        /// <summary>
        /// The page's bounds at 72 DPI. Read-only.
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        /// The number of this page in the original document.
        /// </summary>
        public int PageNumber { get; }

        private MuPDFLinks CachedLinks = null;

        /// <summary>
        /// The links contained within the <see cref="MuPDFPage"/>. This collection is populated on first access.
        /// </summary>
        public MuPDFLinks Links
        {
            get
            {
                if (CachedLinks == null)
                {
                    CachedLinks = new MuPDFLinks(this);
                }

                return CachedLinks;
            }
        }

        /// <summary>
        /// A pointer to the native page object.
        /// </summary>
        internal readonly IntPtr NativePage;

        /// <summary>
        /// The context that owns the document from which this page was extracted.
        /// </summary>
        private readonly MuPDFContext OwnerContext;

        /// <summary>
        /// The document from which the page was extracted.
        /// </summary>
        internal readonly MuPDFDocument OwnerDocument;

        /// <summary>
        /// The page's original bounds. Read-only.
        /// </summary>
        internal Rectangle OriginalBounds { get; }

        /// <summary>
        /// Create a new <see cref="MuPDFPage"/> object from the specified document.
        /// </summary>
        /// <param name="context">The context that owns the document.</param>
        /// <param name="document">The document from which the page should be extracted.</param>
        /// <param name="number">The number of the page that should be extracted (starting at 0).</param>
        internal MuPDFPage(MuPDFContext context, MuPDFDocument document, int number)
        {
            if (document.EncryptionState == EncryptionState.Encrypted)
            {
                throw new DocumentLockedException("A password is necessary to render the document!");
            }

            this.OwnerContext = context;
            this.OwnerDocument = document;
            this.PageNumber = number;

            float x = 0;
            float y = 0;
            float w = 0;
            float h = 0;

            ExitCodes result = (ExitCodes)NativeMethods.LoadPage(context.NativeContext, document.NativeDocument, number, ref NativePage, ref x, ref y, ref w, ref h);

            double sX = Math.Round(x * document.ImageXRes / 72.0 * 1000) / 1000;
            double sY = Math.Round(y * document.ImageYRes / 72.0 * 1000) / 1000;
            double sW = Math.Round(w * document.ImageXRes / 72.0 * 1000) / 1000;
            double sH = Math.Round(h * document.ImageYRes / 72.0 * 1000) / 1000;

            this.Bounds = new Rectangle(sX, sY, sX + sW, sY + sH);
            this.OriginalBounds = new Rectangle(x, y, x + w, y + h);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_LOAD_PAGE:
                    throw new MuPDFException("Cannot load page", result);
                case ExitCodes.ERR_CANNOT_COMPUTE_BOUNDS:
                    throw new MuPDFException("Cannot compute bounds", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }
        }

        /// <summary>
        /// Gets the specified bounding box from the page.
        /// </summary>
        /// <param name="boxType">The type of bounding box to get.</param>
        /// <param name="rescale">If this is <see langword="true"/>, the bounding box is rescaled so that it is expressed in the same resolution units as the underlying document. If this is <see langword="false"/>, the raw value is returned (at 72 dpi).</param>
        /// <returns>A <see cref="Rectangle"/> corresponding to the specified bounding box.</returns>
        /// <exception cref="MuPDFException">Thrown if the bounding box cannot be computed or if another error occurs.</exception>
        public Rectangle GetBoundingBox(BoxType boxType, bool rescale = true)
        {
            float x = 0;
            float y = 0;
            float w = 0;
            float h = 0;

            ExitCodes result = (ExitCodes)NativeMethods.GetPageBox(this.OwnerContext.NativeContext, this.NativePage, (int)boxType, ref x, ref y, ref w, ref h);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_COMPUTE_BOUNDS:
                    throw new MuPDFException("Cannot compute bounds", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            if (rescale)
            {
                double sX = Math.Round(x * this.OwnerDocument.ImageXRes / 72.0 * 1000) / 1000;
                double sY = Math.Round(y * this.OwnerDocument.ImageYRes / 72.0 * 1000) / 1000;
                double sW = Math.Round(w * this.OwnerDocument.ImageXRes / 72.0 * 1000) / 1000;
                double sH = Math.Round(h * this.OwnerDocument.ImageYRes / 72.0 * 1000) / 1000;

                return new Rectangle(sX, sY, sX + sW, sY + sH);
            }
            else
            {
                return new Rectangle(x, y, x + w, y + h);
            }
        }
        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (OwnerContext.disposedValue)
                {
                    throw new LifetimeManagementException<MuPDFPage, MuPDFContext>(this, OwnerContext, this.NativePage, OwnerContext.NativeContext);
                }
                if (disposing)
                {
                    this.Links.Dispose();
                }

                NativeMethods.DisposePage(OwnerContext.NativeContext, NativePage);
                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~MuPDFPage()
        {
            if (NativePage != IntPtr.Zero)
            {
                Dispose(disposing: false);
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// A lazy collection of <see cref="MuPDFPage"/>s. Each page is loaded from the document as it is requested for the first time.
    /// </summary>
    public class MuPDFPageCollection : IReadOnlyList<MuPDFPage>, IDisposable
    {
        /// <summary>
        /// The internal store of the pages.
        /// </summary>
        private readonly MuPDFPage[] Pages;

        /// <summary>
        /// The context that owns the document from which the pages were extracted.
        /// </summary>
        private readonly MuPDFContext OwnerContext;

        /// <summary>
        /// The document from which the pages were extracted.
        /// </summary>
        private readonly MuPDFDocument OwnerDocument;

        /// <summary>
        /// The number of pages in the collection.
        /// </summary>
        public int Length { get { return Pages.Length; } }

        /// <summary>
        /// The number of pages in the collection.
        /// </summary>
        public int Count { get { return Pages.Length; } }

        /// <summary>
        /// Get a page from the collection.
        /// </summary>
        /// <param name="index">The number of the page (starting at 0).</param>
        /// <returns>The specified <see cref="MuPDFPage"/>.</returns>
        public MuPDFPage this[int index]
        {
            get
            {
                if (index < 0 || index > Pages.Length)
                {
                    throw new IndexOutOfRangeException();
                }

                if (Pages[index] == null)
                {
                    Pages[index] = new MuPDFPage(OwnerContext, OwnerDocument, index);
                }

                return Pages[index];
            }
        }

        /// <summary>
        /// Create a new <see cref="MuPDFPageCollection"/> from the specified document, containing the specified number of pages.
        /// </summary>
        /// <param name="context">The context that owns the document.</param>
        /// <param name="document">The document from which the pages should be extracted.</param>
        /// <param name="length">The number of pages in the document.</param>
        internal MuPDFPageCollection(MuPDFContext context, MuPDFDocument document, int length)
        {
            Pages = new MuPDFPage[length];
            OwnerContext = context;
            OwnerDocument = document;
        }

        ///<inheritdoc/>
        public IEnumerator<MuPDFPage> GetEnumerator()
        {
            for (int i = 0; i < Pages.Length; i++)
            {
                if (Pages[i] == null)
                {
                    Pages[i] = new MuPDFPage(OwnerContext, OwnerDocument, i);
                }
            }

            return ((IEnumerable<MuPDFPage>)Pages).GetEnumerator();
        }

        ///<inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    for (int i = 0; i < Pages.Length; i++)
                    {
                        Pages[i]?.Dispose();
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
