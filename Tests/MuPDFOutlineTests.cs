using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System.IO;

namespace Tests
{
    [TestClass]
    public class MuPDFOutlineTests
    {
        [TestMethod]
        public void EmptyMuPDFOutline()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Assert.IsNotNull(document.Outline, "The document outline is null.");
            Assert.AreEqual(0, document.Outline.Count, "The document outline is not empty.");
        }

        [TestMethod]
        public void PopulatedMuPDFOutline()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.mupdf_explored.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Assert.IsNotNull(document.Outline, "The document outline is null.");
            Assert.AreNotEqual(0, document.Outline.Count, "The document outline is empty.");
            Assert.AreEqual(9, document.Outline.Count, "The document outline does not contain the expected number of elements.");
            Assert.AreEqual("Preface", document.Outline[0].Title, "The title of the top outline element is not as expected.");
            Assert.AreEqual("Page Level Functions", document.Outline[5].Children[1].Children[1].Children[2].Title, "The title of an inner element is not as expected.");
            Assert.AreEqual("Coding Style", document.Outline[8].Children[1].Children[2].Title, "The title of the last element is not as expected.");
        }

        [TestMethod]
        public void EpubOutlineLocation()
        {
            using Stream epubDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.basic-v3plus2.epub");
            MemoryStream epubStream = new MemoryStream();
            epubDataStream.CopyTo(epubStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref epubStream, InputFileTypes.PDF);

            Assert.IsNotNull(document.Outline, "The document outline is null.");
            Assert.AreNotEqual(0, document.Outline.Count, "The document outline is empty.");
            Assert.AreEqual(2, document.Outline.Count, "The document outline does not contain the expected number of elements.");
            Assert.AreEqual(1, document.Outline[0].Chapter, "The chapter number for the first outline item is wrong.");
            Assert.AreEqual(0, document.Outline[0].Page, "The page number for the first outline item is wrong.");
            Assert.AreEqual(1, document.Outline[0].PageNumber, "The full page number for the first outline item is wrong.");
            Assert.AreEqual(2, document.Outline[1].Chapter, "The chapter number for the second outline item is wrong.");
            Assert.AreEqual(0, document.Outline[1].Page, "The page number for the second outline item is wrong.");
            Assert.AreEqual(2, document.Outline[1].PageNumber, "The full page number for the second outline item is wrong.");
        }

        [TestMethod]
        public void EpubOutlineLocationReflow()
        {
            using Stream epubDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.basic-v3plus2.epub");
            MemoryStream epubStream = new MemoryStream();
            epubDataStream.CopyTo(epubStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref epubStream, InputFileTypes.PDF);

            document.Layout(595, 842, 10);

            Assert.IsNotNull(document.Outline, "The document outline is null.");
            Assert.AreNotEqual(0, document.Outline.Count, "The document outline is empty.");
            Assert.AreEqual(2, document.Outline.Count, "The document outline does not contain the expected number of elements.");
            Assert.AreEqual(1, document.Outline[0].Chapter, "The chapter number for the first outline item is wrong.");
            Assert.AreEqual(0, document.Outline[0].Page, "The page number for the first outline item is wrong.");
            Assert.AreEqual(1, document.Outline[0].PageNumber, "The full page number for the first outline item is wrong.");
            Assert.AreEqual(2, document.Outline[1].Chapter, "The chapter number for the second outline item is wrong.");
            Assert.AreEqual(0, document.Outline[1].Page, "The page number for the second outline item is wrong.");
            Assert.AreEqual(2, document.Outline[1].PageNumber, "The full page number for the second outline item is wrong.");

            document.Layout(420, 595, 10);
            Assert.AreEqual(1, document.Outline[0].Chapter, "The chapter number for the second outline item after reflow is wrong.");
            Assert.AreEqual(0, document.Outline[0].Page, "The page number for the second outline item after reflow is wrong.");
            Assert.AreEqual(1, document.Outline[0].PageNumber,"The full page number for the second outline item after reflow is wrong.");
            Assert.AreEqual(2, document.Outline[1].Chapter, "The chapter number for the second outline item after reflow is wrong.");
            Assert.AreEqual(0, document.Outline[1].Page, "The page number for the second outline item after reflow is wrong.");
            Assert.AreEqual(3, document.Outline[1].PageNumber, "The full page number for the second outline item after reflow is wrong.");
        }
    }
}
