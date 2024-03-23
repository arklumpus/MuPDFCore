using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
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
            Assert.AreEqual(document.Outline.Count, 0, "The document outline is not empty.");
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
            Assert.AreNotEqual(document.Outline.Count, 0, "The document outline is empty.");
            Assert.AreEqual(document.Outline.Count, 9, "The document outline does not contain the expected number of elements.");
            Assert.AreEqual(document.Outline[0].Title, "Preface", "The title of the top outline element is not as expected.");
            Assert.AreEqual(document.Outline[5].Children[1].Children[1].Children[2].Title, "Page Level Functions", "The title of an inner element is not as expected.");
            Assert.AreEqual(document.Outline[8].Children[1].Children[2].Title, "Coding Style", "The title of the last element is not as expected.");
        }
    }
}
