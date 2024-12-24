﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MuPDFCore.StructuredText;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class MuPDFStructuredTextPageTests
    {
        [TestMethod]
        public void MuPDFStructuredTextAddressComparisons()
        {
            MuPDFStructuredTextAddress address1 = new MuPDFStructuredTextAddress(1, 2, 3);
            MuPDFStructuredTextAddress address2 = new MuPDFStructuredTextAddress(1, 2, 3);
            MuPDFStructuredTextAddress address3 = new MuPDFStructuredTextAddress(1, 2, 4);
            MuPDFStructuredTextAddress address4 = new MuPDFStructuredTextAddress(1, 4, 2);

            MuPDFStructuredTextAddress[][] couples = new MuPDFStructuredTextAddress[][]
            {
                new MuPDFStructuredTextAddress[] { address1, address1 },
                new MuPDFStructuredTextAddress[] { address1, address2 },
                new MuPDFStructuredTextAddress[] { address1, address3 },
                new MuPDFStructuredTextAddress[] { address1, address4 },
                new MuPDFStructuredTextAddress[] { address2, address1 },
                new MuPDFStructuredTextAddress[] { address2, address2 },
                new MuPDFStructuredTextAddress[] { address2, address3 },
                new MuPDFStructuredTextAddress[] { address2, address4 },
                new MuPDFStructuredTextAddress[] { address3, address1 },
                new MuPDFStructuredTextAddress[] { address3, address2 },
                new MuPDFStructuredTextAddress[] { address3, address3 },
                new MuPDFStructuredTextAddress[] { address3, address4 },
                new MuPDFStructuredTextAddress[] { address4, address1 },
                new MuPDFStructuredTextAddress[] { address4, address2 },
                new MuPDFStructuredTextAddress[] { address4, address3 },
                new MuPDFStructuredTextAddress[] { address4, address4 }
            };

            bool[] comparisonResultsGT = new bool[]
            {
                false, false, false, false,

                false, false, false, false,

                true, true, false, false,

                true, true, true, false
            };

            bool[] comparisonResultsGEQ = new bool[]
            {
                true, true, false, false,

                true, true, false, false,

                true, true, true, false,

                true, true, true, true
            };

            bool[] comparisonResultsLT = new bool[]
            {
                false, false, true, true,

                false, false, true, true,

                false, false, false, true,

                false, false, false, false
            };

            bool[] comparisonResultsLEQ = new bool[]
            {
                true, true, true, true,

                true, true, true, true,

                false, false, true, true,

                false, false, false, true
            };

            bool[] comparisonResultsEQ = new bool[]
            {
                true, true, false, false,

                true, true, false, false,

                false, false, true, false,

                false, false, false, true
            };


            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsGT[i])
                {
                    Assert.IsTrue(couples[i][0] > couples[i][1], "The address > comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(couples[i][0] > couples[i][1], "The address > comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsGEQ[i])
                {
                    Assert.IsTrue(couples[i][0] >= couples[i][1], "The address >= comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(couples[i][0] >= couples[i][1], "The address >= comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsLT[i])
                {
                    Assert.IsTrue(couples[i][0] < couples[i][1], "The address < comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(couples[i][0] < couples[i][1], "The address < comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsLEQ[i])
                {
                    Assert.IsTrue(couples[i][0] <= couples[i][1], "The address <= comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(couples[i][0] <= couples[i][1], "The address <= comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsEQ[i])
                {
                    Assert.IsTrue(couples[i][0] == couples[i][1], "The address == comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(couples[i][0] == couples[i][1], "The address == comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (!comparisonResultsEQ[i])
                {
                    Assert.IsTrue(couples[i][0] != couples[i][1], "The address != comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(couples[i][0] != couples[i][1], "The address != comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsEQ[i])
                {
                    Assert.IsTrue(couples[i][0].Equals(couples[i][1]), "The address Equals comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(couples[i][0].Equals(couples[i][1]), "The address Equals comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsEQ[i])
                {
                    Assert.IsTrue(object.Equals(couples[i][0], couples[i][1]), "The address object.Equals comparison returned a wrong result.");
                }
                else
                {
                    Assert.IsFalse(object.Equals(couples[i][0], couples[i][1]), "The address object.Equals comparison returned a wrong result.");
                }
            }

            for (int i = 0; i < couples.Length; i++)
            {
                if (comparisonResultsEQ[i])
                {
                    Assert.IsTrue(couples[i][0].GetHashCode() == couples[i][1].GetHashCode(), "The address GetHashCode is not consistent.");
                }
            }
        }

        [TestMethod]
        public void MuPDFStructuredTextAddressIncrement()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            MuPDFStructuredTextAddress address1 = new MuPDFStructuredTextAddress(1, 0, 3);
            MuPDFStructuredTextAddress address2 = new MuPDFStructuredTextAddress(20, 0, 16);
            MuPDFStructuredTextAddress address3 = new MuPDFStructuredTextAddress(4, 0, 8);
            MuPDFStructuredTextAddress address4 = new MuPDFStructuredTextAddress(sTextPage.Count - 1, sTextPage[^1].Count - 1, sTextPage[^1][^1].Count - 1);

            MuPDFStructuredTextAddress address1Iexp = new MuPDFStructuredTextAddress(1, 0, 4);
            MuPDFStructuredTextAddress address2Iexp = new MuPDFStructuredTextAddress(21, 0, 0);
            MuPDFStructuredTextAddress address3Iexp = new MuPDFStructuredTextAddress(5, 0, 0);

            MuPDFStructuredTextAddress? address1I = address1.Increment(sTextPage);
            MuPDFStructuredTextAddress? address2I = address2.Increment(sTextPage);
            MuPDFStructuredTextAddress? address3I = address3.Increment(sTextPage);
            MuPDFStructuredTextAddress? address4I = address4.Increment(sTextPage);

            Assert.IsNotNull(address1I, "The incremented address 1 is null.");
            Assert.IsNotNull(address2I, "The incremented address 2 is null.");
            Assert.IsNotNull(address3I, "The incremented address 3 is null.");
            Assert.IsNull(address4I, "The incremented address 4 is not null.");

            Assert.AreEqual(address1Iexp, address1I.Value, "Incrementing address 1 returned the wrong result.");
            Assert.AreEqual(address2Iexp, address2I.Value, "Incrementing address 2 returned the wrong result.");
            Assert.AreEqual(address3Iexp, address3I.Value, "Incrementing address 3 returned the wrong result.");
        }

        [TestMethod]
        public void MuPDFStructuredTextPageMembers()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            Assert.IsNotNull(sTextPage, "The structured text page is null.");
            Assert.IsNotNull(sTextPage.StructuredTextBlocks, "The structured text page contents are null.");
            Assert.IsTrue(sTextPage.Count > 10, "The structured text page contains too few blocks.");
            Assert.AreEqual(sTextPage.Count, sTextPage.StructuredTextBlocks.Length, "The structured text page contains the wrong number of blocks.");

            for (int i = 0; i < sTextPage.Count; i++)
            {
                Assert.IsNotNull(sTextPage[i], "The structured text page contains a null block.");
            }

            int index = 0;

            foreach (MuPDFStructuredTextBlock blk in sTextPage)
            {
                Assert.IsNotNull(blk, "The structured text page contains a null block.");
                index++;
            }

            Assert.AreEqual(sTextPage.Count, index, "The structured text page enumeration returned the wrong number of blocks.");
        }

        [TestMethod]
        public void MuPDFStructuredTextAddressSeek()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            MuPDFStructuredTextAddress? address = new MuPDFStructuredTextAddress(0, 0, 0);

            while (address != null)
            {
                MuPDFStructuredTextCharacter chr = sTextPage[address.Value];
                Assert.IsNotNull(chr, "The structured address pointed to a null character.");
                address = address.Value.Increment(sTextPage);
            }
        }

        [TestMethod]
        public void MuPDFStructuredTextHitAddressGetter()
        {
            PointF hitPoint = new PointF(400, 1180);

            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            MuPDFStructuredTextAddress? address = sTextPage.GetHitAddress(hitPoint, false);

            Assert.IsNotNull(address, "The hit test did not return a point.");

            using MuPDFStructuredTextCharacter chr = sTextPage[address.Value];

            Assert.IsTrue(chr.BoundingQuad.Contains(hitPoint), "The matched character does not contain the hit point.");
            Assert.AreEqual("v", chr.Character, "The hit test matched the wrong character.");
        }

        [TestMethod]
        public void MuPDFStructuredTextClosestHitAddressGetter()
        {
            PointF hitPoint = new PointF(400, 816);

            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            MuPDFStructuredTextAddress? address = sTextPage.GetClosestHitAddress(hitPoint, false);

            Assert.IsNotNull(address, "The hit test did not return a point.");

            using MuPDFStructuredTextCharacter chr = sTextPage[address.Value];

            Assert.IsFalse(chr.BoundingQuad.Contains(hitPoint), "The matched character contains the hit point.");
            Assert.IsTrue(chr.Character == "v" || chr.Character == "l", "The hit test matched the wrong character.");
        }

        [TestMethod]
        public void MuPDFStructuredTextHighlightQuadsGetter()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            IEnumerable<Quad> quads = sTextPage.GetHighlightQuads(new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(1, 0, 2), new MuPDFStructuredTextAddress(1, 0, 8)), true);
            Assert.AreEqual(7, quads.Count(), "The highlight quads are in the wrong number.");

            quads = sTextPage.GetHighlightQuads(new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(1, 0, 2), new MuPDFStructuredTextAddress(3, 0, 2)), true);
            Assert.AreEqual(11, quads.Count(), "The highlight quads are in the wrong number.");

            quads = sTextPage.GetHighlightQuads(new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(1, 0, 2), new MuPDFStructuredTextAddress(4, 0, 2)), true);
            Assert.AreEqual(12, quads.Count(), "The highlight quads are in the wrong number.");
        }

        [TestMethod]
        public void MuPDFStructuredTextTextGetter()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            string text = sTextPage.GetText(new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(1, 0, 2), new MuPDFStructuredTextAddress(1, 0, 8)));
            Assert.AreEqual("lvetica", text, "The extracted text is wrong.");

            text = sTextPage.GetText(new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(1, 0, 2), new MuPDFStructuredTextAddress(3, 0, 2)));
            Assert.AreEqual("lvetica\nHelvetica-Bold\nHelvetica-Oblique\nHel".Replace("\n", Environment.NewLine), text, "The extracted text is wrong.");

            text = sTextPage.GetText(new MuPDFStructuredTextAddressSpan(new MuPDFStructuredTextAddress(1, 0, 2), new MuPDFStructuredTextAddress(8, 0, 2)));
            Assert.AreEqual("lvetica\nHelvetica-Bold\nHelvetica-Oblique\nHelvetica-BoldOblique\nCourier\nCourier-Bold\nCourier-Oblique\nCourier-BoldOblique\nΣψμβολ\n✺❁❐❆✤❉■❇❂❁▼▲\nVec".Replace("\n", Environment.NewLine), text, "The extracted text is wrong.");
        }

        [TestMethod]
        public void MuPDFStructuredTextSearching()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            IEnumerable<MuPDFStructuredTextAddressSpan> result = sTextPage.Search(new System.Text.RegularExpressions.Regex("Helvetica"));
            Assert.AreEqual(4, result.Count(), "The search results are wrong.");

            result = sTextPage.Search(new System.Text.RegularExpressions.Regex("(?i)ti[cm]"));
            Assert.AreEqual(8, result.Count(), "The search results are wrong.");
        }

        [TestMethod]
        public void MuPDFStructuredTextBlockMembers()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            using MuPDFStructuredTextBlock block = sTextPage[20];

            Assert.IsNotNull(block, "The block is null.");
            Assert.AreEqual(MuPDFStructuredTextBlock.Types.Text, block.Type, "The block type is wrong.");
            Assert.IsTrue(block.BoundingBox.X0 >= 0, "The block's left coordinate is out of range.");
            Assert.IsTrue(block.BoundingBox.Y0 >= 0, "The block's top coordinate is out of range.");
            Assert.IsTrue(block.BoundingBox.X1 >= block.BoundingBox.X0, "The block's right coordinate is out of range.");
            Assert.IsTrue(block.BoundingBox.Y1 >= block.BoundingBox.Y0, "The block's bottom coordinate is out of range.");
            Assert.AreEqual(1, block.Count, "The number of lines in the block is wrong.");

            for (int i = 0; i < block.Count; i++)
            {
                Assert.IsNotNull(block[i], "Line " + i.ToString() + " is null.");
            }

            int index = 0;

            foreach (MuPDFStructuredTextLine line in block)
            {
                Assert.IsNotNull(line, "Line " + index.ToString() + " is null.");
                Assert.AreEqual(block[index], line, "Line " + index.ToString() + " differs between the accessor and the enumerator.");
                index++;
            }

            Assert.AreEqual(1, index, "The number of lines enumerated in the block is wrong.");
        }

        [TestMethod]
        public void MuPDFTextStructuredTextBlockToString()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            using MuPDFTextStructuredTextBlock block = (MuPDFTextStructuredTextBlock)sTextPage[20];

            Assert.IsNotNull(block.Lines, "The block's lines are null.");
            Assert.AreEqual(block.Count, block.Lines.Length, "The number of lines in the block does not correspond to the Count of the block.");
            Assert.IsTrue((block.ToString() == "5π/6\n".Replace("\n", Environment.NewLine)), "Expected: <" + "5π/6\n".Replace("\n", Environment.NewLine) + ">, Actual: <" + block.ToString() + ">. The block text is wrong.");
        }

        [TestMethod]
        public void MuPDFStructuredTextLineMembers()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            using MuPDFStructuredTextLine line = sTextPage[6][0];

            Assert.IsNotNull(line, "The line is null.");
            Assert.AreEqual(MuPDFStructuredTextLine.WritingModes.Horizontal, line.WritingMode, "The line's writing mode is wrong.");
            Assert.IsTrue(line.BoundingBox.X0 >= 0, "The line's left coordinate is out of range.");
            Assert.IsTrue(line.BoundingBox.Y0 >= 0, "The line's top coordinate is out of range.");
            Assert.IsTrue(line.BoundingBox.X1 >= line.BoundingBox.X0, "The line's right coordinate is out of range.");
            Assert.IsTrue(line.BoundingBox.Y1 >= line.BoundingBox.Y0, "The line's bottom coordinate is out of range.");
            Assert.IsFalse(float.IsNaN(line.Direction.X), "The x component of the direction of the line is NaN.");
            Assert.IsFalse(float.IsNaN(line.Direction.Y), "The y component of the directionof the line is NaN.");
            Assert.AreEqual(1, Math.Sqrt(line.Direction.X * line.Direction.X + line.Direction.Y * line.Direction.Y), 0.01, "The modulus of the direction of the line is not 1.");

            Assert.AreEqual(19, line.Count, "The number of characters in the line is wrong.");

            for (int i = 0; i < line.Count; i++)
            {
                Assert.IsNotNull(line[i], "Character " + i.ToString() + " is null.");
            }

            int index = 0;

            foreach (MuPDFStructuredTextCharacter chr in line)
            {
                Assert.IsNotNull(chr, "Character " + index.ToString() + " is null.");
                Assert.AreEqual(line[index], chr, "Character " + index.ToString() + " differs between the accessor and the enumerator.");
                index++;
            }

            Assert.AreEqual(19, index, "The number of characters enumerated in the line is wrong.");
        }

        [TestMethod]
        public void MuPDFStructuredTextLineText()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            using MuPDFStructuredTextLine line = sTextPage[6][0];

            Assert.IsNotNull(line.Characters, "The line's characters are null.");
            Assert.AreEqual(line.Count, line.Characters.Length, "The number of characters in the line does not correspond to the Count of the line.");
            Assert.AreEqual("Courier-BoldOblique", line.Text, "The line text is wrong.");
            Assert.AreEqual("Courier-BoldOblique", line.ToString(), "The line text is wrong.");
        }

        [TestMethod]
        public void MuPDFStructuredTextCharacterMembers()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            using MuPDFStructuredTextCharacter chr = sTextPage[3][0][8];

            Assert.IsNotNull(chr, "The character is null.");

            Assert.AreEqual(97, chr.CodePoint, "The character's code point is wrong.");
            Assert.AreEqual("a", chr.Character, "The character's string representation is wrong.");
            Assert.AreEqual("a", chr.ToString(), "The character's string representation is wrong.");
            Assert.AreEqual((uint)4290111005, chr.Color, "The character's color is wrong.");
            Assert.IsTrue(chr.Origin.X >= 0, "The left coordinate of the character's origin is out of range.");
            Assert.IsTrue(chr.Origin.Y >= 0, "The top coordinate of the character's origin is out of range.");


            Assert.IsTrue(chr.BoundingQuad.LowerLeft.X >= 0, "The character's lower left x coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.LowerLeft.Y >= 0, "The character's lower left y coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.UpperLeft.X >= 0, "The character's upper left x coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.UpperLeft.Y >= 0, "The character's upper left y coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.UpperRight.X >= 0, "The character's upper right x coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.UpperRight.Y >= 0, "The character's upper right y coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.LowerRight.X >= 0, "The character's lower right x coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.LowerRight.Y >= 0, "The character's lower right y coordinate is out of range.");
            Assert.IsTrue(chr.BoundingQuad.Contains(chr.Origin), "The character's bounding quad does not contain the character's origin.");
            Assert.IsFalse(float.IsNaN(chr.Size), "The character's size is NaN.");

            Assert.AreEqual(MuPDFStructuredTextCharacter.TextDirection.LeftToRight, chr.Direction, "The character's direction is wrong.");
            Assert.IsNotNull(chr.Font, "The character's font is null.");
        }

        [TestMethod]
        public void MuPDFImageStructuredTextPageWithoutImages()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            Assert.AreEqual(2, sTextPage.Count, "The structured text page contains an unexpected block!");
        }

        [TestMethod]
        public void MuPDFImageStructuredTextBlockMembers()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            Assert.IsInstanceOfType(sTextPage[0], typeof(MuPDFImageStructuredTextBlock), "The block does not contain an image!");

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            Assert.IsNotNull(imageBlock.Image, "The image is null!");
            Assert.IsNotNull(imageBlock.TransformMatrix, "The image transform matrix is null!");
            Assert.AreEqual(3, imageBlock.TransformMatrix.GetLength(0), "The image transform matrix is not valid (L0)!");
            Assert.AreEqual(3, imageBlock.TransformMatrix.GetLength(1), "The image transform matrix is not valid (L1)!");
            Assert.AreEqual(0, imageBlock.TransformMatrix[0, 2], "The image transform matrix is not valid (02)!");
            Assert.AreEqual(0, imageBlock.TransformMatrix[1, 2], "The image transform matrix is not valid (12)!");
            Assert.AreEqual(1, imageBlock.TransformMatrix[2, 2], "The image transform matrix is not valid (22)!");

            static (float x, float y) transform(float x, float y, float[,] matrix)
            {
                return (x * matrix[0, 0] + y * matrix[1, 0] + matrix[2, 0], x * matrix[0, 1] + y * matrix[1, 1] + matrix[2, 1]);
            }

            (float x, float y) p1 = transform(0, 0, imageBlock.TransformMatrix);
            (float x, float y) p2 = transform(0, 1, imageBlock.TransformMatrix);
            (float x, float y) p3 = transform(1, 0, imageBlock.TransformMatrix);
            (float x, float y) p4 = transform(1, 1, imageBlock.TransformMatrix);

            float minX = Math.Min(Math.Min(p1.x, p2.x), Math.Min(p3.x, p4.x));
            float minY = Math.Min(Math.Min(p1.y, p2.y), Math.Min(p3.y, p4.y));
            float maxX = Math.Max(Math.Max(p1.x, p2.x), Math.Max(p3.x, p4.x));
            float maxY = Math.Max(Math.Max(p1.y, p2.y), Math.Max(p3.y, p4.y));

            Assert.AreEqual(minX, imageBlock.BoundingBox.X0, 1e-5F, "The image transform matrix and the bounding box are not consistent (X0)!");
            Assert.AreEqual(minY, imageBlock.BoundingBox.Y0, 1e-5F, "The image transform matrix and the bounding box are not consistent (Y0)!");
            Assert.AreEqual(maxX, imageBlock.BoundingBox.X1, 1e-5F, "The image transform matrix and the bounding box are not consistent (X1)!");
            Assert.AreEqual(maxY, imageBlock.BoundingBox.Y1, 1e-5F, "The image transform matrix and the bounding box are not consistent (Y1)!");
        }

    }
}
