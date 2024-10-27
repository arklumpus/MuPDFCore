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

namespace MuPDFCore.StructuredText
{
    /// <summary>
    /// Types of structural text block.
    /// </summary>
    public enum StructureType
    {
        /// <summary>
        /// Invalid structure element.
        /// </summary>
        Invalid = -1,

        /// <summary>
        /// A complete document.
        /// </summary>
        Document,

        /// <summary>
        /// A large-scale division of a document.
        /// </summary>
        Part,

        /// <summary>
        /// A self-contained body of text.
        /// </summary>
        Article,

        /// <summary>
        /// A container for related content elements.
        /// </summary>
        Section,

        /// <summary>
        /// A generic element or block of elements.
        /// </summary>
        Division,

        /// <summary>
        /// Text attributed to someone other than the author of the surrounding text.
        /// </summary>
        BlockQuotation,

        /// <summary>
        /// Text describing a table or figure.
        /// </summary>
        Caption,

        /// <summary>
        /// A list made of up of <see cref="TableOfContentsItem"/>s or other nested <see cref="TableOfContents"/>.
        /// </summary>
        TableOfContents,

        /// <summary>
        /// An individual member of a <see cref="TableOfContents"/>.
        /// </summary>
        TableOfContentsItem,

        /// <summary>
        /// A sequence of entries containing identifying text and <see cref="Reference"/> elements.
        /// </summary>
        Index,

        /// <summary>
        /// A grouping that has no structural significance.
        /// </summary>
        NonStructuralElement,

        /// <summary>
        /// A grouping of content that belongs to the program used to create the document and should not be interpreted or exported to other document formats.
        /// </summary>
        PrivateElement,

        /// <summary>
        /// A logical document fragment.
        /// </summary>
        DocumentFragment,
        
        /// <summary>
        /// Content that is distinct from other content in the parent structure element.
        /// </summary>
        Aside,
        
        /// <summary>
        /// Title of the document or division of content.
        /// </summary>
        Title,

        /// <summary>
        /// A footnote or an endnote.
        /// </summary>
        FootOrEndNote,
        
        /// <summary>
        /// A sub-division of content.
        /// </summary>
        Subdivision,

        /// <summary>
        /// A paragraph.
        /// </summary>
        Paragraph,

        /// <summary>
        /// A heading.
        /// </summary>
        Heading,

        /// <summary>
        /// A level 1 heading.
        /// </summary>
        H1,

        /// <summary>
        /// A level 2 heading.
        /// </summary>
        H2,

        /// <summary>
        /// A level 3 heading.
        /// </summary>
        H3,

        /// <summary>
        /// A level 4 heading.
        /// </summary>
        H4,

        /// <summary>
        /// A level 5 heading.
        /// </summary>
        H5,

        /// <summary>
        /// A level 6 heading.
        /// </summary>
        H6,

        /// <summary>
        /// A sequence of items.
        /// </summary>
        List,

        /// <summary>
        /// A <see cref="List" /> item.
        /// </summary>
        ListItem,

        /// <summary>
        /// A label for a <see cref="ListItem"/>.
        /// </summary>
        Label,

        /// <summary>
        /// The description of a <see cref="ListItem"/>.
        /// </summary>
        ListBody,

        /// <summary>
        /// A table.
        /// </summary>
        Table,

        /// <summary>
        /// A table row.
        /// </summary>
        TableRow,

        /// <summary>
        /// A table cell containing a header.
        /// </summary>
        TableHeaderCell,

        /// <summary>
        /// A table cell containing data.
        /// </summary>
        TableDataCell,

        /// <summary>
        /// A group of rows that constitutes the table's header.
        /// </summary>
        TableHeaderRowGroup,
        
        /// <summary>
        /// A group of rows that constitutes the table's body.
        /// </summary>
        TableBodyRowGroup,

        /// <summary>
        /// A group of rows that constitutes the table's footer.
        /// </summary>
        TableFooterRowGroup,

        /// <summary>
        /// A portion of text.
        /// </summary>
        Span,

        /// <summary>
        /// A portion of text attributed to someone other than the author of the surrounding text. 
        /// </summary>
        Quote,

        /// <summary>
        /// An item of explanatory text.
        /// </summary>
        Note,

        /// <summary>
        /// A citation to content elsewhere in the document.
        /// </summary>
        Reference,

        /// <summary>
        /// A reference identifying the external source of cited content.
        /// </summary>
        BibEntry,

        /// <summary>
        /// A fragment of computer code.
        /// </summary>
        Code,

        /// <summary>
        /// An association between a structure element and a link annotation.
        /// </summary>
        Link,

        /// <summary>
        /// An association between a structure element and an annotation.
        /// </summary>
        Annot,
        
        /// <summary>
        /// Emphasised content.
        /// </summary>
        Emphasis,

        /// <summary>
        /// Content of strong importance.
        /// </summary>
        Strong,

        /// <summary>
        /// A side note written in smaller text size adjacent to the text to which it refers.
        /// </summary>
        Ruby,

        /// <summary>
        /// The text to which the ruby annotation is applied.
        /// </summary>
        RubyBaseText,

        /// <summary>
        /// The smaller-size text that is placed adjacent to the base text.
        /// </summary>
        RubyAnnotationText,

        /// <summary>
        /// Punctuation surrounding the ruby annotation text.
        /// </summary>
        RubyPunctuation,

        /// <summary>
        /// A comment or annotation written in smaller text size on two lines following the text to which it refers.
        /// </summary>
        Warichu,

        /// <summary>
        /// The text of the warichu element.
        /// </summary>
        WarichuText,

