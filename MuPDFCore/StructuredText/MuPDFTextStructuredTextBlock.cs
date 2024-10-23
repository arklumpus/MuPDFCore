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
using System.Runtime.InteropServices;
using System.Text;

namespace MuPDFCore.StructuredText
{
    /// <summary>
    /// Represents a block containing multiple lines of text (typically a paragraph).
    /// </summary>
    public class MuPDFTextStructuredTextBlock : MuPDFStructuredTextBlock, IDisposable
    {
        private bool disposedValue;

        /// <inheritdoc/>
        public override Types Type => Types.Text;

        /// <summary>
        /// The lines of text in the block.
        /// </summary>
        public MuPDFStructuredTextLine[] Lines { get; }

        /// <inheritdoc/>
        public override int Count => ((IReadOnlyCollection<MuPDFStructuredTextLine>)Lines).Count;

        /// <inheritdoc/>
        public override MuPDFStructuredTextLine this[int index] => ((IReadOnlyList<MuPDFStructuredTextLine>)Lines)[index];

        internal MuPDFTextStructuredTextBlock(MuPDFContext context, MuPDFStructuredTextPage parentPage, Rectangle boundingBox, IntPtr blockPointer, int lineCount) : base(boundingBox, parentPage)
        {
            IntPtr[] linePointers = new IntPtr[lineCount];
            GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

            try
            {
                ExitCodes result = (ExitCodes)NativeMethods.GetStructuredTextLines(blockPointer, linesHandle.AddrOfPinnedObject());

                switch (result)
                {
                    case ExitCodes.EXIT_SUCCESS:
                        break;
                    default:
                        throw new MuPDFException("Unknown error", result);
                }

                Lines = new MuPDFStructuredTextLine[lineCount];

                for (int i = 0; i < lineCount; i++)
                {
                    int wmode = -1;
                    float x0 = -1;
                    float y0 = -1;
                    float x1 = -1;
                    float y1 = -1;

                    float x = -1;
                    float y = -1;

                    int charCount = -1;

                    result = (ExitCodes)NativeMethods.GetStructuredTextLine(linePointers[i], ref wmode, ref x0, ref y0, ref x1, ref y1, ref x, ref y, ref charCount);

                    switch (result)
                    {
                        case ExitCodes.EXIT_SUCCESS:
                            break;
                        default:
                            throw new MuPDFException("Unknown error", result);
                    }

                    Rectangle bBox = new Rectangle(x0, y0, x1, y1);
                    PointF direction = new PointF(x, y);

                    Lines[i] = new MuPDFStructuredTextLine(context, this, linePointers[i], (MuPDFStructuredTextLine.WritingModes)wmode, direction, bBox, charCount);
                }
            }
            finally
            {
                linesHandle.Free();
            }
        }

        /// <inheritdoc/>
        public override IEnumerator<MuPDFStructuredTextLine> GetEnumerator()
        {
            return ((IEnumerable<MuPDFStructuredTextLine>)Lines).GetEnumerator();
        }

        /// <summary>
        /// Returns the text contained in the block as a <see cref="string"/>.
        /// </summary>
        /// <returns>The text contained in the block as a <see cref="string"/>. If the block contains at least one line, the return value has a line terminator at the end.</returns>
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();

            foreach (MuPDFStructuredTextLine line in this)
            {
                text.AppendLine(line.Text);
            }

            return text.ToString();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.Lines != null)
                    {
                        for (int i = 0; i < Lines.Length; i++)
                        {
                            this.Lines?[i]?.Dispose();
                        }
                    }
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

    /// <summary>
    /// Represents a single line of text (i.e. characters that share a common baseline).
    /// </summary>
    public class MuPDFStructuredTextLine : IReadOnlyList<MuPDFStructuredTextCharacter>, IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Defines the writing mode of the text.
        /// </summary>
        public enum WritingModes
        {
            /// <summary>
            /// The text is written horizontally.
            /// </summary>
            Horizontal = 0,

            /// <summary>
            /// The text is written vertically.
            /// </summary>
            Vertical = 1
        }

        /// <summary>
        /// The writing mode of the text.
        /// </summary>
        public WritingModes WritingMode { get; }

