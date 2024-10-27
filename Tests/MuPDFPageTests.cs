﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.IO;
using System.Linq;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class MuPDFPageTests
    {
        [TestMethod]
        public void MuPDFPagePageFields()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);
            using MuPDFPage page = document.Pages[0];

            Assert.IsNotNull(page, "The page is null.");
            Assert.AreNotEqual(IntPtr.Zero, page.NativePage, "The native page pointer is null.");
            Assert.AreEqual(context, typeof(MuPDFPage).GetField("OwnerContext", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(page), "The owner context is wrong.");
            Assert.AreEqual(document, page.OwnerDocument, "The owner document is wrong.");
            Assert.AreEqual(4000, page.OriginalBounds.Width, "The page's original width is wrong.");
            Assert.AreEqual(2600, page.OriginalBounds.Height, "The page's original height is wrong.");
            Assert.AreEqual(0, page.OriginalBounds.X0, "The page's original X0 is wrong.");
            Assert.AreEqual(0, page.OriginalBounds.Y0, "The page's original Y0 is wrong.");
            Assert.AreEqual(4000, page.OriginalBounds.X1, "The page's original X1 is wrong.");
            Assert.AreEqual(2600, page.OriginalBounds.Y1, "The page's original Y1 is wrong.");
        }

        [TestMethod]
        public void MuPDFPageBoundsPDF()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);
            using MuPDFPage page = document.Pages[0];

            Assert.AreEqual(4000, page.Bounds.Width, "The page width is wrong.");
            Assert.AreEqual(2600, page.Bounds.Height, "The page height is wrong.");
            Assert.AreEqual(0, page.Bounds.X0, "The page X0 is wrong.");
            Assert.AreEqual(0, page.Bounds.Y0, "The page Y0 is wrong.");
            Assert.AreEqual(4000, page.Bounds.X1, "The page X1 is wrong.");
            Assert.AreEqual(2600, page.Bounds.Y1, "The page Y1 is wrong.");
        }

        [TestMethod]
        public void MuPDFPageBoundsPNG()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);
            using MuPDFPage page = document.Pages[0];

            Assert.AreEqual(4000, page.Bounds.Width, "The page width is wrong.");
            Assert.AreEqual(2600, page.Bounds.Height, "The page height is wrong.");
            Assert.AreEqual(0, page.Bounds.X0, "The page X0 is wrong.");
            Assert.AreEqual(0, page.Bounds.Y0, "The page Y0 is wrong.");
            Assert.AreEqual(4000, page.Bounds.X1, "The page X1 is wrong.");
            Assert.AreEqual(2600, page.Bounds.Y1, "The page Y1 is wrong.");

            Assert.AreEqual(3000, page.OriginalBounds.Width, "The page's original width is wrong.");
            Assert.AreEqual(1950, page.OriginalBounds.Height, "The page's original height is wrong.");
            Assert.AreEqual(0, page.OriginalBounds.X0, "The page's original X0 is wrong.");
            Assert.AreEqual(0, page.OriginalBounds.Y0, "The page's original Y0 is wrong.");
            Assert.AreEqual(3000, page.OriginalBounds.X1, "The page's original X1 is wrong.");
            Assert.AreEqual(1950, page.OriginalBounds.Y1, "The page's original Y1 is wrong.");
        }

        [TestMethod]
        public void MuPDFPagePageNumber()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Assert.AreEqual(0, document.Pages[0].PageNumber, "The page number is wrong.");
            Assert.AreEqual(1, document.Pages[1].PageNumber, "The page number is wrong.");
        }

        [TestMethod]
        public void MuPDFPagePagesFields()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFPageCollection pages = document.Pages;

            Assert.AreEqual(2, pages.Count, "The page count is wrong.");
            Assert.AreEqual(2, pages.Length, "The page length is wrong.");

            Assert.AreEqual(context, typeof(MuPDFPageCollection).GetField("OwnerContext", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(pages), "The owner context is wrong.");
            Assert.AreEqual(document, typeof(MuPDFPageCollection).GetField("OwnerDocument", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(pages), "The owner document is wrong.");
        }

        [TestMethod]
        public void MuPDFPagePagesAccessor()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFPageCollection pages = document.Pages;

            using MuPDFPage page0 = pages[0];
            using MuPDFPage page1 = pages[1];

            Assert.IsNotNull(page0, "Page 0 is null.");
            Assert.IsNotNull(page1, "Page 1 is null.");
            Assert.AreNotEqual(page0, page1, "The pages are equal.");
            Assert.AreEqual(pages[0], page0, "Page 0 is not consistent.");
            Assert.AreEqual(pages[1], page1, "Page 1 is not consistent.");
        }

        [TestMethod]
        public void MuPDFPagePagesEnumeration()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFPageCollection pages = document.Pages;

            int index = 0;

            foreach (MuPDFPage page in pages)
            {
                Assert.IsNotNull(page, "Page " + index.ToString() + " is null.");
                Assert.AreEqual(index, page.PageNumber, "The page number for page " + index.ToString() + " is wrong.");
                index++;
            }
        }

        [TestMethod]
        public void MuPDFPageBoxes()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.PageBoxes.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Rectangle rect = document.Pages[0].GetBoundingBox(BoxType.MediaBox);
            Assert.AreEqual(new Rectangle(-2, -2, 97, 95), rect, "The MediaBox is incorrect");

            rect = document.Pages[0].GetBoundingBox(BoxType.CropBox);
            Assert.AreEqual(new Rectangle(0, 0, 95, 93), rect, "The CropBox is incorrect");

            rect = document.Pages[0].GetBoundingBox(BoxType.TrimBox);
            Assert.AreEqual(new Rectangle(2, 2, 93, 91), rect, "The TrimBox is incorrect");

            rect = document.Pages[0].GetBoundingBox(BoxType.ArtBox);
            Assert.AreEqual(new Rectangle(4, 4, 91, 89), rect, "The ArtBox is incorrect");

            rect = document.Pages[0].GetBoundingBox(BoxType.BleedBox);
            Assert.AreEqual(new Rectangle(6, 6, 89, 87), rect, "The BleedBox is incorrect");

            rect = document.Pages[0].GetBoundingBox(BoxType.UnknownBox);
            Assert.AreEqual(new Rectangle(0, 0, 95, 93), rect, "The UnknownBox is incorrect");
        }

        [TestMethod]
        public void MuPDFPageNoLinks()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Assert.AreEqual(0, document.Pages[0].Links.Count, "A page without links has a non-empty links array.");
        }

        [TestMethod]
        public void MuPDFPageLinks()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.VectSharp.Markdown.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            string[] expectedLinkUris = new string[]
            {
                "https://commonmark.org/",
                "https://commonmark.org/",
                "https://github.com/xoofx/markdig",
                "https://github.com/xoofx/markdig",
                "https://github.com/arklumpus/highlight",
                "https://github.com/arklumpus/highlight",
                "https://www.nuget.org/packages/VectSharp.Markdown/",
                "https://www.nuget.org/packages/VectSharp.Markdown/",
                "https://www.nuget.org/packages/VectSharp.Markdown/",
                "https://www.nuget.org/packages/VectSharp.MuPDFUtils/",
                "https://www.nuget.org/packages/VectSharp.MuPDFUtils/"
            };

            Assert.AreEqual(11, document.Pages[0].Links.Count, "The number of links in the page is wrong.");
            CollectionAssert.AreEqual(Enumerable.Repeat(MuPDFLinkDestination.DestinationType.External, document.Pages[0].Links.Count).ToArray(), document.Pages[0].Links.Select(x => x.Destination.Type).ToArray(), "The link destination types are wrong.");
            CollectionAssert.AreEqual(expectedLinkUris, document.Pages[0].Links.Select(x => ((MuPDFExternalLinkDestination)x.Destination).Uri).ToArray(), "The link destinations are wrong.");
        }

        [TestMethod]
        public void MuPDFPageOCGLinks()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.NoughtsCrosses.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            bool[] expectedVisibles = new bool[]
            {
                false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false
            };

            Assert.AreEqual(36, document.Pages[0].Links.Count, "The number of links in the page is wrong.");
            CollectionAssert.AreEqual(Enumerable.Repeat(MuPDFLinkDestination.DestinationType.SetOCGState, document.Pages[0].Links.Count).ToArray(), document.Pages[0].Links.Select(x => x.Destination.Type).ToArray(), "The link destination types are wrong.");
            CollectionAssert.AreEqual(expectedVisibles, document.Pages[0].Links.Select(x => x.IsVisible).ToArray(), "The link visibilities are wrong.");

            ((MuPDFSetOCGStateLinkDestination)document.Pages[0].Links[1].Destination).Activate();

            expectedVisibles = new bool[]
            {
                false, false, false, true, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false
            };
            CollectionAssert.AreEqual(expectedVisibles, document.Pages[0].Links.Select(x => x.IsVisible).ToArray(), "The link visibilities are wrong.");

            document.OptionalContentGroupData.DefaultConfiguration.Activate();

            expectedVisibles = new bool[]
            {
                false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false, false, true, false, false
            };
            CollectionAssert.AreEqual(expectedVisibles, document.Pages[0].Links.Select(x => x.IsVisible).ToArray(), "The link visibilities are wrong.");
        }
    }
}
