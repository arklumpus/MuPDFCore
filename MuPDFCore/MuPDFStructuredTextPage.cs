using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MuPDFCore
{
    /// <summary>
    /// Represents a structured representation of the text contained in a page.
    /// </summary>
    public class MuPDFStructuredTextPage : IReadOnlyList<MuPDFStructuredTextBlock>
    {
        /// <summary>
        /// The blocks contained in the page.
        /// </summary>
        public MuPDFStructuredTextBlock[] StructuredTextBlocks { get; }

        /// <summary>
        /// The number of blocks in the page.
        /// </summary>
        public int Count => ((IReadOnlyCollection<MuPDFStructuredTextBlock>)StructuredTextBlocks).Count;

        /// <summary>
        /// Gets the specified block in the page.
        /// </summary>
        /// <param name="index">The index of the block.</param>
        /// <returns>The block with the specified <paramref name="index"/>.</returns>
        public MuPDFStructuredTextBlock this[int index] => ((IReadOnlyList<MuPDFStructuredTextBlock>)StructuredTextBlocks)[index];

        /// <summary>
        /// Gets the specified character in the page.
        /// </summary>
        /// <param name="address">The address (block, line and character index) of the character.</param>
        /// <returns>A <see cref="MuPDFStructuredTextCharacter"/> representing the specified character.</returns>
        public MuPDFStructuredTextCharacter this[MuPDFStructuredTextAddress address]
        {
            get
            {
                return StructuredTextBlocks[address.BlockIndex][address.LineIndex][address.CharacterIndex];
            }
        }

        internal MuPDFStructuredTextPage(MuPDFContext context, MuPDFDisplayList list, TesseractLanguage ocrLanguage, double zoom, Rectangle pageBounds)
        {
            int blockCount = -1;

            IntPtr nativeStructuredPage = IntPtr.Zero;

            ExitCodes result;

            if (ocrLanguage != null)
            {
                result = (ExitCodes)NativeMethods.GetStructuredTextPageWithOCR(context.NativeContext, list.NativeDisplayList, ref nativeStructuredPage, ref blockCount, (float)zoom, pageBounds.X0, pageBounds.Y0, pageBounds.X1, pageBounds.Y1, "TESSDATA_PREFIX=" + ocrLanguage.Prefix, ocrLanguage.Language);
            }
            else
            {
                result = (ExitCodes)NativeMethods.GetStructuredTextPage(context.NativeContext, list.NativeDisplayList, ref nativeStructuredPage, ref blockCount);
            }


            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_CREATE_PAGE:
                    throw new MuPDFException("Cannot create page", result);
                case ExitCodes.ERR_CANNOT_POPULATE_PAGE:
                    throw new MuPDFException("Cannot populate page", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            IntPtr[] blockPointers = new IntPtr[blockCount];
            GCHandle blocksHandle = GCHandle.Alloc(blockPointers, GCHandleType.Pinned);

            try
            {
                result = (ExitCodes)NativeMethods.GetStructuredTextBlocks(nativeStructuredPage, blocksHandle.AddrOfPinnedObject());

                switch (result)
                {
                    case ExitCodes.EXIT_SUCCESS:
                        break;
                    default:
                        throw new MuPDFException("Unknown error", result);
                }

                StructuredTextBlocks = new MuPDFStructuredTextBlock[blockCount];

                for (int i = 0; i < blockCount; i++)
                {
                    int type = -1;
                    float x0 = -1;
                    float y0 = -1;
                    float x1 = -1;
                    float y1 = -1;
                    int lineCount = -1;

                    result = (ExitCodes)NativeMethods.GetStructuredTextBlock(blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount);

                    switch (result)
                    {
                        case ExitCodes.EXIT_SUCCESS:
                            break;
                        default:
                            throw new MuPDFException("Unknown error", result);
                    }

                    Rectangle bBox = new Rectangle(x0, y0, x1, y1);

                    switch ((MuPDFStructuredTextBlock.Types)type)
                    {
                        case MuPDFStructuredTextBlock.Types.Image:
                            StructuredTextBlocks[i] = new MuPDFImageStructuredTextBlock(bBox);
                            break;
                        case MuPDFStructuredTextBlock.Types.Text:
                            StructuredTextBlocks[i] = new MuPDFTextStructuredTextBlock(bBox, blockPointers[i], lineCount);
                            break;
                    }
                }
            }
            finally
            {
                blocksHandle.Free();
            }

            NativeMethods.DisposeStructuredTextPage(context.NativeContext, nativeStructuredPage);
        }

        /// <summary>
        /// Gets the address of the character that contains the specified <paramref name="point"/> in page units.
        /// </summary>
        /// <param name="point">The point that must be contained by the character. This is expressed in page units (i.e. with a zoom factor of 1).</param>
        /// <param name="includeImages">If this is <see langword="true"/>, blocks containing images may be returned. Otherwise, only blocks containing text are considered.</param>
        /// <returns>The address of the character containing the specified <paramref name="point"/>, or <see langword="null"/> if no character contains the <paramref name="point"/>.</returns>
        public MuPDFStructuredTextAddress? GetHitAddress(PointF point, bool includeImages)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (includeImages || this[i].Type == MuPDFStructuredTextBlock.Types.Text)
                {
                    if (this[i].BoundingBox.Contains(point))
                    {
                        for (int j = 0; j < this[i].Count; j++)
                        {
                            if (this[i][j].BoundingBox.Contains(point))
                            {
                                for (int k = 0; k < this[i][j].Count; k++)
                                {
                                    if (this[i][j][k].BoundingQuad.Contains(point))
                                    {
                                        return new MuPDFStructuredTextAddress(i, j, k);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the address of the character that contains the specified <paramref name="point"/> in page units.
        /// </summary>
        /// <param name="point">The point that must be closest to the character. This is expressed in page units (i.e. with a zoom factor of 1).</param>
        /// <param name="includeImages">If this is <see langword="true"/>, blocks containing images may be returned. Otherwise, only blocks containing text are considered.</param>
        /// <returns>The address of the character closest to the specified <paramref name="point"/> This is <see langword="null"/> only if the page contains no characters.</returns>
        public MuPDFStructuredTextAddress? GetClosestHitAddress(PointF point, bool includeImages)
        {
            float minDistance = float.MaxValue;
            MuPDFStructuredTextAddress? closestHit = null;

            float minBlockDistance = float.MaxValue;
            float minLineDistance = float.MaxValue;

            for (int i = 0; i < this.Count; i++)
            {
                if (includeImages || this[i].Type == MuPDFStructuredTextBlock.Types.Text)
                {
                    float dx = Math.Max(0, Math.Max(this[i].BoundingBox.X0 - point.X, point.X - this[i].BoundingBox.X1));
                    float dy = Math.Max(0, Math.Max(this[i].BoundingBox.Y0 - point.Y, point.Y - this[i].BoundingBox.Y1));
                    float blockDist = dx * dx + dy * dy;

                    if (this[i].BoundingBox.Contains(point) || blockDist < minBlockDistance)
                    {
                        if (blockDist < minBlockDistance)
                        {
                            minBlockDistance = blockDist;
                            minLineDistance = float.MaxValue;
                        }

                        for (int j = 0; j < this[i].Count; j++)
                        {
                            dx = Math.Max(0, Math.Max(this[i][j].BoundingBox.X0 - point.X, point.X - this[i][j].BoundingBox.X1));
                            dy = Math.Max(0, Math.Max(this[i][j].BoundingBox.Y0 - point.Y, point.Y - this[i][j].BoundingBox.Y1));
                            float lineDist = dx * dx + dy * dy;

                            if (this[i][j].BoundingBox.Contains(point) || lineDist < minLineDistance)
                            {
                                if (lineDist < minLineDistance)
                                {
                                    minLineDistance = lineDist;
                                }

                                for (int k = 0; k < this[i][j].Count; k++)
                                {
                                    if (this[i][j][k].BoundingQuad.Contains(point))
                                    {
                                        return new MuPDFStructuredTextAddress(i, j, k);
                                    }
                                    else
                                    {
                                        //The quads should be small enough that the error due to only checking vertices and not sides is negligible. Also, since the square root is monotonous, we can skip it.
                                        float minDist = (point.X - this[i][j][k].BoundingQuad.UpperLeft.X) * (point.X - this[i][j][k].BoundingQuad.UpperLeft.X) + (point.Y - this[i][j][k].BoundingQuad.UpperLeft.Y) * (point.Y - this[i][j][k].BoundingQuad.UpperLeft.Y);
                                        minDist = Math.Min(minDist, (point.X - this[i][j][k].BoundingQuad.UpperRight.X) * (point.X - this[i][j][k].BoundingQuad.UpperRight.X) + (point.Y - this[i][j][k].BoundingQuad.UpperRight.Y) * (point.Y - this[i][j][k].BoundingQuad.UpperRight.Y));
                                        minDist = Math.Min(minDist, (point.X - this[i][j][k].BoundingQuad.LowerRight.X) * (point.X - this[i][j][k].BoundingQuad.LowerRight.X) + (point.Y - this[i][j][k].BoundingQuad.LowerRight.Y) * (point.Y - this[i][j][k].BoundingQuad.LowerRight.Y));
                                        minDist = Math.Min(minDist, (point.X - this[i][j][k].BoundingQuad.LowerLeft.X) * (point.X - this[i][j][k].BoundingQuad.LowerLeft.X) + (point.Y - this[i][j][k].BoundingQuad.LowerLeft.Y) * (point.Y - this[i][j][k].BoundingQuad.LowerLeft.Y));

                                        if (minDist < minDistance)
                                        {
                                            minDistance = minDist;
                                            closestHit = new MuPDFStructuredTextAddress(i, j, k);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return closestHit;
        }

        /// <summary>
        /// Gets a collection of <see cref="Quad"/>s delimiting the specified character <paramref name="range"/>. Where possible, these are collapsed at the line and block level. Each <see cref="Quad"/> may or may not be a rectangle.
        /// </summary>
        /// <param name="range">A <see cref="MuPDFStructuredTextAddressSpan"/> representing the character range</param>
        /// <param name="includeImages">If this is <see langword="true"/>, the bounding boxes for blocks containing images are also returned. Otherwise, only blocks containing text are considered.</param>
        /// <returns>A lazy collection of <see cref="Quad"/>s delimiting the characters in the specified <paramref name="includeImages"/>.</returns>
        public IEnumerable<Quad> GetHighlightQuads(MuPDFStructuredTextAddressSpan range, bool includeImages)
        {
            if (range == null || range.End == null)
            {
                yield break;
            }

            MuPDFStructuredTextAddress rangeStart = range.Start;
            MuPDFStructuredTextAddress rangeEnd = range.End.Value;

            if (rangeEnd < rangeStart)
            {
                MuPDFStructuredTextAddress temp = rangeStart;
                rangeStart = rangeEnd;
                rangeEnd = temp;
            }

            if (rangeStart.BlockIndex != rangeEnd.BlockIndex)
            {
                //Add remaining part of this block
                if (rangeStart.LineIndex == 0 && rangeStart.CharacterIndex == 0)
                {
                    if (includeImages || this[rangeStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Text)
                    {
                        yield return this[rangeStart.BlockIndex].BoundingBox.ToQuad();
                    }
                }
                else
                {
                    if (rangeStart.CharacterIndex == 0)
                    {
                        yield return this[rangeStart.BlockIndex][rangeStart.LineIndex].BoundingBox.ToQuad();
                    }
                    else
                    {
                        for (int i = rangeStart.CharacterIndex; i < this[rangeStart.BlockIndex][rangeStart.LineIndex].Count; i++)
                        {
                            yield return this[rangeStart.BlockIndex][rangeStart.LineIndex][i].BoundingQuad;
                        }
                    }

                    for (int j = rangeStart.LineIndex + 1; j < this[rangeStart.BlockIndex].Count; j++)
                    {
                        yield return this[rangeStart.BlockIndex][j].BoundingBox.ToQuad();
                    }
                }

                //Add full blocks in the middle
                for (int i = rangeStart.BlockIndex + 1; i < rangeEnd.BlockIndex; i++)
                {
                    if (includeImages || this[i].Type == MuPDFStructuredTextBlock.Types.Text)
                    {
                        yield return this[i].BoundingBox.ToQuad();
                    }
                }

                rangeStart = new MuPDFStructuredTextAddress(rangeEnd.BlockIndex, 0, 0);
            }

            if (includeImages || this[rangeStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Text)
            {
                if (rangeStart.LineIndex != rangeEnd.LineIndex)
                {
                    //Add remaining part of this line
                    if (rangeStart.CharacterIndex == 0)
                    {
                        yield return this[rangeStart.BlockIndex][rangeStart.LineIndex].BoundingBox.ToQuad();
                    }
                    else
                    {
                        for (int i = rangeStart.CharacterIndex; i < this[rangeStart.BlockIndex][rangeStart.LineIndex].Count; i++)
                        {
                            yield return this[rangeStart.BlockIndex][rangeStart.LineIndex][i].BoundingQuad;
                        }
                    }

                    //Add full lines in the middle
                    for (int j = rangeStart.LineIndex + 1; j < rangeEnd.LineIndex; j++)
                    {
                        yield return this[rangeStart.BlockIndex][j].BoundingBox.ToQuad();
                    }

                    rangeStart = new MuPDFStructuredTextAddress(rangeEnd.BlockIndex, rangeEnd.LineIndex, 0);
                }

                //Add remaining part of this line
                if (rangeStart.CharacterIndex == 0 && rangeEnd.CharacterIndex == this[rangeStart.BlockIndex][rangeStart.LineIndex].Count - 1)
                {
                    yield return this[rangeStart.BlockIndex][rangeStart.LineIndex].BoundingBox.ToQuad();
                }
                else
                {
                    for (int j = rangeStart.CharacterIndex; j <= rangeEnd.CharacterIndex; j++)
                    {
                        yield return this[rangeStart.BlockIndex][rangeStart.LineIndex][j].BoundingQuad;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the text corresponding to the specified character <paramref name="range"/>. Blocks containing images are ignored.
        /// </summary>
        /// <param name="range">A <see cref="MuPDFStructuredTextAddressSpan"/> representing the range of text to extract.</param>
        /// <returns>A string representation of the text contained in the specified <paramref name="range"/>.</returns>
        public string GetText(MuPDFStructuredTextAddressSpan range)
        {
            if (range == null || range.End == null)
            {
                return null;
            }

            MuPDFStructuredTextAddress selectionStart = range.Start;
            MuPDFStructuredTextAddress selectionEnd = range.End.Value;

            if (selectionEnd < selectionStart)
            {
                MuPDFStructuredTextAddress temp = selectionStart;
                selectionStart = selectionEnd;
                selectionEnd = temp;
            }

            StringBuilder builder = new StringBuilder();

            if (selectionStart.BlockIndex != selectionEnd.BlockIndex)
            {
                //Add remaining part of this block
                if (selectionStart.LineIndex == 0 && selectionStart.CharacterIndex == 0)
                {
                    if (this[selectionStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Text)
                    {
                        builder.Append(((MuPDFTextStructuredTextBlock)this[selectionStart.BlockIndex]).ToString());
                    }
                }
                else
                {
                    if (selectionStart.CharacterIndex == 0)
                    {
                        builder.AppendLine(this[selectionStart.BlockIndex][selectionStart.LineIndex].ToString());
                    }
                    else
                    {
                        for (int i = selectionStart.CharacterIndex; i < this[selectionStart.BlockIndex][selectionStart.LineIndex].Count; i++)
                        {
                            builder.Append(this[selectionStart.BlockIndex][selectionStart.LineIndex][i].ToString());
                        }
                        builder.AppendLine();
                    }

                    for (int j = selectionStart.LineIndex + 1; j < this[selectionStart.BlockIndex].Count; j++)
                    {
                        builder.AppendLine(this[selectionStart.BlockIndex][j].ToString());
                    }
                }

                //Add full blocks in the middle
                for (int i = selectionStart.BlockIndex + 1; i < selectionEnd.BlockIndex; i++)
                {
                    if (this[i].Type == MuPDFStructuredTextBlock.Types.Text)
                    {
                        builder.Append(this[i].ToString());
                    }
                }

                selectionStart = new MuPDFStructuredTextAddress(selectionEnd.BlockIndex, 0, 0);
            }

            if (this[selectionStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Text)
            {
                if (selectionStart.LineIndex != selectionEnd.LineIndex)
                {
                    //Add remaining part of this line
                    if (selectionStart.CharacterIndex == 0)
                    {
                        builder.AppendLine(this[selectionStart.BlockIndex][selectionStart.LineIndex].ToString());
                    }
                    else
                    {
                        for (int i = selectionStart.CharacterIndex; i < this[selectionStart.BlockIndex][selectionStart.LineIndex].Count; i++)
                        {
                            builder.Append(this[selectionStart.BlockIndex][selectionStart.LineIndex][i].ToString());
                        }
                        builder.AppendLine();
                    }

                    //Add full lines in the middle
                    for (int j = selectionStart.LineIndex + 1; j < selectionEnd.LineIndex; j++)
                    {
                        builder.AppendLine(this[selectionStart.BlockIndex][j].ToString());
                    }

                    selectionStart = new MuPDFStructuredTextAddress(selectionEnd.BlockIndex, selectionEnd.LineIndex, 0);
                }

                //Add remaining part of this line
                if (selectionStart.CharacterIndex == 0 && selectionEnd.CharacterIndex == this[selectionStart.BlockIndex][selectionStart.LineIndex].Count - 1)
                {
                    builder.Append(this[selectionStart.BlockIndex][selectionStart.LineIndex].ToString());
                }
                else
                {
                    for (int j = selectionStart.CharacterIndex; j <= selectionEnd.CharacterIndex; j++)
                    {
                        builder.Append(this[selectionStart.BlockIndex][selectionStart.LineIndex][j].ToString());
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Searches for the specified <see cref="Regex"/> in the text of the page. A single match cannot span multiple lines.
        /// </summary>
        /// <param name="needle">The <see cref="Regex"/> to search for.</param>
        /// <returns>A lazy collection of <see cref="MuPDFStructuredTextAddressSpan"/>s representing all the occurrences of the <paramref name="needle"/> in the text.</returns>
        public IEnumerable<MuPDFStructuredTextAddressSpan> Search(Regex needle)
        {

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Type == MuPDFStructuredTextBlock.Types.Text)
                {
                    for (int j = 0; j < this[i].Count; j++)
                    {
                        foreach (Match match in needle.Matches(this[i][j].Text))
                        {
                            if (match.Success)
                            {
                                int startIndex = 0;
                                int kStart = 0;

                                while (startIndex < match.Index)
                                {
                                    startIndex += this[i][j][kStart].Character.Length;
                                    kStart++;
                                }

                                if (startIndex > match.Index)
                                {
                                    kStart--;
                                }

                                int length = 0;
                                int kEnd = kStart - 1;

                                while (length < match.Length)
                                {
                                    kEnd++;
                                    length += this[i][j][kEnd].Character.Length;
                                }

                                yield return new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(i, j, kStart), new MuPDFStructuredTextAddress(i, j, kEnd));
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerator<MuPDFStructuredTextBlock> GetEnumerator()
        {
            return ((IEnumerable<MuPDFStructuredTextBlock>)StructuredTextBlocks).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return StructuredTextBlocks.GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a structured text block containing text or an image.
    /// </summary>
    public abstract class MuPDFStructuredTextBlock : IReadOnlyList<MuPDFStructuredTextLine>
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
            Image = 1
        }

        /// <summary>
        /// The type of the block.
        /// </summary>
        public abstract Types Type { get; }

        /// <summary>
        /// The bounding box of the block.
        /// </summary>
        public Rectangle BoundingBox { get; }

        /// <summary>
        /// The number of lines in the block.
        /// </summary>
        public abstract int Count { get; }

        /// <summary>
        /// Gets the specified line from the block.
        /// </summary>
        /// <param name="index">The index of the line to extract.</param>
        /// <returns>The <see cref="MuPDFStructuredTextLine"/> with the specified <paramref name="index"/>.</returns>
        public abstract MuPDFStructuredTextLine this[int index] { get; }

        internal MuPDFStructuredTextBlock() { }

        internal MuPDFStructuredTextBlock(Rectangle boundingBox)
        {
            this.BoundingBox = boundingBox;
        }

        /// <inheritdoc/>
        public abstract IEnumerator<MuPDFStructuredTextLine> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a block containing a single image. The block contains a single line with a single character.
    /// </summary>
    public class MuPDFImageStructuredTextBlock : MuPDFStructuredTextBlock
    {
        /// <inheritdoc/>
        public override Types Type => Types.Image;

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
                    throw new IndexOutOfRangeException("A structured text block containing an image only has one line!");
                }
            }
        }

        internal MuPDFImageStructuredTextBlock(Rectangle boundingBox) : base(boundingBox)
        {
            this.Line = new MuPDFStructuredTextLine(this.BoundingBox);
        }

        /// <inheritdoc/>
        public override IEnumerator<MuPDFStructuredTextLine> GetEnumerator()
        {
            return ((IEnumerable<MuPDFStructuredTextLine>)new MuPDFStructuredTextLine[] { Line }).GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a block containing multiple lines of text (typically a paragraph).
    /// </summary>
    public class MuPDFTextStructuredTextBlock : MuPDFStructuredTextBlock
    {
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

        internal MuPDFTextStructuredTextBlock(Rectangle boundingBox, IntPtr blockPointer, int lineCount) : base(boundingBox)
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

                    Lines[i] = new MuPDFStructuredTextLine(linePointers[i], (MuPDFStructuredTextLine.WritingModes)wmode, direction, bBox, charCount);
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
    }

    /// <summary>
    /// Represents a single line of text (i.e. characters that share a common baseline).
    /// </summary>
    public class MuPDFStructuredTextLine : IReadOnlyList<MuPDFStructuredTextCharacter>
    {
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

        internal MuPDFStructuredTextLine(Rectangle boundingBox)
        {
            this.BoundingBox = boundingBox;
            this.Characters = new MuPDFStructuredTextCharacter[]
            {
                new MuPDFStructuredTextCharacter(0, -1, new PointF(boundingBox.X0, boundingBox.Y1), new Quad(new PointF(boundingBox.X0, boundingBox.Y1), new PointF(boundingBox.X0, boundingBox.Y0), new PointF(boundingBox.X1, boundingBox.Y0), new PointF(boundingBox.X1, boundingBox.Y1)), 9)
            };
        }

        internal MuPDFStructuredTextLine(IntPtr linePointer, WritingModes writingMode, PointF direction, Rectangle boundingBox, int charCount)
        {
            this.WritingMode = writingMode;
            this.Direction = direction;
            this.BoundingBox = boundingBox;

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

                    result = (ExitCodes)NativeMethods.GetStructuredTextChar(charPointers[i], ref c, ref color, ref originX, ref originY, ref size, ref llX, ref llY, ref ulX, ref ulY, ref urX, ref urY, ref lrX, ref lrY);

                    switch (result)
                    {
                        case ExitCodes.EXIT_SUCCESS:
                            break;
                        default:
                            throw new MuPDFException("Unknown error", result);
                    }

                    Quad quad = new Quad(new PointF(llX, llY), new PointF(ulX, ulY), new PointF(urX, urY), new PointF(lrX, lrY));
                    PointF origin = new PointF(originX, originY);

                    Characters[i] = new MuPDFStructuredTextCharacter(c, color, origin, quad, size);
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
    }

    /// <summary>
    /// Represents a single text character.
    /// </summary>
    public class MuPDFStructuredTextCharacter
    {
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

        internal MuPDFStructuredTextCharacter(int codePoint, int color, PointF origin, Quad boundingQuad, float size)
        {
            this.CodePoint = codePoint;
            this.Character = Char.ConvertFromUtf32(codePoint);
            this.Color = color;
            this.Origin = origin;
            this.BoundingQuad = boundingQuad;
            this.Size = size;
        }

        /// <summary>
        /// Returns a string representation of the character.
        /// </summary>
        /// <returns>A string representation of the character.</returns>
        public override string ToString()
        {
            return this.Character;
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
