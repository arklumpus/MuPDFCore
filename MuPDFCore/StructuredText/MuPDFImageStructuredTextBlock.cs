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

namespace MuPDFCore.StructuredText
{
    /// <summary>
    /// Represents a block containing a single image. The block contains a single line with a single character.
    /// </summary>
    public class MuPDFImageStructuredTextBlock : MuPDFStructuredTextBlock, IDisposable
    {
        /// <inheritdoc/>
        public override Types Type => Types.Image;

        /// <inheritdoc/>
        public override int Count => 1;

        /// <summary>
        /// Transform matrix for the image. This maps the { { 0, 0 }, { 1, 1 } } square to the actual size, position, and rotation of the image in the document.
        /// </summary>
        public float[,] TransformMatrix { get; }

        /// <summary>
        /// The image contained in the block.
        /// </summary>
        public MuPDFImage Image { get; }

        private readonly MuPDFStructuredTextLine Line;
        private bool disposedValue;

        /// <inheritdoc/>
        public override MuPDFStructuredTextLine this[int index]
        {
            get
            {
                if (index == 0)
                {
                    return Line;
                }
                else
                {
                    throw new IndexOutOfRangeException("A structured text block containing an image only has one line!");
                }
            }
        }

        internal MuPDFImageStructuredTextBlock(MuPDFContext context, MuPDFStructuredTextPage parentPage, Rectangle boundingBox, IntPtr imagePointer, float a, float b, float c, float d, float e, float f) : base(boundingBox, parentPage)
        {
            this.Line = new MuPDFStructuredTextLine(this.BoundingBox, this);
            this.TransformMatrix = new float[,] { { a, b, 0 }, { c, d, 0 }, { e, f, 1 } };
            this.Image = new MuPDFImage(imagePointer, context, this);
        }

        /// <inheritdoc/>
        public override IEnumerator<MuPDFStructuredTextLine> GetEnumerator()
        {
            return ((IEnumerable<MuPDFStructuredTextLine>)new MuPDFStructuredTextLine[] { Line }).GetEnumerator();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Image?.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}
