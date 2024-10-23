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
    /// Represents the position of a grid line.
    /// </summary>
    public class MuPDFGridLine
    {
        /// <summary>
        /// Position of the grid line.
        /// </summary>
        public float Position { get; }

        /// <summary>
        /// Uncertainty in the position of the grid line.
        /// </summary>
        public int Uncertainty { get; }

        internal MuPDFGridLine(float position, int uncertainty)
        {
            this.Position = position;
            this.Uncertainty = uncertainty;
        }
    }

    /// <summary>
    /// A structured text block representing "grid" lines.
    /// </summary>
    public class MuPDFGridStructuredTextBlock : MuPDFStructuredTextBlock
    {
        /// <summary>
        /// X grid lines.
        /// </summary>
        public MuPDFGridLine[] XGrid { get; }

        /// <summary>
        /// Maximum uncertainty for the X grid lines.
        /// </summary>
        public int MaxUncertaintyX { get; }

        /// <summary>
        /// Y grid lines.
        /// </summary>
        public MuPDFGridLine[] YGrid { get; }

        /// <summary>
        /// Maximum uncertainty for the Y grid lines.
        /// </summary>
        public int MaxUncertaintyY { get; }

        /// <inheritdoc/>
        public override Types Type => Types.Grid;

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
                    throw new IndexOutOfRangeException("A structured text block containing a grid only has one line!");
                }
            }
        }

        internal unsafe MuPDFGridStructuredTextBlock(MuPDFContext context, MuPDFStructuredTextPage parentPage, Rectangle boundingBox, IntPtr blockPtr, int xs_len, int ys_len) : base(boundingBox, parentPage)
        {
            this.Line = new MuPDFStructuredTextLine(this.BoundingBox, this);

            ExitCodes result;

            int xMaxUncertainty = 0;
            int yMaxUncertainty = 0;

            float[] xPos = new float[xs_len];
            int[] xUncertainty = new int[xs_len];
            float[] yPos = new float[ys_len];
            int[] yUncertainty = new int[ys_len];

            fixed (float* xPosPtr = xPos)
            fixed (int* xUncertaintyPtr = xUncertainty)
            fixed (float* yPosPtr = yPos)
            fixed (int* yUncertaintyPtr = yUncertainty)
            {
                result = (ExitCodes)NativeMethods.GetGridStructuredTextBlock(blockPtr, xs_len, ys_len, ref xMaxUncertainty, ref yMaxUncertainty, (IntPtr)xPosPtr, (IntPtr)yPosPtr, (IntPtr)xUncertaintyPtr, (IntPtr)yUncertaintyPtr);
            }

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            this.XGrid = new MuPDFGridLine[xs_len];
            this.YGrid = new MuPDFGridLine[ys_len];

            for (int i = 0; i < xs_len; i++)
            {
                this.XGrid[i] = new MuPDFGridLine(xPos[i], xUncertainty[i]);
            }

            for (int i = 0; i < ys_len; i++)
            {
                this.YGrid[i] = new MuPDFGridLine(yPos[i], yUncertainty[i]);
            }

            this.MaxUncertaintyX = xMaxUncertainty;
            this.MaxUncertaintyY = yMaxUncertainty;
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
