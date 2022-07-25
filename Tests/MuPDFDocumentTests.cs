using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class MuPDFDocumentTests
    {
        [TestMethod]
        [DeploymentItem("Data/Sample.pdf")]
        public void MuPDFDocumentCreationFromPDFFile()
        {
            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, "Sample.pdf");

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        [DeploymentItem("Data/Sample.png")]
        public void MuPDFDocumentCreationFromPNGFile()
        {
            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, "Sample.png");

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(96, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(96, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        [DeploymentItem("Data/LλиՀქカかעاދदবਗગଓதతಕമසไမᠮབລខᑐᑯᒐᖃግ조한汉漢.pdf")]
        public void MuPDFDocumentCreationFromFileWithUTF8Characters()
        {
            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, "LλиՀქカかעاދदবਗગଓதతಕമසไမᠮབລខᑐᑯᒐᖃግ조한汉漢.pdf");

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPDFStream()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPNGStream()
        {
            using Stream pngDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pngStream = new MemoryStream();
            pngDataStream.CopyTo(pngStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pngStream, InputFileTypes.PNG);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(96, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(96, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPDFBytes()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            using MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, pdfStream.ToArray(), InputFileTypes.PDF);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPNGBytes()
        {
            using Stream pngDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            using MemoryStream pngStream = new MemoryStream();
            pngDataStream.CopyTo(pngStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, pngStream.ToArray(), InputFileTypes.PNG);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(96, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(96, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPDFIntPtr()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            using MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            byte[] dataBytes = pdfStream.ToArray();
            GCHandle dataHandle = GCHandle.Alloc(dataBytes, GCHandleType.Pinned);
            IntPtr dataAddress = IntPtr.Add(dataHandle.AddrOfPinnedObject(), 0);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, dataAddress, dataBytes.Length, InputFileTypes.PDF);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");

            dataHandle.Free();
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPNGIntPtr()
        {
            using Stream pngDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            using MemoryStream pngStream = new MemoryStream();
            pngDataStream.CopyTo(pngStream);

            byte[] dataBytes = pngStream.ToArray();
            GCHandle dataHandle = GCHandle.Alloc(dataBytes, GCHandleType.Pinned);
            IntPtr dataAddress = IntPtr.Add(dataHandle.AddrOfPinnedObject(), 0);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, dataAddress, dataBytes.Length, InputFileTypes.PNG);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(96, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(96, document.ImageYRes, "The image x resolution is wrong.");

            dataHandle.Free();
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPDFIntPtrWithDisposable()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            using MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            byte[] dataBytes = pdfStream.ToArray();

            IntPtr dataAddress = Marshal.AllocHGlobal(dataBytes.Length);
            Marshal.Copy(dataBytes, 0, dataAddress, dataBytes.Length);
            IDisposable disposable = new DisposableIntPtr(dataAddress);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, dataAddress, dataBytes.Length, InputFileTypes.PDF, ref disposable);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentCreationFromPNGIntPtrWithDisposable()
        {
            using Stream pngDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            using MemoryStream pngStream = new MemoryStream();
            pngDataStream.CopyTo(pngStream);

            byte[] dataBytes = pngStream.ToArray();

            IntPtr dataAddress = Marshal.AllocHGlobal(dataBytes.Length);
            Marshal.Copy(dataBytes, 0, dataAddress, dataBytes.Length);
            IDisposable disposable = new DisposableIntPtr(dataAddress);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, dataAddress, dataBytes.Length, InputFileTypes.PNG, ref disposable);

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(96, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(96, document.ImageYRes, "The image x resolution is wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentPages()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Assert.IsNotNull(document.Pages, "The pages of the document are null.");
            Assert.AreEqual(2, document.Pages.Count, "The page count is wrong.");
            Assert.AreEqual(2, document.Pages.Length, "The length of pages is wrong.");

            for (int i = 0; i < document.Pages.Count; i++)
            {
                Assert.IsNotNull(document.Pages[i], "The document containes a null page.");
            }
        }

        [TestMethod]
        public void MuPDFDocumentRenderingFullPageToRGBByteArray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            byte[] rendered = document.Render(0, 1, PixelFormats.RGB);
            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);
            Assert.IsNotNull(displayLists[0], "The display list has not been generated.");

            Assert.AreEqual(4000 * 2600 * 3, rendered.Length, "The size of the rendered image is wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, rendered[0..3], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, rendered[^3..^0], "The end of the rendered image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentRenderingFullPageToBGRAByteArray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            byte[] rendered = document.Render(0, 1, PixelFormats.BGRA);
            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);
            Assert.IsNotNull(displayLists[0], "The display list has not been generated.");

            Assert.AreEqual(4000 * 2600 * 4, rendered.Length, "The size of the rendered image is wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x73, 0x17, 0x0B, }, rendered[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x73, 0x17, 0x0B }, rendered[^4..^0], "The end of the rendered image appears to be wrong.");
        }


        [TestMethod]
        public void MuPDFDocumentRenderingRegionToRGBAByteArray()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            byte[] rendered = document.Render(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA);
            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);
            Assert.IsNotNull(displayLists[0], "The display list has not been generated.");

            Assert.AreEqual(567 * 567 * 4, rendered.Length, "The size of the rendered image is wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x73, 0xFF, 0x0B }, rendered[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x73, 0xFF, 0x0B }, rendered[^4..^0], "The end of the rendered image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentRenderingFullPageToRGBIntPtr()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);
            int bufferSize = 4000 * 2600 * 3;

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            IntPtr destination = Marshal.AllocHGlobal(bufferSize);
            document.Render(0, 1, PixelFormats.RGB, destination);

            byte[] rendered = new byte[bufferSize];
            Marshal.Copy(destination, rendered, 0, bufferSize);

            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);
            Assert.IsNotNull(displayLists[0], "The display list has not been generated.");

            Assert.AreEqual(4000 * 2600 * 3, rendered.Length, "The size of the rendered image is wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, rendered[0..3], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, rendered[^3..^0], "The end of the rendered image appears to be wrong.");

            Marshal.FreeHGlobal(destination);
        }

        [TestMethod]
        public void MuPDFDocumentRenderingRegionToRGBAIntPtr()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            int bufferSize = 567 * 567 * 4;

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            IntPtr destination = Marshal.AllocHGlobal(bufferSize);
            document.Render(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA, destination);

            byte[] rendered = new byte[bufferSize];
            Marshal.Copy(destination, rendered, 0, bufferSize);

            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);
            Assert.IsNotNull(displayLists[0], "The display list has not been generated.");

            Assert.AreEqual(567 * 567 * 4, rendered.Length, "The size of the rendered image is wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x73, 0xFF, 0x0B }, rendered[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x73, 0xFF, 0x0B }, rendered[^4..^0], "The end of the rendered image appears to be wrong.");

            Marshal.FreeHGlobal(destination);
        }

        [TestMethod]
        public void MuPDFDocumentRenderingFullPageToRGBSpan()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Span<byte> rendered = document.Render(0, 1, PixelFormats.RGB, out IDisposable disposable);

            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);
            Assert.IsNotNull(displayLists[0], "The display list has not been generated.");

            Assert.AreEqual(4000 * 2600 * 3, rendered.Length, "The size of the rendered image is wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, new byte[] { rendered[0], rendered[1], rendered[2] }, "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, new byte[] { rendered[rendered.Length - 3], rendered[rendered.Length - 2], rendered[rendered.Length - 1] }, "The end of the rendered image appears to be wrong.");

            disposable.Dispose();
        }

        [TestMethod]
        public void MuPDFDocumentRenderingRegionToRGBASpan()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            Span<byte> rendered = document.Render(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA, out IDisposable disposable);

            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);
            Assert.IsNotNull(displayLists[0], "The display list has not been generated.");

            Assert.AreEqual(567 * 567 * 4, rendered.Length, "The size of the rendered image is wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x73, 0xFF, 0x0B }, new byte[] { rendered[0], rendered[1], rendered[2], rendered[3] }, "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x73, 0xFF, 0x0B }, new byte[] { rendered[rendered.Length - 4], rendered[rendered.Length - 3], rendered[rendered.Length - 2], rendered[rendered.Length - 1] }, "The end of the rendered image appears to be wrong.");

            disposable.Dispose();
        }

        [TestMethod]
        public void MuPDFDocumentCacheClearing()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            _ = document.Render(0, 1, PixelFormats.RGB);
            _ = document.Render(1, 1, PixelFormats.RGB);

            MuPDFDisplayList[] displayLists = (MuPDFDisplayList[])typeof(MuPDFDocument).GetField("DisplayLists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(document);

            document.ClearCache();

            for (int i = 0; i < displayLists.Length; i++)
            {
                Assert.IsNull(displayLists[i], "Not all display lists have been freed.");
            }
        }

        [TestMethod]
        public void MuPDFDocumentRenderedSizeEstimationFullPage()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            int expectedSizeBGRA = document.Render(0, 1, PixelFormats.BGRA).Length;
            int expectedSizeBGR = document.Render(0, Math.Sqrt(2), PixelFormats.BGR).Length;
            int expectedSizeRGBA = document.Render(0, Math.Sqrt(2), PixelFormats.RGBA).Length;
            int expectedSizeRGB = document.Render(0, 1, PixelFormats.RGB).Length;

            int actualSizeBGRA = document.GetRenderedSize(0, 1, PixelFormats.BGRA);
            int actualSizeBGR = document.GetRenderedSize(0, Math.Sqrt(2), PixelFormats.BGR);
            int actualSizeRGBA = document.GetRenderedSize(0, Math.Sqrt(2), PixelFormats.RGBA);
            int actualSizeRGB = document.GetRenderedSize(0, 1, PixelFormats.RGB);

            Assert.AreEqual(expectedSizeBGRA, actualSizeBGRA, "The estimated size for the BGRA image does not correspond to the real size.");
            Assert.AreEqual(expectedSizeBGR, actualSizeBGR, "The estimated size for the BGR image does not correspond to the real size.");
            Assert.AreEqual(expectedSizeRGBA, actualSizeRGBA, "The estimated size for the RGBA image does not correspond to the real size.");
            Assert.AreEqual(expectedSizeRGB, actualSizeRGB, "The estimated size for the RGB image does not correspond to the real size.");
        }

        [TestMethod]
        public void MuPDFDocumentRenderedSizeEstimationRegion()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            int expectedSizeBGRA = document.Render(0, new Rectangle(100, 100, 500, 500), 1, PixelFormats.BGRA).Length;
            int expectedSizeBGR = document.Render(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.BGR).Length;
            int expectedSizeRGBA = document.Render(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA).Length;
            int expectedSizeRGB = document.Render(0, new Rectangle(100, 100, 500, 500), 1, PixelFormats.RGB).Length;

            int actualSizeBGRA = MuPDFDocument.GetRenderedSize(new Rectangle(100, 100, 500, 500), 1, PixelFormats.BGRA);
            int actualSizeBGR = MuPDFDocument.GetRenderedSize(new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.BGR);
            int actualSizeRGBA = MuPDFDocument.GetRenderedSize(new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA);
            int actualSizeRGB = MuPDFDocument.GetRenderedSize(new Rectangle(100, 100, 500, 500), 1, PixelFormats.RGB);

            Assert.AreEqual(expectedSizeBGRA, actualSizeBGRA, "The estimated size for the BGRA image does not correspond to the real size.");
            Assert.AreEqual(expectedSizeBGR, actualSizeBGR, "The estimated size for the BGR image does not correspond to the real size.");
            Assert.AreEqual(expectedSizeRGBA, actualSizeRGBA, "The estimated size for the RGBA image does not correspond to the real size.");
            Assert.AreEqual(expectedSizeRGB, actualSizeRGB, "The estimated size for the RGB image does not correspond to the real size.");
        }

        [TestMethod]
        public void MuPDFDocumentMultiThreadedRendererGetter()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFMultiThreadedPageRenderer renderer = document.GetMultiThreadedRenderer(0, 11);

            Assert.IsNotNull(renderer, "The multi-threaded page renderer is null.");
            Assert.AreEqual(document.ImageXRes, renderer.ImageXRes, "The image x resolution for the renderer differs from the document's.");
            Assert.AreEqual(document.ImageYRes, renderer.ImageYRes, "The image x resolution for the renderer differs from the document's.");
        }

        [TestMethod]
        public void MuPDFDocumentImageSavingFullPagePNG()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            string tempFile = Path.GetTempFileName();

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            document.SaveImage(0, 1, PixelFormats.RGBA, tempFile, RasterOutputFileTypes.PNG);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, savedBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, savedBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageSavingWithUTF8Characters()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            string tempFile = Path.GetTempFileName();

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            tempFile += "LλиՀქカかעاދदবਗગଓதతಕമසไမᠮབລខᑐᑯᒐᖃግ조한汉漢";

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            document.SaveImage(0, 1, PixelFormats.RGBA, tempFile, RasterOutputFileTypes.PNG);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, savedBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, savedBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageSavingRegionPNG()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            string tempFile = Path.GetTempFileName();

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            document.SaveImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA, tempFile, RasterOutputFileTypes.PNG);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, savedBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, savedBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageSavingRegionPAM()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            string tempFile = Path.GetTempFileName();

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            document.SaveImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA, tempFile, RasterOutputFileTypes.PAM);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x37, 0x0A, 0x57 }, savedBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x74, 0xFF, 0x0B }, savedBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageSavingRegionPNM()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            string tempFile = Path.GetTempFileName();

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            document.SaveImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGB, tempFile, RasterOutputFileTypes.PNM);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x36, 0x0A, }, savedBytes[0..3], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, savedBytes[^3..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageSavingRegionPSD()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            string tempFile = Path.GetTempFileName();

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            document.SaveImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGB, tempFile, RasterOutputFileTypes.PSD);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, savedBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, savedBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageWritingFullPagePNG()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();

            document.WriteImage(0, 1, PixelFormats.RGBA, renderStream, RasterOutputFileTypes.PNG);

            byte[] writtenBytes = renderStream.ToArray();

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, writtenBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, writtenBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }


        [TestMethod]
        public void MuPDFDocumentImageWritingRegionPNG()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();

            document.WriteImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA, renderStream, RasterOutputFileTypes.PNG);

            byte[] writtenBytes = renderStream.ToArray();

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, writtenBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, writtenBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageWritingRegionPAM()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();

            document.WriteImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGBA, renderStream, RasterOutputFileTypes.PAM);

            byte[] writtenBytes = renderStream.ToArray();

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x37, 0x0A, 0x57 }, writtenBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x74, 0xFF, 0x0B }, writtenBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageWritingRegionPNM()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();

            document.WriteImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGB, renderStream, RasterOutputFileTypes.PNM);

            byte[] writtenBytes = renderStream.ToArray();

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x36, 0x0A, }, writtenBytes[0..3], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF4, 0xF9, 0xFF }, writtenBytes[^3..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageWritingRegionPSD()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();

            document.WriteImage(0, new Rectangle(100, 100, 500, 500), Math.Sqrt(2), PixelFormats.RGB, renderStream, RasterOutputFileTypes.PSD);

            byte[] writtenBytes = renderStream.ToArray();

            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53 }, writtenBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, writtenBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageWritingWithoutAnnotation()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();

            document.WriteImage(0, 1, PixelFormats.RGBA, renderStream, RasterOutputFileTypes.PNG, false);

            byte[] writtenBytes = renderStream.ToArray();

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, writtenBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, writtenBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageWritingWithAnnotation()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();

            document.WriteImage(0, 1, PixelFormats.RGBA, renderStream, RasterOutputFileTypes.PNG, true);

            byte[] writtenBytes = renderStream.ToArray();

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, writtenBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, writtenBytes[^4..^0], "The end of the saved image appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentImageWritingWithAndWithoutAnnotationAreDifferent()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MemoryStream renderStream = new MemoryStream();
            document.WriteImage(0, 1, PixelFormats.RGBA, renderStream, RasterOutputFileTypes.PNG, false);
            byte[] writtenBytes = renderStream.ToArray();

            document.ClearCache();

            using MemoryStream renderStream2 = new MemoryStream();
            document.WriteImage(0, 1, PixelFormats.RGBA, renderStream2, RasterOutputFileTypes.PNG, true);
            byte[] writtenBytes2 = renderStream2.ToArray();

            CollectionAssert.AreNotEqual(writtenBytes, writtenBytes2, "The images produced with and without annotation are identical.");
        }

        [TestMethod]
        public void MuPDFDocumentPDFDocumentCreationWithFullPages()
        {
            using Stream pdfDataStream1 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream1 = new MemoryStream();
            pdfDataStream1.CopyTo(pdfStream1);

            using Stream pdfDataStream2 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream2 = new MemoryStream();
            pdfDataStream2.CopyTo(pdfStream2);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document1 = new MuPDFDocument(context, ref pdfStream1, InputFileTypes.PDF);
            using MuPDFDocument document2 = new MuPDFDocument(context, ref pdfStream2, InputFileTypes.PDF);

            string tempFile = Path.GetTempFileName();

            MuPDFDocument.CreateDocument(context, tempFile, DocumentOutputFileTypes.PDF, true, document1.Pages[0], document2.Pages[0]);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }, savedBytes[0..4], "The start of the created document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x45, 0x4F, 0x46, 0x0A }, savedBytes[^4..^0], "The end of the created document appears to be wrong.");
        }


        [TestMethod]
        public void MuPDFDocumentPDFDocumentCreationWithUTF8Characters()
        {
            using Stream pdfDataStream1 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream1 = new MemoryStream();
            pdfDataStream1.CopyTo(pdfStream1);

            using Stream pdfDataStream2 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream2 = new MemoryStream();
            pdfDataStream2.CopyTo(pdfStream2);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document1 = new MuPDFDocument(context, ref pdfStream1, InputFileTypes.PDF);
            using MuPDFDocument document2 = new MuPDFDocument(context, ref pdfStream2, InputFileTypes.PDF);

            string tempFile = Path.GetTempFileName();

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            tempFile += "LλиՀქカかעاދदবਗગଓதతಕമසไမᠮབລខᑐᑯᒐᖃግ조한汉漢";

            MuPDFDocument.CreateDocument(context, tempFile, DocumentOutputFileTypes.PDF, true, document1.Pages[0], document2.Pages[0]);

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }, savedBytes[0..4], "The start of the created document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x45, 0x4F, 0x46, 0x0A }, savedBytes[^4..^0], "The end of the created document appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentPDFDocumentCreationWithRegion()
        {
            using Stream pdfDataStream1 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream1 = new MemoryStream();
            pdfDataStream1.CopyTo(pdfStream1);

            using Stream pdfDataStream2 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream2 = new MemoryStream();
            pdfDataStream2.CopyTo(pdfStream2);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document1 = new MuPDFDocument(context, ref pdfStream1, InputFileTypes.PDF);
            using MuPDFDocument document2 = new MuPDFDocument(context, ref pdfStream2, InputFileTypes.PDF);

            string tempFile = Path.GetTempFileName();

            MuPDFDocument.CreateDocument(context, tempFile, DocumentOutputFileTypes.PDF, true, new (MuPDFPage page, Rectangle region, float zoom)[]
            {
                (document1.Pages[0], new Rectangle(100, 100, 500, 500), (float)Math.Sqrt(2)),
                (document2.Pages[0], new Rectangle(0, 0, 500, 500), (float)Math.Sqrt(3)),
            });

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }, savedBytes[0..4], "The start of the created document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x45, 0x4F, 0x46, 0x0A }, savedBytes[^4..^0], "The end of the created document appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentSVGDocumentCreationWithRegion()
        {
            using Stream pdfDataStream1 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream1 = new MemoryStream();
            pdfDataStream1.CopyTo(pdfStream1);

            using Stream pdfDataStream2 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream2 = new MemoryStream();
            pdfDataStream2.CopyTo(pdfStream2);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document1 = new MuPDFDocument(context, ref pdfStream1, InputFileTypes.PDF);
            using MuPDFDocument document2 = new MuPDFDocument(context, ref pdfStream2, InputFileTypes.PDF);

            string tempFile = Path.GetTempFileName();

            Assert.ThrowsException<ArgumentException>(() => MuPDFDocument.CreateDocument(context, tempFile, DocumentOutputFileTypes.SVG, true, new (MuPDFPage page, Rectangle region, float zoom)[]
            {
                (document1.Pages[0], new Rectangle(100, 100, 500, 500), (float)Math.Sqrt(2)),
                (document2.Pages[0], new Rectangle(0, 0, 500, 500), (float)Math.Sqrt(3)),
            }), "Creating an SVG document with multiple pages succeeded.");

            MuPDFDocument.CreateDocument(context, tempFile, DocumentOutputFileTypes.SVG, true, new (MuPDFPage page, Rectangle region, float zoom)[]
            {
                (document1.Pages[0], new Rectangle(100, 100, 500, 500), (float)Math.Sqrt(2))
            });

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x3C, 0x3F, 0x78, 0x6D }, savedBytes[0..4], "The start of the created document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x76, 0x67, 0x3E, 0x0A }, savedBytes[^4..^0], "The end of the created document appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentCBZDocumentCreationWithRegion()
        {
            using Stream pdfDataStream1 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream1 = new MemoryStream();
            pdfDataStream1.CopyTo(pdfStream1);

            using Stream pdfDataStream2 = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream2 = new MemoryStream();
            pdfDataStream2.CopyTo(pdfStream2);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document1 = new MuPDFDocument(context, ref pdfStream1, InputFileTypes.PDF);
            using MuPDFDocument document2 = new MuPDFDocument(context, ref pdfStream2, InputFileTypes.PDF);

            string tempFile = Path.GetTempFileName();

            MuPDFDocument.CreateDocument(context, tempFile, DocumentOutputFileTypes.CBZ, true, new (MuPDFPage page, Rectangle region, float zoom)[]
            {
                (document1.Pages[0], new Rectangle(100, 100, 500, 500), (float)Math.Sqrt(2)),
                (document2.Pages[0], new Rectangle(0, 0, 500, 500), (float)Math.Sqrt(3)),
            });

            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] savedBytes = File.ReadAllBytes(tempFile);

            try
            {
                File.Delete(tempFile);
            }
            catch { }

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, savedBytes[0..4], "The start of the document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x75, 0x50, 0x44, 0x46 }, savedBytes[^4..^0], "The end of the document appears to be wrong.");
        }

        [TestMethod]
        public void MuPDFDocumentStructuredTextPageGetter()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0);

            Assert.IsNotNull(sTextPage, "The structured text page is null.");
            Assert.IsTrue(sTextPage.Count > 0, "The structured text page is empty.");
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public void MuPDFDocumentStructuredTextPageGetterWithOCR()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            MuPDFStructuredTextPage sTextPage = document.GetStructuredTextPage(0, new TesseractLanguage("eng.traineddata"));

            Assert.IsNotNull(sTextPage, "The structured text page is null.");
            Assert.IsTrue(sTextPage.Count > 0, "The structured text page is empty.");
        }

        [TestMethod]
        public void MuPDFDocumentStructuredTextPageGetterWithAnnotations()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Annotation.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            MuPDFStructuredTextPage sTextPageWithAnnotation = document.GetStructuredTextPage(0, true);
            document.ClearCache();
            MuPDFStructuredTextPage sTextPageWithoutAnnotation = document.GetStructuredTextPage(0, false);

            Assert.IsTrue(sTextPageWithAnnotation.Count > sTextPageWithoutAnnotation.Count, "The structured text page with annotations does not have more elements than the one without annotations.");
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public async Task MuPDFDocumentStructuredTextPageGetterWithOCRAsync()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            MuPDFStructuredTextPage sTextPage = await document.GetStructuredTextPageAsync(0, new TesseractLanguage("eng.traineddata"));

            Assert.IsNotNull(sTextPage, "The structured text page is null.");
            Assert.IsTrue(sTextPage.Count > 0, "The structured text page is empty.");
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public async Task MuPDFDocumentStructuredTextPageGetterWithOCRAsyncProgress()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            int progressCount = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                await Assert.ThrowsExceptionAsync<PlatformNotSupportedException>(async () => await document.GetStructuredTextPageAsync(0, new TesseractLanguage("eng.traineddata"), progress: new Progress<OCRProgressInfo>(prog => progressCount++)), "Providing a progress callback should throw a PlatformNotSupportedException on Windows x86.");
            }
            else
            {
                MuPDFStructuredTextPage sTextPage = await document.GetStructuredTextPageAsync(0, new TesseractLanguage("eng.traineddata"), progress: new Progress<OCRProgressInfo>(prog => progressCount++));

                Assert.IsNotNull(sTextPage, "The structured text page is null.");
                Assert.IsTrue(sTextPage.Count > 0, "The structured text page is empty.");
            }
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public async Task MuPDFDocumentStructuredTextPageGetterWithOCRAsyncCancellation()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                await Assert.ThrowsExceptionAsync<PlatformNotSupportedException>(async () => await document.GetStructuredTextPageAsync(0, new TesseractLanguage("eng.traineddata"), cancellationToken: cancellationTokenSource.Token), "Providing a cancellation token should throw a PlatformNotSupportedException on Windows x86.");
            }
            else
            {
                await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await document.GetStructuredTextPageAsync(0, new TesseractLanguage("eng.traineddata"), cancellationToken: cancellationTokenSource.Token, progress: new Progress<OCRProgressInfo>(prog => cancellationTokenSource.Cancel())), "The expected OperationCanceledException was not thrown.");
            }
        }

        [TestMethod]
        public void MuPDFDocumentTextExtraction()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            string extractedText = document.ExtractText();

            Assert.IsFalse(string.IsNullOrEmpty(extractedText), "The extracted text is empty.");
            Assert.IsTrue(extractedText.Length > 10, "The extracted text is too short.");
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public void MuPDFDocumentTextExtractionWithOCR()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            string extractedText = document.ExtractText(new TesseractLanguage("eng.traineddata"));

            Assert.IsFalse(string.IsNullOrEmpty(extractedText), "The extracted text is empty.");
            Assert.IsTrue(extractedText.Length > 10, "The extracted text is too short.");
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public async Task MuPDFDocumentTextExtractionWithOCRAsync()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            string extractedText = await document.ExtractTextAsync(new TesseractLanguage("eng.traineddata"));

            Assert.IsFalse(string.IsNullOrEmpty(extractedText), "The extracted text is empty.");
            Assert.IsTrue(extractedText.Length > 10, "The extracted text is too short.");
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public async Task MuPDFDocumentTextExtractionWithOCRAsyncProgress()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            int progressCount = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                await Assert.ThrowsExceptionAsync<PlatformNotSupportedException>(async () => await document.ExtractTextAsync(new TesseractLanguage("eng.traineddata"), progress: new Progress<OCRProgressInfo>(prog => progressCount++)), "Providing a progress callback should throw a PlatformNotSupportedException on Windows x86.");
            }
            else
            {
                string extractedText = await document.ExtractTextAsync(new TesseractLanguage("eng.traineddata"), progress: new Progress<OCRProgressInfo>(prog => progressCount++));

                Assert.IsFalse(string.IsNullOrEmpty(extractedText), "The extracted text is empty.");
                Assert.IsTrue(extractedText.Length > 10, "The extracted text is too short.");
                Assert.IsTrue(progressCount > 0, "The progress callback was not called.");
            }
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public async Task MuPDFDocumentTextExtractionWithOCRAsyncCancellation()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PNG);

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                await Assert.ThrowsExceptionAsync<PlatformNotSupportedException>(async () => await document.ExtractTextAsync(new TesseractLanguage("eng.traineddata"), cancellationToken: cancellationTokenSource.Token), "Providing a cancellation token should throw a PlatformNotSupportedException on Windows x86.");
            }
            else
            {
                await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () => await document.ExtractTextAsync(new TesseractLanguage("eng.traineddata"), cancellationToken: cancellationTokenSource.Token, progress: new Progress<OCRProgressInfo>(prog => cancellationTokenSource.Cancel())), "The expected OperationCanceledException was not thrown.");
            }
        }

        [TestMethod]
        [DeploymentItem("Data/Sample-user.pdf")]
        public void MuPDFDecryptDocument()
        {
            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, "Sample-user.pdf");

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");
            
            Assert.AreEqual(EncryptionState.Encrypted, document.EncryptionState, "The document is not encrypted.");
            Assert.IsFalse(document.TryUnlock("wrongpsw", out PasswordTypes pwType), "The wrong password should not be able to unlock the document.");
            Assert.AreEqual(PasswordTypes.User, pwType, "A user password should be required.");
            Assert.ThrowsException<DocumentLockedException>(() => document.Pages[0].ToString(), "Accessing a page on an encrypted document should fail.");
            Assert.IsTrue(document.TryUnlock("userpsw"), "The correct password should unlock the document.");
            Assert.AreEqual(EncryptionState.Unlocked, document.EncryptionState, "The document should now be unlocked.");
            Assert.IsNotNull(document.Pages[0].ToString(), "Accessing a page after the document has been unlocked should work.");
        }

        [TestMethod]
        [DeploymentItem("Data/Sample-owner.pdf")]
        public void MuPDFRemoveDocumentRestrictions()
        {
            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, "Sample-owner.pdf");

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");

            Assert.AreEqual(RestrictionState.Restricted, document.RestrictionState, "The document is not restricted.");
            Assert.IsFalse(document.TryUnlock("wrongpsw", out PasswordTypes pwType), "The wrong password should not be able to unlock the document.");
            Assert.AreEqual(PasswordTypes.Owner, pwType, "An owner password should be required.");
            Assert.IsTrue(document.TryUnlock("ownerpsw"), "The correct password should unlock the document.");
            Assert.AreEqual(RestrictionState.Unlocked, document.RestrictionState, "The document should now be unlocked.");
        }

        [TestMethod]
        [DeploymentItem("Data/Sample-user-owner.pdf")]
        public void MuPDFDecryptAndRemoveDocumentRestrictions()
        {
            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, "Sample-user-owner.pdf");

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");

            Assert.AreEqual(RestrictionState.Restricted, document.RestrictionState, "The document is not restricted.");
            Assert.AreEqual(EncryptionState.Encrypted, document.EncryptionState, "The document is not encrypted.");
            Assert.IsFalse(document.TryUnlock("wrongpsw", out PasswordTypes pwType), "The wrong password should not be able to unlock the document.");
            Assert.AreEqual(PasswordTypes.Owner | PasswordTypes.User, pwType, "Both a user and an owner password should be required.");
            Assert.ThrowsException<DocumentLockedException>(() => document.Pages[0].ToString(), "Accessing a page on an encrypted document should fail.");

            Assert.IsTrue(document.TryUnlock("userpsw"), "The user password should decrypt the document.");
            Assert.AreEqual(EncryptionState.Unlocked, document.EncryptionState, "The document should now be decrypted.");
            Assert.AreEqual(RestrictionState.Restricted, document.RestrictionState, "The document should still be restricted.");
            Assert.IsNotNull(document.Pages[0].ToString(), "Accessing a page after the document has been decrypted should work.");

            Assert.IsFalse(document.TryUnlock("wrongpsw", out pwType), "The wrong password should not be able to unlock the document.");
            Assert.AreEqual(PasswordTypes.Owner, pwType, "Only an owner password should now be required.");
            
            Assert.IsTrue(document.TryUnlock("ownerpsw"), "The owner password should unlock the document.");
            Assert.AreEqual(RestrictionState.Unlocked, document.RestrictionState, "The document should now be unlocked.");
        }

        [TestMethod]
        [DeploymentItem("Data/Sample-user-owner.pdf")]
        public void MuPDFRemoveDocumentRestrictionsAndDecrypt()
        {
            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, "Sample-user-owner.pdf");

            Assert.IsNotNull(document, "The created document is null.");
            Assert.AreNotEqual(IntPtr.Zero, document.NativeDocument, "The native document pointer is null.");
            Assert.AreEqual(72, document.ImageXRes, "The image x resolution is wrong.");
            Assert.AreEqual(72, document.ImageYRes, "The image x resolution is wrong.");

            Assert.AreEqual(RestrictionState.Restricted, document.RestrictionState, "The document is not restricted.");
            Assert.AreEqual(EncryptionState.Encrypted, document.EncryptionState, "The document is not encrypted.");
            Assert.IsFalse(document.TryUnlock("wrongpsw", out PasswordTypes pwType), "The wrong password should not be able to unlock the document.");
            Assert.AreEqual(PasswordTypes.Owner | PasswordTypes.User, pwType, "Both a user and an owner password should be required.");
            Assert.ThrowsException<DocumentLockedException>(() => document.Pages[0].ToString(), "Accessing a page on an encrypted document should fail.");

            Assert.IsTrue(document.TryUnlock("ownerpsw"), "The owner password should unlock the document.");
            Assert.AreEqual(RestrictionState.Unlocked, document.RestrictionState, "The document should now be unlocked.");
            Assert.AreEqual(EncryptionState.Encrypted, document.EncryptionState, "The document should still be encrypted.");
            Assert.ThrowsException<DocumentLockedException>(() => document.Pages[0].ToString(), "Accessing a page on an encrypted document should still fail.");

            Assert.IsFalse(document.TryUnlock("wrongpsw", out pwType), "The wrong password should not be able to unlock the document.");
            Assert.AreEqual(PasswordTypes.User, pwType, "Only a user password should now be required.");
            Assert.ThrowsException<DocumentLockedException>(() => document.Pages[0].ToString(), "Accessing a page on an encrypted document should still fail.");

            Assert.IsTrue(document.TryUnlock("userpsw"), "The user password should decrypt the document.");
            Assert.AreEqual(EncryptionState.Unlocked, document.EncryptionState, "The document should now be decrypted.");
            Assert.AreEqual(RestrictionState.Unlocked, document.RestrictionState, "The document should still be unlocked.");
            Assert.IsNotNull(document.Pages[0].ToString(), "Accessing a page after the document has been decrypted should work.");
        }
    }
}