        /// <summary>
        /// The normalised direction of the text baseline.
        /// </summary>
        public PointF Direction { get; }

        /// <summary>
        /// The bounding box of the line.
        /// </summary>
        public Rectangle BoundingBox { get; }

        /// <summary>
        /// The characters contained in the line.
        /// </summary>
        public MuPDFStructuredTextCharacter[] Characters { get; }

        /// <summary>
        /// The <see cref="MuPDFStructuredTextBlock"/> that contains this <see cref="MuPDFStructuredTextLine"/>.
        /// </summary>
        public MuPDFStructuredTextBlock ParentBlock { get; }

        /// <summary>
        /// A string representation of the characters contained in the line.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The number of characters in the line.
        /// </summary>
        public int Count => ((IReadOnlyCollection<MuPDFStructuredTextCharacter>)Characters).Count;

        /// <summary>
        /// Gets the specified character from the line.
        /// </summary>
        /// <param name="index">The index of the character.</param>
        /// <returns>The <see cref="MuPDFStructuredTextCharacter"/> with the specified <paramref name="index"/>.</returns>
        public MuPDFStructuredTextCharacter this[int index] => ((IReadOnlyList<MuPDFStructuredTextCharacter>)Characters)[index];

        internal MuPDFStructuredTextLine(Rectangle boundingBox, MuPDFStructuredTextBlock parentBlock)
        {
            this.BoundingBox = boundingBox;
            this.ParentBlock = parentBlock;
            this.Characters = new MuPDFStructuredTextCharacter[]
            {
                new MuPDFStructuredTextCharacter(this, 0, -1, new PointF(boundingBox.X0, boundingBox.Y1), new Quad(new PointF(boundingBox.X0, boundingBox.Y1), new PointF(boundingBox.X0, boundingBox.Y0), new PointF(boundingBox.X1, boundingBox.Y0), new PointF(boundingBox.X1, boundingBox.Y1)), 9, MuPDFStructuredTextCharacter.TextDirection.LeftToRight, null)
            };
        }

        internal MuPDFStructuredTextLine(MuPDFContext context, MuPDFStructuredTextBlock parentBlock, IntPtr linePointer, WritingModes writingMode, PointF direction, Rectangle boundingBox, int charCount)
        {
            this.WritingMode = writingMode;
            this.Direction = direction;
            this.BoundingBox = boundingBox;
            this.ParentBlock = parentBlock;

            IntPtr[] charPointers = new IntPtr[charCount];
            GCHandle charsHandle = GCHandle.Alloc(charPointers, GCHandleType.Pinned);

            try
            {
                ExitCodes result = (ExitCodes)NativeMethods.GetStructuredTextChars(linePointer, charsHandle.AddrOfPinnedObject());

                switch (result)
                {
                    case ExitCodes.EXIT_SUCCESS:
                        break;
                    default:
                        throw new MuPDFException("Unknown error", result);
                }

                Characters = new MuPDFStructuredTextCharacter[charCount];

                StringBuilder textBuilder = new StringBuilder(charCount);

                for (int i = 0; i < charCount; i++)
                {
                    int c = -1;
                    int color = -1;
                    float originX = -1;
                    float originY = -1;
                    float size = -1;
                    float llX = -1;
                    float llY = -1;
                    float ulX = -1;
                    float ulY = -1;
                    float urX = -1;
                    float urY = -1;
                    float lrX = -1;
                    float lrY = -1;
                    int bidi = -1;
                    IntPtr font = IntPtr.Zero;

                    result = (ExitCodes)NativeMethods.GetStructuredTextChar(context.NativeContext, charPointers[i], ref c, ref color, ref originX, ref originY, ref size, ref llX, ref llY, ref ulX, ref ulY, ref urX, ref urY, ref lrX, ref lrY, ref bidi, ref font);

                    switch (result)
                    {
                        case ExitCodes.EXIT_SUCCESS:
                            break;
                        default:
                            throw new MuPDFException("Unknown error", result);
                    }

                    Quad quad = new Quad(new PointF(llX, llY), new PointF(ulX, ulY), new PointF(urX, urY), new PointF(lrX, lrY));
                    PointF origin = new PointF(originX, originY);

                    MuPDFFont muPDFFont = context.Resolve(font);

                    Characters[i] = new MuPDFStructuredTextCharacter(this, c, color, origin, quad, size, bidi % 2 == 0 ? MuPDFStructuredTextCharacter.TextDirection.LeftToRight : MuPDFStructuredTextCharacter.TextDirection.RightToLeft, muPDFFont);
                    textBuilder.Append(Characters[i].Character);
                }

                this.Text = textBuilder.ToString();
            }
            finally
            {
                charsHandle.Free();
            }
        }

