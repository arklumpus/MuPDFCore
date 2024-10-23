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
    /// Represents a block of vector art.
    /// </summary>
    public class MuPDFVectorStructuredTextBlock : MuPDFStructuredTextBlock
    {
        /// <summary>
        /// Whether the <see cref="MuPDFVectorStructuredTextBlock"/> is stroked or not.
        /// </summary>
        public bool Stroked { get; }

        /// <summary>
        /// The stroke colour (RGBA).
        /// </summary>
        public byte[] Color { get; }

        /// <inheritdoc/>
        public override Types Type => Types.Vector;

        /// <inheritdoc/>
        public override int Count => 1;

        private readonly MuPDFStructuredTextLine Line;

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
                    throw new IndexOutOfRangeException("A structured text block containing vector art only has one line!");
                }
            }
        }

        internal MuPDFVectorStructuredTextBlock(MuPDFStructuredTextPage parentPage, Rectangle boundingBox, bool stroked, byte r, byte g, byte b, byte a) : base(boundingBox, parentPage)
        {
            this.Line = new MuPDFStructuredTextLine(this.BoundingBox, this);
            Stroked = stroked;
            Color = new byte[] { r, g, b, a };
        }

        /// <inheritdoc/>
        public override IEnumerator<MuPDFStructuredTextLine> GetEnumerator()
        {
            return ((IEnumerable<MuPDFStructuredTextLine>)new MuPDFStructuredTextLine[] { Line }).GetEnumerator();
        }

        /// <inheritdoc/>
        public override void Dispose() { }
    }
}
