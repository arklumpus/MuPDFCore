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
using System.Text.RegularExpressions;
using System.Threading;

namespace MuPDFCore.StructuredText
{
    /// <summary>
    /// Flags for structured text extraction.
    /// </summary>
    public enum StructuredTextFlags
    {
        /// <summary>
        /// Use default settings.
        /// </summary>
        None = 0,

        /// <summary>
        /// Preserve ligatures instead of expanding them into their constituent characters.
        /// </summary>
        PreserveLigatures = 1,

        /// <summary>
        /// Preserve all whitespace characters as they are specified in the document, rather than converting them to spaces.
        /// </summary>
        PreserveWhitespace = 2,

        /// <summary>
        /// Collect image blocks.
        /// </summary>
        PreserveImages = 4,

        /// <summary>
        /// Do not add additional space characters when there are large gaps between characters.
        /// </summary>
        InhibitSpaces = 8,

        /// <summary>
        /// Remove hyphens at the end of a line and merge the lines.
        /// </summary>
        Dehyphenate = 16,

        /// <summary>
        /// Do not merge spans of different styles that are located on the same line.
        /// </summary>
        PreserveSpans = 32,

        /// <summary>
        /// Ignore characters entirely outside of the page's mediabox.
        /// </summary>
        MediaboxClip = 64,

        /// <summary>
        /// Use the character's CID for unknown Unicode characters.
        /// </summary>
        UseCIDForUnknownUnicode = 128,

        /// <summary>
        /// Collect a tree of structured text blocks rather than a simple list.
        /// </summary>
        CollectStructure = 256,

        /// <summary>
        /// Collect accurate bounding boxes for each glyph.
        /// </summary>
        AccurateBoundingBoxes = 512,
        
        /// <summary>
        /// Collect information about vector graphics.
        /// </summary>
        CollectVectors = 1024,

        /// <summary>
        /// Do not replace text with the ActualText specified in the document.
        /// </summary>
        IgnoreActualText = 2048,

        /// <summary>
        /// Segment the page into different regions (unless there is structure information present).
        /// </summary>
        Segment = 4096
    }

    /// <summary>
    /// Describes OCR progress.
    /// </summary>
    public class OCRProgressInfo
    {
        /// <summary>
        /// A value between 0 and 1, indicating how much progress has been completed.
        /// </summary>
        public double Progress { get; }

        internal OCRProgressInfo(double progress)
        {
            this.Progress = progress;
        }
    }

    /// <summary>
    /// Represents a structured representation of the text contained in a page.
    /// </summary>
    public class MuPDFStructuredTextPage : IReadOnlyList<MuPDFStructuredTextBlock>, IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// The blocks contained in the page.
        /// </summary>
        public MuPDFStructuredTextBlock[] StructuredTextBlocks { get; private set; }

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

        private MuPDFContext OwnerContext { get; }
        private IntPtr NativePointer { get; }

        internal unsafe MuPDFStructuredTextPage(MuPDFContext context, MuPDFDisplayList list, TesseractLanguage ocrLanguage, double zoom, Rectangle pageBounds, StructuredTextFlags flags, CancellationToken cancellationToken = default, IProgress<OCRProgressInfo> progress = null)
        {
            if (ocrLanguage != null && RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86 && (cancellationToken != default || progress != null))
            {
                throw new PlatformNotSupportedException("A cancellationToken or a progress callback are not supported on Windows x86!");
            }

            int blockCount = -1;

            IntPtr nativeStructuredPage = IntPtr.Zero;

            ExitCodes result;

            if (ocrLanguage != null)
            {
                result = (ExitCodes)NativeMethods.GetStructuredTextPageWithOCR(context.NativeContext, list.NativeDisplayList, (int)flags, ref nativeStructuredPage, ref blockCount, (float)zoom, pageBounds.X0, pageBounds.Y0, pageBounds.X1, pageBounds.Y1, "TESSDATA_PREFIX=" + ocrLanguage.Prefix, ocrLanguage.Language, prog =>
                {
                    progress?.Report(new OCRProgressInfo(prog / 100.0));

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                });
            }
            else
            {
                result = (ExitCodes)NativeMethods.GetStructuredTextPage(context.NativeContext, list.NativeDisplayList, (int)flags, ref nativeStructuredPage, ref blockCount);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                this.disposedValue = true;
            }

            cancellationToken.ThrowIfCancellationRequested();

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

            fixed (IntPtr* blockPointersPtr = blockPointers)
            {
                result = (ExitCodes)NativeMethods.GetStructuredTextBlocks(nativeStructuredPage, (IntPtr)blockPointersPtr);
            }

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
                this.StructuredTextBlocks[i] = MuPDFStructuredTextBlock.Create(context, blockPointers[i], this, null);
            }

            this.OwnerContext = context;
            this.NativePointer = nativeStructuredPage;
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
                if (includeImages || this[i].Type == MuPDFStructuredTextBlock.Types.Text || this[i].Type == MuPDFStructuredTextBlock.Types.Structure)
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
                if (includeImages || this[i].Type == MuPDFStructuredTextBlock.Types.Text || this[i].Type == MuPDFStructuredTextBlock.Types.Structure)
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
                    if (includeImages || this[rangeStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Text || this[rangeStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Structure)
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
                    if (includeImages || this[i].Type == MuPDFStructuredTextBlock.Types.Text || this[i].Type == MuPDFStructuredTextBlock.Types.Structure)
                    {
                        yield return this[i].BoundingBox.ToQuad();
                    }
                }

                rangeStart = new MuPDFStructuredTextAddress(rangeEnd.BlockIndex, 0, 0);
            }

            if (includeImages || this[rangeStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Text || this[rangeStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Structure)
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
                    else if (this[selectionStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Structure)
                    {
                        builder.Append(((MuPDFStructureStructuredTextBlock)this[selectionStart.BlockIndex]).ToString());
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
                    if (this[i].Type == MuPDFStructuredTextBlock.Types.Text || this[i].Type == MuPDFStructuredTextBlock.Types.Structure)
                    {
                        builder.Append(this[i].ToString());
                    }
                }

                selectionStart = new MuPDFStructuredTextAddress(selectionEnd.BlockIndex, 0, 0);
            }

            if (this[selectionStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Text || this[selectionStart.BlockIndex].Type == MuPDFStructuredTextBlock.Types.Structure)
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
                if (this[i].Type == MuPDFStructuredTextBlock.Types.Text || this[i].Type == MuPDFStructuredTextBlock.Types.Structure)
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

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (this.StructuredTextBlocks != null)
                    {
                        for (int i = 0; i < this.StructuredTextBlocks.Length; i++)
                        {
                            this.StructuredTextBlocks?[i]?.Dispose();
                        }
                    }
                }

                if ((OwnerContext == null && this.NativePointer != IntPtr.Zero) || OwnerContext?.disposedValue == true)
                {
                    throw new LifetimeManagementException<MuPDFStructuredTextPage, MuPDFContext>(this, OwnerContext, this.NativePointer, OwnerContext.NativeContext);
                }

                if (OwnerContext != null && this.NativePointer != IntPtr.Zero)
                {
                    NativeMethods.DisposeStructuredTextPage(this.OwnerContext.NativeContext, this.NativePointer);
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Dispose the <see cref="MuPDFStructuredTextPage"/>.
        /// </summary>
        ~MuPDFStructuredTextPage()
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