        /// <summary>
        /// Returns a string representation of the line.
        /// </summary>
        /// <returns>A string representation of the line.</returns>
        public override string ToString()
        {
            return this.Text;
        }

        /// <inheritdoc/>
        public IEnumerator<MuPDFStructuredTextCharacter> GetEnumerator()
        {
            return ((IEnumerable<MuPDFStructuredTextCharacter>)Characters).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Characters.GetEnumerator();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.Characters != null)
                    {
                        for (int i = 0; i < this.Characters.Length; i++)
                        {
                            this.Characters?[i]?.Dispose();
                        }
                    }
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a single text character.
    /// </summary>
    public class MuPDFStructuredTextCharacter : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Text writing directions.
        /// </summary>
        public enum TextDirection
        {
            /// <summary>
            /// Left to right.
            /// </summary>
            LeftToRight,

            /// <summary>
            /// Right to left.
            /// </summary>
            RightToLeft
        }

        /// <summary>
        /// The unicode code point of the character.
        /// </summary>
        public int CodePoint { get; }

        /// <summary>
        /// A string representation of the character. It may consist of a single <see cref="char"/> or of a surrogate pair of <see cref="char"/>s.
        /// </summary>
        public string Character { get; }

        /// <summary>
        /// An sRGB hex representation of the colour of the character.
        /// </summary>
        public int Color { get; }

        /// <summary>
        /// The baseline origin of the character.
        /// </summary>
        public PointF Origin { get; }

        /// <summary>
        /// A quadrilater bound for the character. This may or may not be a rectangle.
        /// </summary>
        public Quad BoundingQuad { get; }

        /// <summary>
        /// The size in points of the character.
        /// </summary>
        public float Size { get; }

        /// <summary>
        /// Text writing direction.
        /// </summary>
        public TextDirection Direction { get; }

        /// <summary>
        /// The font used to draw the character.
        /// </summary>
        public MuPDFFont Font { get; }

        /// <summary>
        /// The <see cref="MuPDFStructuredTextLine"/> that contains this <see cref="MuPDFStructuredTextCharacter"/>.
        /// </summary>
        public MuPDFStructuredTextLine ParentLine { get; }

        internal MuPDFStructuredTextCharacter(MuPDFStructuredTextLine parentLine, int codePoint, int color, PointF origin, Quad boundingQuad, float size, TextDirection direction, MuPDFFont font)
        {
            this.ParentLine = parentLine;
            this.CodePoint = codePoint;
            this.Character = Char.ConvertFromUtf32(codePoint);
            this.Color = color;
            this.Origin = origin;
            this.BoundingQuad = boundingQuad;
            this.Size = size;
            this.Direction = direction;
            this.Font = font;
        }

