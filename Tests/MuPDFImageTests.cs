using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.IO;
using MuPDFCore.StructuredText;

#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0230 // Use UTF-8 string literal

namespace Tests
{
    [TestClass]
    public class MuPDFImageTests
    {
        [TestMethod]
        public void MuPDFImageMembers()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            Assert.AreEqual(96, image.XRes, "The image's horizontal resolution is wrong!");
            Assert.AreEqual(96, image.YRes, "The image's vertical resolution is wrong!");
            Assert.AreEqual(1280, image.Width, "The image's width is wrong!");
            Assert.AreEqual(640, image.Height, "The image's height is wrong!");
            Assert.AreEqual(MuPDFImage.ImageOrientation.Undefined, image.Orientation, "The image's orientation is wrong!");
            Assert.IsNotNull(image.ColorSpace, "The image's colour space is null!");
        }

        [TestMethod]
        public void MuPDFImageGetBytes_SampleRGB()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            byte[] bytes = image.GetBytes();

            Assert.AreEqual(image.Width * image.Height * 3, bytes.Length, "The byte size of the image is wrong!");
            Assert.AreEqual(15, bytes[366783], "The image pixels are wrong!");
            Assert.AreEqual(27, bytes[366784], "The image pixels are wrong!");
            Assert.AreEqual(39, bytes[366785], "The image pixels are wrong!");
        }

        [TestMethod]
        public void MuPDFImageGetBytes_SampleCMYK()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            byte[] bytes = image.GetBytes();

            Assert.AreEqual(image.Width * image.Height * 4, bytes.Length, "The byte size of the image is wrong!");
            Assert.AreEqual(192, bytes[0], "The image pixels are wrong!");
            Assert.AreEqual(169, bytes[1], "The image pixels are wrong!");
            Assert.AreEqual(162, bytes[2], "The image pixels are wrong!");
            Assert.AreEqual(242, bytes[3], "The image pixels are wrong!");
        }

        [TestMethod]
        public void MuPDFImageGetBytes_SampleGray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.Gray.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            byte[] bytes = image.GetBytes();

            Assert.AreEqual(image.Width * image.Height, bytes.Length, "The byte size of the image is wrong!");
            Assert.AreEqual(37, bytes[122262], "The image pixels are wrong!");
            Assert.AreEqual(44, bytes[122263], "The image pixels are wrong!");
            Assert.AreEqual(55, bytes[122264], "The image pixels are wrong!");
        }

        [TestMethod]
        public void MuPDFImageGetBytesRGBA_SampleRGB()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            byte[] bytes = image.GetBytes(PixelFormats.RGBA);

            Assert.AreEqual(image.Width * image.Height * 4, bytes.Length, "The byte size of the image is wrong!");
            Assert.AreEqual(15, bytes[489044], "The image pixels are wrong!");
            Assert.AreEqual(27, bytes[489045], "The image pixels are wrong!");
            Assert.AreEqual(39, bytes[489046], "The image pixels are wrong!");
            Assert.AreEqual(255, bytes[489047], "The image pixels are wrong!");
        }

        [TestMethod]
        public void MuPDFImageGetBytesRGB_SampleCMYK()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);
            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];
            using MuPDFImage image = imageBlock.Image;

            byte[] bytes = image.GetBytes(PixelFormats.RGB);

            Assert.AreEqual(image.Width * image.Height * 3, bytes.Length, "The byte size of the image is wrong!");
            Assert.AreEqual(13, bytes[366786], "The image pixels are wrong!");
            Assert.AreEqual(44, bytes[366787], "The image pixels are wrong!");
            Assert.AreEqual(61, bytes[366788], "The image pixels are wrong!");
        }

        [TestMethod]
        public void MuPDFImageGetBytesBGRA_SampleGray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.Gray.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            byte[] bytes = image.GetBytes(PixelFormats.BGRA);

            Assert.AreEqual(image.Width * image.Height * 4, bytes.Length, "The byte size of the image is wrong!");
            Assert.AreEqual(25, bytes[489044], "The image pixels are wrong!");
            Assert.AreEqual(25, bytes[489045], "The image pixels are wrong!");
            Assert.AreEqual(25, bytes[489046], "The image pixels are wrong!");
            Assert.AreEqual(255, bytes[489047], "The image pixels are wrong!");
        }

        [TestMethod]
        public void MuPDFImageWrite_SampleRGB()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            using MemoryStream ms = new MemoryStream();

            image.Write(ms, RasterOutputFileTypes.PNM, false);
            CollectionAssert.AreEqual(new byte[] { 80, 54  }, ms.GetBuffer()[0..2], "The image stream magic number is wrong (PNM).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.PAM, false);
            CollectionAssert.AreEqual(new byte[] { 80, 55 }, ms.GetBuffer()[0..2], "The image stream magic number is wrong (PAM).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.PNG, false);
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }, ms.GetBuffer()[0..8], "The image stream magic number is wrong (PNG).");
            ms.SetLength(0);
            
            image.Write(ms, RasterOutputFileTypes.PSD, false);
            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, ms.GetBuffer()[0..4], "The image stream magic number is wrong (PSD).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.JPEG, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
        }

        [TestMethod]
        public void MuPDFImageWriteJPEG_SampleRGB()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            using MemoryStream ms = new MemoryStream();

            image.WriteAsJPEG(ms, 80, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
            long length1 = ms.Length;
            ms.SetLength(0);

            image.WriteAsJPEG(ms, 10, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
            long length2 = ms.Length;

            Assert.IsTrue(length1 > length2, "The JPEG image with higher quality is not larger than the one with lower quality.");
        }

        [TestMethod]
        public void MuPDFImageWrite_SampleGray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.Gray.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            using MemoryStream ms = new MemoryStream();

            image.Write(ms, RasterOutputFileTypes.PNM, false);
            CollectionAssert.AreEqual(new byte[] { 80, 53 }, ms.GetBuffer()[0..2], "The image stream magic number is wrong (PNM).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.PAM, false);
            CollectionAssert.AreEqual(new byte[] { 80, 55 }, ms.GetBuffer()[0..2], "The image stream magic number is wrong (PAM).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.PNG, false);
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }, ms.GetBuffer()[0..8], "The image stream magic number is wrong (PNG).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.PSD, false);
            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, ms.GetBuffer()[0..4], "The image stream magic number is wrong (PSD).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.JPEG, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
        }

        [TestMethod]
        public void MuPDFImageWriteJPEG_SampleGray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.Gray.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            using MemoryStream ms = new MemoryStream();

            image.WriteAsJPEG(ms, 80, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
            long length1 = ms.Length;
            ms.SetLength(0);

            image.WriteAsJPEG(ms, 10, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
            long length2 = ms.Length;

            Assert.IsTrue(length1 > length2, "The JPEG image with higher quality is not larger than the one with lower quality.");
        }

        [TestMethod]
        public void MuPDFImageWrite_SampleCMYK()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            using MemoryStream ms = new MemoryStream();

            Assert.ThrowsException<ArgumentException>(() => image.Write(ms, RasterOutputFileTypes.PNM, false), "Writing an image in CMYK colour space in PNM format did not fail.");
            image.Write(ms, RasterOutputFileTypes.PNM);
            CollectionAssert.AreEqual(new byte[] { 80, 54 }, ms.GetBuffer()[0..2], "The image stream magic number is wrong (PNM).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.PAM, false);
            CollectionAssert.AreEqual(new byte[] { 80, 55 }, ms.GetBuffer()[0..2], "The image stream magic number is wrong (PAM).");
            ms.SetLength(0);

            Assert.ThrowsException<ArgumentException>(() => image.Write(ms, RasterOutputFileTypes.PNG, false), "Writing an image in CMYK colour space in PNG format did not fail.");
            image.Write(ms, RasterOutputFileTypes.PNG);
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }, ms.GetBuffer()[0..8], "The image stream magic number is wrong (PNG).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.PSD, false);
            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, ms.GetBuffer()[0..4], "The image stream magic number is wrong (PSD).");
            ms.SetLength(0);

            image.Write(ms, RasterOutputFileTypes.JPEG, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
        }

        [TestMethod]
        public void MuPDFImageWriteJPEG_SampleCMYK()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            using MemoryStream ms = new MemoryStream();

            image.WriteAsJPEG(ms, 80, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
            long length1 = ms.Length;
            ms.SetLength(0);

            image.WriteAsJPEG(ms, 10, false);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, ms.GetBuffer()[0..3], "The image stream magic number is wrong (JPEG).");
            long length2 = ms.Length;

            Assert.IsTrue(length1 > length2, "The JPEG image with higher quality is not larger than the one with lower quality.");
        }

        [TestMethod]
        public void MuPDFImageSave_SampleRGB()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            string tempFile = Path.GetTempFileName();
            byte[] fileBytes;

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            image.Save(tempFile, RasterOutputFileTypes.PNM, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PNM).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 80, 54 }, fileBytes[0..2], "The image file magic number is wrong (PNM).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PAM, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PAM).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 80, 55 }, fileBytes[0..2], "The image stream magic number is wrong (PAM).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PNG, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PNG).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }, fileBytes[0..8], "The image stream magic number is wrong (PNG).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PSD, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PSD).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, fileBytes[0..4], "The image stream magic number is wrong (PSD).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.JPEG, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (JPEG).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image stream magic number is wrong (JPEG).");
            File.Delete(tempFile);
        }

        [TestMethod]
        public void MuPDFImageSaveJPEG_SampleRGB()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.RGB.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;


            string tempFile = Path.GetTempFileName();
            byte[] fileBytes;

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            image.SaveAsJPEG(tempFile, 80, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (80).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image file magic number is wrong (JPEG).");
            int length1 = fileBytes.Length;
            File.Delete(tempFile);

            image.SaveAsJPEG(tempFile, 10, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (10).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image file magic number is wrong (JPEG).");
            int length2 = fileBytes.Length;
            File.Delete(tempFile);

            Assert.IsTrue(length1 > length2, "The JPEG image with higher quality is not larger than the one with lower quality.");
        }

        [TestMethod]
        public void MuPDFImageSave_SampleGray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.Gray.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            string tempFile = Path.GetTempFileName();
            byte[] fileBytes;

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            image.Save(tempFile, RasterOutputFileTypes.PNM, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PNM).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 80, 53 }, fileBytes[0..2], "The image file magic number is wrong (PNM).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PAM, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PAM).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 80, 55 }, fileBytes[0..2], "The image stream magic number is wrong (PAM).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PNG, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PNG).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }, fileBytes[0..8], "The image stream magic number is wrong (PNG).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PSD, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PSD).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, fileBytes[0..4], "The image stream magic number is wrong (PSD).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.JPEG, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (JPEG).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image stream magic number is wrong (JPEG).");
            File.Delete(tempFile);
        }

        [TestMethod]
        public void MuPDFImageSaveJPEG_SampleGray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.Gray.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;


            string tempFile = Path.GetTempFileName();
            byte[] fileBytes;

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            image.SaveAsJPEG(tempFile, 80, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (80).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image file magic number is wrong (JPEG).");
            int length1 = fileBytes.Length;
            File.Delete(tempFile);

            image.SaveAsJPEG(tempFile, 10, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (10).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image file magic number is wrong (JPEG).");
            int length2 = fileBytes.Length;
            File.Delete(tempFile);

            Assert.IsTrue(length1 > length2, "The JPEG image with higher quality is not larger than the one with lower quality.");
        }

        [TestMethod]
        public void MuPDFImageSave_SampleCMYK()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;

            string tempFile = Path.GetTempFileName();
            byte[] fileBytes;

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            Assert.ThrowsException<ArgumentException>(() => image.Save(tempFile, RasterOutputFileTypes.PNM, false), "Writing an image in CMYK colour space in PNM format did not fail.");
            image.Save(tempFile, RasterOutputFileTypes.PNM);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PNM).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 80, 54 }, fileBytes[0..2], "The image file magic number is wrong (PNM).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PAM, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PAM).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 80, 55 }, fileBytes[0..2], "The image stream magic number is wrong (PAM).");
            File.Delete(tempFile);

            Assert.ThrowsException<ArgumentException>(() => image.Save(tempFile, RasterOutputFileTypes.PNG, false), "Writing an image in CMYK colour space in PNG format did not fail.");
            image.Save(tempFile, RasterOutputFileTypes.PNG);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PNG).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }, fileBytes[0..8], "The image stream magic number is wrong (PNG).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.PSD, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (PSD).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, fileBytes[0..4], "The image stream magic number is wrong (PSD).");
            File.Delete(tempFile);

            image.Save(tempFile, RasterOutputFileTypes.JPEG, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (JPEG).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image stream magic number is wrong (JPEG).");
            File.Delete(tempFile);
        }

        [TestMethod]
        public void MuPDFImageSaveJPEG_SampleCMYK()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.CMYK.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, flags: StructuredTextFlags.PreserveImages);

            using MuPDFImageStructuredTextBlock imageBlock = (MuPDFImageStructuredTextBlock)sTextPage[0];

            using MuPDFImage image = imageBlock.Image;


            string tempFile = Path.GetTempFileName();
            byte[] fileBytes;

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            image.SaveAsJPEG(tempFile, 80, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (80).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image file magic number is wrong (JPEG).");
            int length1 = fileBytes.Length;
            File.Delete(tempFile);

            image.SaveAsJPEG(tempFile, 10, false);
            Assert.IsTrue(File.Exists(tempFile), "The output file does not exist (10).");
            fileBytes = File.ReadAllBytes(tempFile);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, fileBytes[0..3], "The image file magic number is wrong (JPEG).");
            int length2 = fileBytes.Length;
            File.Delete(tempFile);

            Assert.IsTrue(length1 > length2, "The JPEG image with higher quality is not larger than the one with lower quality.");
        }
    }
}
