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
    public class MuPDFFont
    {
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

        private IntPtr NativeContext { get; }
        private IntPtr NativePointer { get; }

        private static Dictionary<IntPtr, MuPDFFont> FontCache { get; } = new Dictionary<IntPtr, MuPDFFont>();

        internal static MuPDFFont Resolve(MuPDFContext context, IntPtr nativePointer)
        {
            if (!FontCache.TryGetValue(nativePointer, out MuPDFFont font))
            {
                font = new MuPDFFont(context, nativePointer);
                FontCache[nativePointer] = font;
            }

            return font;
        }

        private unsafe MuPDFFont(MuPDFContext context, IntPtr nativePointer)
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

            this.NativeContext = context.NativeContext;
            this.NativePointer = nativePointer;

            Console.WriteLine("New font: {1}{2}{3}{4} {0}", this.Name, bold, italic, serif, monospaced);

            try
            {
                IntPtr freeType = this.GetFreeTypeHandle();
                IntPtr type3 = this.GetType3Handle();
                Console.WriteLine("Handles: {0}   {1}", freeType, type3);
            }
            catch (MuPDFException)
            {
                Console.WriteLine("Handle error!");
            }
        }

        /// <summary>
        /// Get a pointer to the FreeType FT_Face object for this font. You will need native bindings to the FreeType library to use this.
        /// </summary>
        /// <returns>A pointer to the FreeType FT_Face object for this font, or <see cref="IntPtr.Zero"/> if this is a Type3 font.</returns>
        /// <exception cref="MuPDFException">Thrown if an error occurs while accessing the FreeType handle for the font.</exception>
        public IntPtr GetFreeTypeHandle()
        {
            IntPtr tbr = IntPtr.Zero;

            ExitCodes result = (ExitCodes)NativeMethods.GetFTHandle(this.NativeContext, this.NativePointer, ref tbr);

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
            IntPtr tbr = IntPtr.Zero;

            ExitCodes result = (ExitCodes)NativeMethods.GetT3Procs(this.NativeContext, this.NativePointer, ref tbr);

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
    }
}