        /// <summary>
        /// Returns a string representation of the character.
        /// </summary>
        /// <returns>A string representation of the character.</returns>
        public override string ToString()
        {
            return this.Character;
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Font?.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents the address of a particular character in a <see cref="MuPDFStructuredTextPage"/>, in terms of block index, line index and character index.
    /// </summary>
    public struct MuPDFStructuredTextAddress : IComparable<MuPDFStructuredTextAddress>, IEquatable<MuPDFStructuredTextAddress>
    {
        /// <summary>
        /// The index of the block.
        /// </summary>
        public readonly int BlockIndex;

        /// <summary>
        /// The index of the line within the block.
        /// </summary>
        public readonly int LineIndex;

        /// <summary>
        /// The index of the character within the line.
        /// </summary>
        public readonly int CharacterIndex;

        /// <summary>
        /// Creates a new <see cref="MuPDFStructuredTextAddress"/> from the specified indices.
        /// </summary>
        /// <param name="blockIndex">The index of the block.</param>
        /// <param name="lineIndex">The index of the line within the block.</param>
        /// <param name="characterIndex">The index of the character within the line.</param>
        public MuPDFStructuredTextAddress(int blockIndex, int lineIndex, int characterIndex)
        {
            this.BlockIndex = blockIndex;
            this.LineIndex = lineIndex;
            this.CharacterIndex = characterIndex;
        }

        /// <summary>
        /// Compares this <see cref="MuPDFStructuredTextAddress"/> with another <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="other">The <see cref="MuPDFStructuredTextAddress"/> to compare with the current instance.</param>
        /// <returns>-1 if the <paramref name="other"/> <see cref="MuPDFStructuredTextAddress"/> comes after the current instance, 1 if it comes before, or 0 if they represent the same address.</returns>
        public int CompareTo(MuPDFStructuredTextAddress other)
        {
            if (this < other)
            {
                return -1;
            }
            else if (this > other)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Compares two <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="first">The first <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <param name="second">The second <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <returns><see langword="true"/> if the <paramref name="first"/> <see cref="MuPDFStructuredTextAddress"/> comes after the <paramref name="second"/> one; otherwise, <see langword="false"/>.</returns>
        public static bool operator >(MuPDFStructuredTextAddress first, MuPDFStructuredTextAddress second)
        {
            if (first.BlockIndex > second.BlockIndex)
            {
                return true;
            }
            else if (first.BlockIndex < second.BlockIndex)
            {
                return false;
            }
            else
            {
                if (first.LineIndex > second.LineIndex)
                {
                    return true;
                }
                else if (first.LineIndex < second.LineIndex)
                {
                    return false;
                }
                else
                {
                    if (first.CharacterIndex > second.CharacterIndex)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Compares two <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="first">The first <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <param name="second">The second <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <returns><see langword="true"/> if the <paramref name="first"/> <see cref="MuPDFStructuredTextAddress"/> comes after the <paramref name="second"/> one or if they represent the same address; otherwise, <see langword="false"/>.</returns>
        public static bool operator >=(MuPDFStructuredTextAddress first, MuPDFStructuredTextAddress second)
        {
            if (first.BlockIndex > second.BlockIndex)
            {
                return true;
            }
            else if (first.BlockIndex < second.BlockIndex)
            {
                return false;
            }
            else
            {
                if (first.LineIndex > second.LineIndex)
                {
                    return true;
                }
                else if (first.LineIndex < second.LineIndex)
                {
                    return false;
                }
                else
                {
                    if (first.CharacterIndex >= second.CharacterIndex)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Compares two <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="first">The first <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <param name="second">The second <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <returns><see langword="true"/> if the <paramref name="first"/> <see cref="MuPDFStructuredTextAddress"/> comes before the <paramref name="second"/> one; otherwise, <see langword="false"/>.</returns>
        public static bool operator <(MuPDFStructuredTextAddress first, MuPDFStructuredTextAddress second)
        {
            if (first.BlockIndex > second.BlockIndex)
            {
                return false;
            }
            else if (first.BlockIndex < second.BlockIndex)
            {
                return true;
            }
            else
            {
                if (first.LineIndex > second.LineIndex)
                {
                    return false;
                }
                else if (first.LineIndex < second.LineIndex)
                {
                    return true;
                }
                else
                {
                    if (first.CharacterIndex < second.CharacterIndex)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Compares two <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="first">The first <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <param name="second">The second <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <returns><see langword="true"/> if the <paramref name="first"/> <see cref="MuPDFStructuredTextAddress"/> comes before the <paramref name="second"/> one or if they represent the same address; otherwise, <see langword="false"/>.</returns>
        public static bool operator <=(MuPDFStructuredTextAddress first, MuPDFStructuredTextAddress second)
        {
            if (first.BlockIndex > second.BlockIndex)
            {
                return false;
            }
            else if (first.BlockIndex < second.BlockIndex)
            {
                return true;
            }
            else
            {
                if (first.LineIndex > second.LineIndex)
                {
                    return false;
                }
                else if (first.LineIndex < second.LineIndex)
                {
                    return true;
                }
                else
                {
                    if (first.CharacterIndex <= second.CharacterIndex)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Compares two <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="first">The first <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <param name="second">The second <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="MuPDFStructuredTextAddress"/>es represent the same address; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(MuPDFStructuredTextAddress first, MuPDFStructuredTextAddress second)
        {
            return first.CharacterIndex == second.CharacterIndex && first.LineIndex == second.LineIndex && first.BlockIndex == second.BlockIndex;
        }

        /// <summary>
        /// Compares two <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="first">The first <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <param name="second">The second <see cref="MuPDFStructuredTextAddress"/> to compare.</param>
        /// <returns><see langword="true"/> if the two <see cref="MuPDFStructuredTextAddress"/>es represent different addresses; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(MuPDFStructuredTextAddress first, MuPDFStructuredTextAddress second)
        {
            return first.CharacterIndex != second.CharacterIndex || first.LineIndex != second.LineIndex || first.BlockIndex != second.BlockIndex;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.BlockIndex * 33 * 33) ^ this.LineIndex * 33) ^ this.CharacterIndex;
            }
        }

        /// <summary>
        /// Returns a <see cref="MuPDFStructuredTextAddress"/> corresponding to the next character in the specified page.
        /// </summary>
        /// <param name="page">The page the address refers to.</param>
        /// <returns>A <see cref="MuPDFStructuredTextAddress"/> corresponding to the next character in the specified page.</returns>
        public MuPDFStructuredTextAddress? Increment(MuPDFStructuredTextPage page)
        {
            int newBlockIndex = this.BlockIndex;
            int newLineIndex = this.LineIndex;
            int newCharacterIndex = this.CharacterIndex + 1;

            if (page[newBlockIndex][newLineIndex].Count <= newCharacterIndex)
            {
                newCharacterIndex = 0;
                newLineIndex++;
            }

            if (page[newBlockIndex].Count <= newLineIndex)
            {
                newLineIndex = 0;
                newBlockIndex++;
            }

            if (page.Count <= newBlockIndex)
            {
                return null;
            }

            return new MuPDFStructuredTextAddress(newBlockIndex, newLineIndex, newCharacterIndex);
        }

        /// <summary>
        /// Compares the current <see cref="MuPDFStructuredTextAddress"/> with another <see cref="MuPDFStructuredTextAddress"/>.
        /// </summary>
        /// <param name="other">The other <see cref="MuPDFStructuredTextAddress"/> to compare with the current instance.</param>
        /// <returns><see langword="true"/> if the two <see cref="MuPDFStructuredTextAddress"/>es represent the same address; otherwise, <see langword="false"/>.</returns>
        public bool Equals(MuPDFStructuredTextAddress other)
        {
            return this.CharacterIndex == other.CharacterIndex && this.LineIndex == other.LineIndex && this.BlockIndex == other.BlockIndex;
        }

        /// <inheritdoc/>
        public override bool Equals(object other)
        {
            return other is MuPDFStructuredTextAddress otherAddress && Equals(otherAddress);
        }
    }

    /// <summary>
    /// Represents a range of characters in a <see cref="MuPDFStructuredTextPage"/>.
    /// </summary>
    public class MuPDFStructuredTextAddressSpan
    {
        /// <summary>
        /// The addres of the start of the range.
        /// </summary>
        public readonly MuPDFStructuredTextAddress Start;

        /// <summary>
        /// The address of the end of the range (inclusive), or <see langword="null" /> to signify an empty range.
        /// </summary>
        public readonly MuPDFStructuredTextAddress? End;

        /// <summary>
        /// Creates a new <see cref="MuPDFStructuredTextAddressSpan"/> corresponding to the specified character range.
        /// </summary>
        /// <param name="start">The addres of the start of the range.</param>
        /// <param name="end">The address of the end of the range (inclusive), or <see langword="null" /> to signify an empty range.</param>
        public MuPDFStructuredTextAddressSpan(MuPDFStructuredTextAddress start, MuPDFStructuredTextAddress? end)
        {
            this.Start = start;
            this.End = end;
        }
    }

}
