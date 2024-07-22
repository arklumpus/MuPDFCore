/*
    MuPDFCore - A set of multiplatform .NET Core bindings for MuPDF.
    Copyright (C) 2024  Giorgio Bianchini

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
using System.Text;


namespace MuPDFCore
{
    /// <summary>
    /// Represents a font.
    /// </summary>
    public class MuPDFFont : IDisposable
    {
        internal bool disposedValue;

        /// <summary>
        /// Returns whether the font is bold or not.
        /// </summary>
        public bool IsBold { get; }

        /// <summary>
        /// Returns whether the font is italic or not.
        /// </summary>
        public bool IsItalic { get; }

        /// <summary>
        /// Returns whether the font is serif or not.
        /// </summary>
        public bool IsSerif { get; }

        /// <summary>
        /// Returns whether the font is monospaced or not.
        /// </summary>
        public bool IsMonospaced { get; }

        /// <summary>
        /// The name of the font.
        /// </summary>
        public string Name { get; }

        private MuPDFContext OwnerContext { get; }
        private IntPtr NativePointer { get; }

        internal unsafe MuPDFFont(MuPDFContext context, IntPtr nativePointer)
        {
            int nameLength = 0;
            int bold = -1;
            int italic = -1;
            int serif = -1;
            int monospaced = -1;

            ExitCodes result = (ExitCodes)NativeMethods.GetFontMetadata(context.NativeContext, nativePointer, ref nameLength, ref bold, ref italic, ref serif, ref monospaced);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_FONT_METADATA:
                    throw new MuPDFException("An error occurred while retrieving font metadata!", result);
                default:
                    throw new MuPDFException("Unknown error!", result);
            }

            byte[] fontName = new byte[nameLength];

            fixed (byte* fontNamePtr = fontName)
            {
                result = (ExitCodes)NativeMethods.GetFontName(context.NativeContext, nativePointer, nameLength, (IntPtr)fontNamePtr);
            }

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_FONT_METADATA:
                    throw new MuPDFException("An error occurred while retrieving font metadata!", result);
                default:
                    throw new MuPDFException("Unknown error!", result);
            }

            this.IsBold = bold != 0;
            this.IsItalic = italic != 0;
            this.IsSerif = serif != 0;
            this.IsMonospaced = monospaced != 0;
            this.Name = Encoding.ASCII.GetString(fontName);

            this.OwnerContext = context;
            this.NativePointer = nativePointer;
        }

        /// <summary>
        /// Get a pointer to the FreeType FT_Face object for this font. You will need native bindings to the FreeType library to use this.
        /// </summary>
        /// <returns>A pointer to the FreeType FT_Face object for this font, or <see cref="IntPtr.Zero"/> if this is a Type3 font.</returns>
        /// <exception cref="MuPDFException">Thrown if an error occurs while accessing the FreeType handle for the font.</exception>
        public IntPtr GetFreeTypeHandle()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("MuPDFFont", "The MuPDFFont object has already been disposed! Maybe you disposed the MuPDFStructuredTextPage that contained it?");
            }

            IntPtr tbr = IntPtr.Zero;

            ExitCodes result = (ExitCodes)NativeMethods.GetFTHandle(this.OwnerContext.NativeContext, this.NativePointer, ref tbr);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_FONT_METADATA:
                    throw new MuPDFException("Cannot get the font handle!", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            return tbr;
        }

        /// <summary>
        /// Get a pointer to the Type3 procs for the font. You will need some more specialised MuPDF bindings to do anything with it.
        /// </summary>
        /// <returns>A pointer to the Type3 procs for the font, or <see cref="IntPtr.Zero"/> if this is not a Type3 font.</returns>
        /// <exception cref="MuPDFException">Thrown if an error occurs while accessing the Type3 font procs.</exception>
        public IntPtr GetType3Handle()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException("MuPDFFont", "The MuPDFFont object has already been disposed! Maybe you disposed the MuPDFStructuredTextPage that contained it?");
            }

            IntPtr tbr = IntPtr.Zero;

            ExitCodes result = (ExitCodes)NativeMethods.GetT3Procs(this.OwnerContext.NativeContext, this.NativePointer, ref tbr);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_FONT_METADATA:
                    throw new MuPDFException("Cannot get the font handle!", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            return tbr;
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (OwnerContext.disposedValue)
                {
                    throw new LifetimeManagementException<MuPDFFont, MuPDFContext>(this, OwnerContext, this.NativePointer, OwnerContext.NativeContext);
                }

                lock (OwnerContext.FontCacheLock)
                {
                    (MuPDFFont font, int referenceCount) item;
                    if (OwnerContext.FontCache.TryGetValue(this.NativePointer, out item))
                    {
                        if (item.referenceCount <= 1)
                        {
                            NativeMethods.DisposeFont(this.OwnerContext.NativeContext, this.NativePointer);
                            OwnerContext.FontCache.Remove(this.NativePointer);
                            disposedValue = true;
                        }
                        else
                        {
                            item = (item.font, item.referenceCount - 1);
                            OwnerContext.FontCache[this.NativePointer] = item;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dispose the font object.
        /// </summary>
        ~MuPDFFont()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
