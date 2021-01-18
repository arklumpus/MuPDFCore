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

namespace MuPDFCore
{
    /// <summary>
    /// A wrapper around a MuPDF display list object, which contains the necessary informations to render a page to an image.
    /// </summary>
    internal class MuPDFDisplayList : IDisposable
    {
        /// <summary>
        /// The context that owns the document that was used to create this display list.
        /// </summary>
        private readonly MuPDFContext OwnerContext;

        /// <summary>
        /// A pointer to the native display list object.
        /// </summary>
        readonly internal IntPtr NativeDisplayList;

        /// <summary>
        /// The display list's bounds in page units. Read-only.
        /// </summary>
        public Rectangle Bounds { get; }

        /// <summary>
        /// Create a new <see cref="MuPDFDisplayList"/> instance from the specified page.
        /// </summary>
        /// <param name="context">The context that owns the document from which the page was taken.</param>
        /// <param name="page">The page from which the display list should be generated.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        public MuPDFDisplayList(MuPDFContext context, MuPDFPage page, bool includeAnnotations = true)
        {
            this.OwnerContext = context;

            float x0 = 0;
            float y0 = 0;
            float x1 = 0;
            float y1 = 0;

            ExitCodes result = (ExitCodes)NativeMethods.GetDisplayList(context.NativeContext, page.NativePage, includeAnnotations ? 1 : 0, ref NativeDisplayList, ref x0, ref y0, ref x1, ref y1);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            this.Bounds = new Rectangle(Math.Round(x0 * page.OwnerDocument.ImageXRes / 72.0 * 1000) / 1000, Math.Round(y0 * page.OwnerDocument.ImageYRes / 72.0 * 1000) / 1000, Math.Round(x1 * page.OwnerDocument.ImageXRes / 72.0 * 1000) / 1000, Math.Round(y1 * page.OwnerDocument.ImageYRes / 72.0 * 1000) / 1000);
        }

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                NativeMethods.DisposeDisplayList(OwnerContext.NativeContext, NativeDisplayList);
                disposedValue = true;
            }
        }

        ~MuPDFDisplayList()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