        /// <summary>
        /// The punctuation that surrounds the warichu text.
        /// </summary>
        WP,

        /// <summary>
        /// A figure.
        /// </summary>
        Figure,

        /// <summary>
        /// A mathematical formula.
        /// </summary>
        Formula,

        /// <summary>
        /// An interactive form field.
        /// </summary>
        Form,

        /// <summary>
        /// Content that needs to be referenced in the structure tree even though it is not part of the document's real content.
        /// </summary>
        Artifact
    }

    /// <summary>
    /// Represents a structural element.
    /// </summary>
    public class MuPDFStructureStructuredTextBlock : MuPDFStructuredTextBlock
    {
        /// <summary>
        /// The parent <see cref="MuPDFStructureStructuredTextBlock"/>, or <see langword="null"/> for elements directly contained within a <see cref="MuPDFStructuredTextPage"/>.
        /// </summary>
        public MuPDFStructureStructuredTextBlock Parent { get; }

        /// <summary>
        /// The type of structural element.
        /// </summary>
        public StructureType StructureType { get; }

        /// <summary>
        /// The raw type of the structural element.
        /// </summary>
        public string RawStructure { get; }

        /// <summary>
        /// <see cref="MuPDFStructuredTextBlock"/> descending from this element.
        /// </summary>
        public MuPDFStructuredTextBlock[] Children { get; }
        
        /// <summary>
        /// The index of this node within this level of the tree.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc/>
        public override Types Type => Types.Structure;

        private MuPDFStructuredTextLine[] EnumeratedLines = null;

        /// <summary>
        /// The total number of lines within all descendant blocks of this block. Note that accessing this property for the first time will cause all descendants to be enumerated.
        /// </summary>
        public override int Count
        {
            get
            {
                if (this.EnumeratedLines == null)
                {
                    this.Enumerate();
                }

                return this.EnumeratedLines.Length;
            }
        }

        /// <summary>
        /// Gets the specified line from the block. Note that using the indexer for the first time will cause all descendants to be enumerated. If you wish to avoid this, use a <see langword="foreach"/> loop instead.
        /// </summary>
        /// <param name="index">The index of the line to extract.</param>
        /// <returns>The <see cref="MuPDFStructuredTextLine"/> with the specified <paramref name="index"/>.</returns>
        public override MuPDFStructuredTextLine this[int index]
        {
            get
            {
                if (this.EnumeratedLines == null)
                {
                    this.Enumerate();
                }

                return this.EnumeratedLines[index];
            }
        }

        internal unsafe MuPDFStructureStructuredTextBlock(MuPDFContext context, MuPDFStructuredTextPage parentPage, Rectangle boundingBox, IntPtr down, int index, MuPDFStructureStructuredTextBlock parentBlock) : base(boundingBox, parentPage)
        {
            this.Index = index;

            int countChildren = NativeMethods.CountStructStructuredTextBlockChildren(down);

            IntPtr[] blockPointers = new IntPtr[countChildren];
            int rawLength = 0;
            int standard = 0;
            IntPtr parent = IntPtr.Zero;

            ExitCodes result;

            fixed (IntPtr* blockPointersPtr = blockPointers)
            {
                result = (ExitCodes)NativeMethods.GetStructStructuredTextBlock(down, ref rawLength, ref standard, ref parent, (IntPtr)blockPointersPtr);
            }

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            this.Parent = parentBlock;

            string raw = null;

            if (rawLength > 0)
            {
                byte[] rawBytes = new byte[rawLength];

                fixed (byte* rawBytesPtr = rawBytes)
                {
                    NativeMethods.GetStructStructuredTextBlockRawStructure(down, rawLength, (IntPtr)rawBytesPtr);
                }

                raw = Encoding.UTF8.GetString(rawBytes);
            }

            this.RawStructure = raw;
            this.StructureType = (StructureType)standard;

            MuPDFStructuredTextBlock[] children = new MuPDFStructuredTextBlock[countChildren];

            Rectangle actualBoundingBox = new Rectangle();

            for (int i = 0; i < countChildren; i++)
            {
                children[i] = MuPDFStructuredTextBlock.Create(context, blockPointers[i], parentPage, this);

                if (i == 0)
                {
                    actualBoundingBox = children[i].BoundingBox;
                }
                else
                {
                    actualBoundingBox = new Rectangle(Math.Min(actualBoundingBox.X0, children[i].BoundingBox.X0), Math.Min(actualBoundingBox.Y0, children[i].BoundingBox.Y0), Math.Max(actualBoundingBox.X1, children[i].BoundingBox.X1), Math.Max(actualBoundingBox.Y1, children[i].BoundingBox.Y1));
                }
            }

            this.BoundingBox = actualBoundingBox;

            this.Children = children;
        }

        /// <inheritdoc/>
        public override IEnumerator<MuPDFStructuredTextLine> GetEnumerator()
        {
            if (this.EnumeratedLines != null)
            {
                foreach (MuPDFStructuredTextLine line in this.EnumeratedLines)
                {
                    yield return line;
                }
            }
            else
            {
                foreach (MuPDFStructuredTextBlock child in this.Children)
                {
                    foreach (MuPDFStructuredTextLine line in child)
                    {
                        yield return line;
                    }
                }
            }
        }

        private void Enumerate()
        {
            List<MuPDFStructuredTextLine> lines = new List<MuPDFStructuredTextLine>();

            foreach (MuPDFStructuredTextBlock child in this.Children)
            {
                foreach (MuPDFStructuredTextLine line in child)
                {
                    lines.Add(line);
                }
            }

            this.EnumeratedLines = lines.ToArray();
        }

        /// <inheritdoc/>
        public override void Dispose() { }
    }
}
