using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System.IO;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class MuPDFColorSpaceTests
    {
        [TestMethod]
        public void MuPDFColorSpaceMembers_SampleRGB()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, preserveImages: true);
            MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];
            MuPDFImage image = imageBlock.Image;
            MuPDFColorSpace colorSpace = image.ColorSpace;

            Assert.AreEqual(ColorSpaceType.RGB, colorSpace.Type, "The colour space type is wrong.");
            Assert.AreEqual("DeviceRGB", colorSpace.Name, "The colour space name is wrong.");
            Assert.AreEqual(3, colorSpace.NumBytes, "The number of bytes in the colour space is wrong.");
            Assert.AreEqual(colorSpace, colorSpace.RootColorSpace, "The root colour space is wrong.");
        }

        [TestMethod]
        public void MuPDFColorSpaceMembers_SampleGray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.Gray.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, preserveImages: true);
            MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];
            MuPDFImage image = imageBlock.Image;
            MuPDFColorSpace colorSpace = image.ColorSpace;

            Assert.AreEqual(ColorSpaceType.Gray, colorSpace.Type, "The colour space type is wrong.");
            Assert.AreEqual("DeviceGray", colorSpace.Name, "The colour space name is wrong.");
            Assert.AreEqual(1, colorSpace.NumBytes, "The number of bytes in the colour space is wrong.");
            Assert.AreEqual(colorSpace, colorSpace.RootColorSpace, "The root colour space is wrong.");
        }

        [TestMethod]
        public void MuPDFColorSpaceMembers_SampleCMYK()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, preserveImages: true);
            MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];
            MuPDFImage image = imageBlock.Image;
            MuPDFColorSpace colorSpace = image.ColorSpace;

            Assert.AreEqual(ColorSpaceType.CMYK, colorSpace.Type, "The colour space type is wrong.");
            Assert.AreEqual("DeviceCMYK", colorSpace.Name, "The colour space name is wrong.");
            Assert.AreEqual(4, colorSpace.NumBytes, "The number of bytes in the colour space is wrong.");
            Assert.AreEqual(colorSpace, colorSpace.RootColorSpace, "The root colour space is wrong.");
        }
    }
}
