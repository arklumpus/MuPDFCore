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

namespace MuPDFCore
{
    /// <summary>
    /// A wrapper over a MuPDF page object, which contains information about the page's boundaries.
    /// </summary>
    public class MuPDFPage : IDisposable
    {
        /// <summary>
        /// The page's bounds in page units. Read-only.
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        /// The number of this page in the original document.
        /// </summary>
        public int PageNumber { get; }

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
        /// Create a new <see cref="MuPDFPage"/> object from the specified document.
        /// </summary>
        /// <param name="context">The context that owns the document.</param>
        /// <param name="document">The document from which the page should be extracted.</param>
        /// <param name="number">The number of the page that should be extracted (starting at 0).</param>
        internal MuPDFPage(MuPDFContext context, MuPDFDocument document, int number)
        {
            this.OwnerContext = context;
            this.OwnerDocument = document;
            this.PageNumber = number;

            float x = 0;
            float y = 0;
            float w = 0;
            float h = 0;

            ExitCodes result = (ExitCodes)NativeMethods.LoadPage(context.NativeContext, document.NativeDocument, number, ref NativePage, ref x, ref y, ref w, ref h);

            this.Bounds = new Rectangle(x, y, w, h);

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

        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                NativeMethods.DisposePage(OwnerContext.NativeContext, NativePage);
                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~MuPDFPage()
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

            return (IEnumerator<MuPDFPage>)Pages.GetEnumerator();
        }

        ///<inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Pages.Length; i++)
            {
                if (Pages[i] == null)
                {
                    Pages[i] = new MuPDFPage(OwnerContext, OwnerDocument, i);
                }
            }

            return Pages.GetEnumerator();
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
