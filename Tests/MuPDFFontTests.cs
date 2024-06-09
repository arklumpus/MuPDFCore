using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.IO;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    internal class MuPDFFontTests
    {
        [TestMethod]
        public void MuPDFFontMembers()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            MuPDFStructuredTextCharacter chr = sTextPage[5][0][8];

            MuPDFFont font = chr.Font;

            Assert.AreEqual("Helvetica-Bold", font.Name, "The font's name is wrong.");
            Assert.IsTrue(font.IsBold, "The font's weight is wrong.");
            Assert.IsFalse(font.IsItalic, "The font's style is wrong.");
            Assert.IsFalse(font.IsMonospaced, "The font's spacing is wrong.");
            Assert.IsFalse(font.IsSerif, "The font's serifness (?) is wrong.");
            Assert.AreNotEqual(IntPtr.Zero, font.GetFreeTypeHandle(), "The font's FT_Face handle is NULL.");
            Assert.AreEqual(IntPtr.Zero, font.GetType3Handle(), "The font's Type3 procs handle is not NULL.");
        }

    }
}
