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
using System.Collections;
using System.Collections.Generic;

namespace MuPDFCore.StructuredText
{
    /// <summary>
    /// Represents a structured text block containing text or an image.
    /// </summary>
    public abstract class MuPDFStructuredTextBlock : IReadOnlyList<MuPDFStructuredTextLine>, IDisposable
    {
        /// <summary>
        /// Defines the type of the block.
        /// </summary>
        public enum Types
        {
            /// <summary>
            /// The block contains text.
            /// </summary>
            Text = 0,

            /// <summary>
            /// The block contains an image.
            /// </summary>
            Image = 1,

            /// <summary>
            /// The block represents a structural element.
            /// </summary>
            Structure = 2,

            /// <summary>
            /// The block contains vector art.
            /// </summary>
            Vector = 3,

            /// <summary>
            /// The box contains "grid" lines.
            /// </summary>
            Grid = 4
        }

        /// <summary>
        /// The type of the block.
        /// </summary>
        public abstract Types Type { get; }

        /// <summary>
        /// The bounding box of the block.
        /// </summary>
        public Rectangle BoundingBox { get; protected set; }

        /// <summary>
        /// The number of lines in the block.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// The <see cref="MuPDFStructuredTextPage"/> that contains this <see cref="MuPDFStructuredTextBlock"/>.
        /// </summary>
        public MuPDFStructuredTextPage ParentPage { get; }

        /// <summary>
        /// Gets the specified line from the block.
        /// </summary>
        /// <param name="index">The index of the line to extract.</param>
        /// <returns>The <see cref="MuPDFStructuredTextLine"/> with the specified <paramref name="index"/>.</returns>
        public abstract MuPDFStructuredTextLine this[int index] { get; }

        internal MuPDFStructuredTextBlock(Rectangle boundingBox, MuPDFStructuredTextPage parentPage)
        {
            this.BoundingBox = boundingBox;
            this.ParentPage = parentPage;
        }

        /// <inheritdoc/>
        public abstract IEnumerator<MuPDFStructuredTextLine> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <inheritdoc/>
        public abstract void Dispose();

        internal static MuPDFStructuredTextBlock Create(MuPDFContext context, IntPtr blockPointer, MuPDFStructuredTextPage parentPage, MuPDFStructureStructuredTextBlock parentBlock)
        {
            int type = -1;
            float x0 = -1;
            float y0 = -1;
            float x1 = -1;
            float y1 = -1;
            int lineCount = -1;
            IntPtr imagePointer = IntPtr.Zero;

            float a = 0;
            float b = 0;
            float c = 0;
            float d = 0;
            float e = 0;
            float f = 0;

            byte stroked = 0;
            byte rgba_r = 0;
            byte rgba_g = 0;
            byte rgba_b = 0;
            byte rgba_a = 0;

            int xs_len = 0;
            int ys_len = 0;

            IntPtr down = IntPtr.Zero;
            int index = 0;

            ExitCodes result = (ExitCodes)NativeMethods.GetStructuredTextBlock(context.NativeContext, blockPointer, ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref imagePointer, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref rgba_r, ref rgba_g, ref rgba_b, ref rgba_a, ref xs_len, ref ys_len, ref down, ref index);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            Rectangle bBox = new Rectangle(x0, y0, x1, y1);

            switch ((Types)type)
            {
                case Types.Image:
                    return new MuPDFImageStructuredTextBlock(context, parentPage, bBox, imagePointer, a, b, c, d, e, f);
                case Types.Text:
                    return new MuPDFTextStructuredTextBlock(context, parentPage, bBox, blockPointer, lineCount);
                case Types.Vector:
                    return new MuPDFVectorStructuredTextBlock(parentPage, bBox, stroked != 0, rgba_r, rgba_g, rgba_b, rgba_a);
                case Types.Grid:
                    return new MuPDFGridStructuredTextBlock(context, parentPage, bBox, blockPointer, xs_len, ys_len);
                case Types.Structure:
                    return new MuPDFStructureStructuredTextBlock(context, parentPage, bBox, down, index, parentBlock);
                default:
                    throw new ArgumentException("Unknown structured text block type: " + type.ToString());
            }
        }
    }
}
