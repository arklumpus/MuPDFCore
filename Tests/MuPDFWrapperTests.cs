using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using MuPDFCore.StructuredText;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable IDE0090 // Use 'new(...)'
#pragma warning disable IDE0230 // Use UTF-8 string literal

namespace Tests
{
    [TestClass]
    public class MuPDFWrapperTests
    {
        [TestMethod]
        public void ContextCreationWithDefaultStoreSize()
        {
            IntPtr nativeContext = IntPtr.Zero;

            int result = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateContext returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativeContext, "The native context pointer is null.");

            try
            {
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void ContextCreationWithEmptyStoreSize()
        {
            IntPtr nativeContext = IntPtr.Zero;

            int result = NativeMethods.CreateContext(0, ref nativeContext);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateContext returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativeContext, "The native context pointer is null.");

            try
            {
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void ContextDisposal()
        {
            IntPtr nativeContext = IntPtr.Zero;
            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result = NativeMethods.DisposeContext(nativeContext);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "DisposeContext returned the wrong exit code.");
        }

        [TestMethod]
        public void ContextCloning()
        {
            IntPtr nativeContext = IntPtr.Zero;
            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            IntPtr[] contexts = new IntPtr[4];
            GCHandle contextsHandle = GCHandle.Alloc(contexts, GCHandleType.Pinned);

            int result = NativeMethods.CloneContext(nativeContext, 4, contextsHandle.AddrOfPinnedObject());

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CloneContext returned the wrong exit code.");
            CollectionAssert.DoesNotContain(contexts, IntPtr.Zero, "One or more of the cloned context pointers are null.");

            for (int i = 0; i < contexts.Length; i++)
            {
                _ = NativeMethods.DisposeContext(contexts[i]);
            }

            contextsHandle.Free();
            _ = NativeMethods.DisposeContext(nativeContext);
        }

        [TestMethod]
        public void CurrentStoreSizeGetter()
        {
            IntPtr nativeContext = IntPtr.Zero;

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);
            ulong result = NativeMethods.GetCurrentStoreSize(nativeContext);

            Assert.AreEqual((ulong)0, result, "GetCurrentStoreSize returned the wrong store size.");

            try
            {
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void MaxStoreSizeGetter()
        {
            IntPtr nativeContext = IntPtr.Zero;

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);
            ulong result = NativeMethods.GetMaxStoreSize(nativeContext);

            Assert.AreEqual((ulong)(256 << 20), result, "GetMaxStoreSize returned the wrong store size.");

            try
            {
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StoreShrinkageWhenEmpty()
        {
            IntPtr nativeContext = IntPtr.Zero;

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);
            int result = NativeMethods.ShrinkStore(nativeContext, (uint)50);

            Assert.AreEqual(1, result, "ShrinkStore returned the wrong exit code.");

            try
            {
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StoreEmptyingWhenEmpty()
        {
            IntPtr nativeContext = IntPtr.Zero;

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);
            NativeMethods.EmptyStore(nativeContext);

            ulong result = NativeMethods.GetCurrentStoreSize(nativeContext);

            Assert.AreEqual((ulong)0, result, "The size of the store is not 0.");

            try
            {
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        [DeploymentItem("Data/Sample.pdf")]
        public void DocumentCreationFromPDFFile()
        {
            IntPtr nativeContext = IntPtr.Zero;
            string fileName = "Sample.pdf";
            IntPtr nativeDocument = IntPtr.Zero;
            int pageCount = -1;
            float xRes = 0;
            float yRes = 0;

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            {
                result = NativeMethods.CreateDocumentFromFile(nativeContext, encodedFileName.Address, 0, ref nativeDocument, ref pageCount, ref xRes, ref yRes);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateDocumentFromFile returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativeDocument, "The native document pointer is null.");
            Assert.AreEqual(2, pageCount, "The page count is wrong.");
            Assert.AreEqual(-1, xRes, "The x resolution is wrong.");
            Assert.AreEqual(-1, yRes, "The y resolution is wrong.");

            try
            {
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void DocumentCreationFromPDFStream()
        {
            IntPtr nativeContext = IntPtr.Zero;
            Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            IntPtr nativeDocument = IntPtr.Zero;
            IntPtr nativeStream = IntPtr.Zero;
            int pageCount = -1;
            float xRes = 0;
            float yRes = 0;

            int origin = (int)pdfDataStream.Seek(0, SeekOrigin.Begin);
            ulong dataLength = (ulong)pdfDataStream.Length;
            MemoryStream ms = new MemoryStream((int)pdfDataStream.Length);
            pdfDataStream.CopyTo(ms);
            byte[] dataBytes = ms.GetBuffer();
            pdfDataStream.Dispose();

            GCHandle dataHandle = GCHandle.Alloc(dataBytes, GCHandleType.Pinned);
            IntPtr dataAddress = IntPtr.Add(dataHandle.AddrOfPinnedObject(), origin);

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result = NativeMethods.CreateDocumentFromStream(nativeContext, dataAddress, dataLength, ".pdf", 0, ref nativeDocument, ref nativeStream, ref pageCount, ref xRes, ref yRes);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateDocumentFromStream returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativeDocument, "The native document pointer is null.");
            Assert.AreEqual(2, pageCount, "The page count is wrong.");
            Assert.AreEqual(-1, xRes, "The x resolution is wrong.");
            Assert.AreEqual(-1, yRes, "The y resolution is wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        [DeploymentItem("Data/Sample.png")]
        public void DocumentCreationFromPNGFile()
        {
            IntPtr nativeContext = IntPtr.Zero;
            string fileName = "Sample.png";
            IntPtr nativeDocument = IntPtr.Zero;
            int pageCount = -1;
            float xRes = 0;
            float yRes = 0;

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            {
                result = NativeMethods.CreateDocumentFromFile(nativeContext, encodedFileName.Address, 1, ref nativeDocument, ref pageCount, ref xRes, ref yRes);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateDocumentFromFile returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativeDocument, "The native document pointer is null.");
            Assert.AreEqual(1, pageCount, "The page count is wrong.");
            Assert.AreEqual(96, xRes, "The x resolution is wrong.");
            Assert.AreEqual(96, yRes, "The y resolution is wrong.");

            try
            {
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void DocumentCreationFromPNGStream()
        {
            IntPtr nativeContext = IntPtr.Zero;
            Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            IntPtr nativeDocument = IntPtr.Zero;
            IntPtr nativeStream = IntPtr.Zero;
            int pageCount = -1;
            float xRes = 0;
            float yRes = 0;

            int origin = (int)pdfDataStream.Seek(0, SeekOrigin.Begin);
            ulong dataLength = (ulong)pdfDataStream.Length;
            MemoryStream ms = new MemoryStream((int)pdfDataStream.Length);
            pdfDataStream.CopyTo(ms);
            byte[] dataBytes = ms.GetBuffer();
            pdfDataStream.Dispose();

            GCHandle dataHandle = GCHandle.Alloc(dataBytes, GCHandleType.Pinned);
            IntPtr dataAddress = IntPtr.Add(dataHandle.AddrOfPinnedObject(), origin);

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result = NativeMethods.CreateDocumentFromStream(nativeContext, dataAddress, dataLength, ".png", 1, ref nativeDocument, ref nativeStream, ref pageCount, ref xRes, ref yRes);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateDocumentFromStream returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativeDocument, "The native document pointer is null.");
            Assert.AreEqual(1, pageCount, "The page count is wrong.");
            Assert.AreEqual(96, xRes, "The x resolution is wrong.");
            Assert.AreEqual(96, yRes, "The y resolution is wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) CreateSampleDocument(string resource = "Tests.Data.Sample.pdf")
        {
            IntPtr nativeContext = IntPtr.Zero;
            Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            IntPtr nativeDocument = IntPtr.Zero;
            IntPtr nativeStream = IntPtr.Zero;
            int pageCount = -1;
            float xRes = 0;
            float yRes = 0;

            int origin = (int)pdfDataStream.Seek(0, SeekOrigin.Begin);
            ulong dataLength = (ulong)pdfDataStream.Length;
            MemoryStream ms = new MemoryStream((int)pdfDataStream.Length);
            pdfDataStream.CopyTo(ms);
            byte[] dataBytes = ms.GetBuffer();
            pdfDataStream.Dispose();

            GCHandle dataHandle = GCHandle.Alloc(dataBytes, GCHandleType.Pinned);
            IntPtr dataAddress = IntPtr.Add(dataHandle.AddrOfPinnedObject(), origin);

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            _ = NativeMethods.CreateDocumentFromStream(nativeContext, dataAddress, dataLength, ".pdf", 0, ref nativeDocument, ref nativeStream, ref pageCount, ref xRes, ref yRes);

            return (dataHandle, ms, nativeContext, nativeDocument, nativeStream);
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) CreateSamplePNGDocument()
        {
            IntPtr nativeContext = IntPtr.Zero;
            Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.png");
            IntPtr nativeDocument = IntPtr.Zero;
            IntPtr nativeStream = IntPtr.Zero;
            int pageCount = -1;
            float xRes = 0;
            float yRes = 0;

            int origin = (int)pdfDataStream.Seek(0, SeekOrigin.Begin);
            ulong dataLength = (ulong)pdfDataStream.Length;
            MemoryStream ms = new MemoryStream((int)pdfDataStream.Length);
            pdfDataStream.CopyTo(ms);
            byte[] dataBytes = ms.GetBuffer();
            pdfDataStream.Dispose();

            GCHandle dataHandle = GCHandle.Alloc(dataBytes, GCHandleType.Pinned);
            IntPtr dataAddress = IntPtr.Add(dataHandle.AddrOfPinnedObject(), origin);

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            _ = NativeMethods.CreateDocumentFromStream(nativeContext, dataAddress, dataLength, ".png", 0, ref nativeDocument, ref nativeStream, ref pageCount, ref xRes, ref yRes);

            return (dataHandle, ms, nativeContext, nativeDocument, nativeStream);
        }

        [TestMethod]
        public void DocumentDisposal()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            int result = NativeMethods.DisposeDocument(nativeContext, nativeDocument);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "DisposeDocument returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StreamDisposal()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            int result = NativeMethods.DisposeStream(nativeContext, nativeStream);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "DisposeStream returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void PageLoading()
        {
            IntPtr nativePage = IntPtr.Zero;

            float x = -1;
            float y = -1;
            float w = -1;
            float h = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            int result = NativeMethods.LoadPage(nativeContext, nativeDocument, 0, ref nativePage, ref x, ref y, ref w, ref h);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "LoadPage returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativePage, "The native page pointer is null.");
            Assert.AreEqual(0, x, "The page x coordinate is wrong.");
            Assert.AreEqual(0, y, "The page y coordinate is wrong.");
            Assert.AreEqual(4000, w, "The page width is wrong.");
            Assert.AreEqual(2600, h, "The page height is wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x, float y, float w, float h) CreateSamplePage(string resource = "Tests.Data.Sample.pdf")
        {
            IntPtr nativePage = IntPtr.Zero;

            float x = -1;
            float y = -1;
            float w = -1;
            float h = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument(resource);

            _ = NativeMethods.LoadPage(nativeContext, nativeDocument, 0, ref nativePage, ref x, ref y, ref w, ref h);

            return (dataHandle, ms, nativePage, nativeDocument, nativeStream, nativeContext, x, y, w, h);
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x, float y, float w, float h) CreateSamplePNGPage()
        {
            IntPtr nativePage = IntPtr.Zero;

            float x = -1;
            float y = -1;
            float w = -1;
            float h = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSamplePNGDocument();

            _ = NativeMethods.LoadPage(nativeContext, nativeDocument, 0, ref nativePage, ref x, ref y, ref w, ref h);

            return (dataHandle, ms, nativePage, nativeDocument, nativeStream, nativeContext, x, y, w, h);
        }

        [TestMethod]
        public void PageDisposal()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSamplePage();

            int result = NativeMethods.DisposePage(nativeContext, nativePage);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "DisposePage returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void DisplayListGetter()
        {
            IntPtr nativeDisplayList = IntPtr.Zero;

            float x0 = -1;
            float y0 = -1;
            float x1 = -1;
            float y1 = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSamplePage();

            int result = NativeMethods.GetDisplayList(nativeContext, nativePage, 1, ref nativeDisplayList, ref x0, ref y0, ref x1, ref y1);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetDisplayList returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, nativeDisplayList, "The native display list pointer is null.");
            Assert.AreEqual(0, x0, "The display list left coordinate is wrong.");
            Assert.AreEqual(0, y0, "The display list top coordinate is wrong.");
            Assert.AreEqual(4000, x1, "The display list right is wrong.");
            Assert.AreEqual(2600, y1, "The display list bottom is wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) CreateSampleDisplayList(string resource = "Tests.Data.Sample.pdf")
        {
            IntPtr nativeDisplayList = IntPtr.Zero;

            float x0 = -1;
            float y0 = -1;
            float x1 = -1;
            float y1 = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSamplePage(resource);

            _ = NativeMethods.GetDisplayList(nativeContext, nativePage, 1, ref nativeDisplayList, ref x0, ref y0, ref x1, ref y1);

            return (dataHandle, ms, nativeDisplayList, nativePage, nativeDocument, nativeStream, nativeContext, x0, y0, x1, y1);
        }
        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) CreateSamplePNGDisplayList()
        {
            IntPtr nativeDisplayList = IntPtr.Zero;

            float x0 = -1;
            float y0 = -1;
            float x1 = -1;
            float y1 = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSamplePNGPage();

            _ = NativeMethods.GetDisplayList(nativeContext, nativePage, 1, ref nativeDisplayList, ref x0, ref y0, ref x1, ref y1);

            return (dataHandle, ms, nativeDisplayList, nativePage, nativeDocument, nativeStream, nativeContext, x0, y0, x1, y1);
        }

        [TestMethod]
        public void DisplayListDisposal()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSampleDisplayList();

            int result = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "DisposeDisplayList returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void SubDisplayListRenderingRGB()
        {
            int bufferSize = 4000 * 2600 * 3;
            byte[] buffer = new byte[bufferSize];

            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPointer = bufferHandle.AddrOfPinnedObject();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result = NativeMethods.RenderSubDisplayList(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, bufferPointer, IntPtr.Zero);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "RenderSubDisplayList returned the wrong exit code.");

            CollectionAssert.AreEqual(new byte[] { 0xF5, 0xF9, 0xFF }, buffer[0..3], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xF5, 0xF9, 0xFF }, buffer[^3..^0], "The end of the rendered image appears to be wrong.");

            try
            {
                bufferHandle.Free();
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void SubDisplayListRenderingBGRA()
        {
            int bufferSize = 4000 * 2600 * 4;
            byte[] buffer = new byte[bufferSize];

            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPointer = bufferHandle.AddrOfPinnedObject();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result = NativeMethods.RenderSubDisplayList(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 3, bufferPointer, IntPtr.Zero);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "RenderSubDisplayList returned the wrong exit code.");

            CollectionAssert.AreEqual(new byte[] { 0x0B, 0x05, 0x01, 0x0B }, buffer[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x0B, 0x05, 0x01, 0x0B }, buffer[^4..^0], "The end of the rendered image appears to be wrong.");

            try
            {
                bufferHandle.Free();
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        private static (GCHandle bufferHandle, GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) RenderSampleDisplayList()
        {
            int bufferSize = 4000 * 2600 * 3;
            byte[] buffer = new byte[bufferSize];

            GCHandle bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            IntPtr bufferPointer = bufferHandle.AddrOfPinnedObject();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            _ = NativeMethods.RenderSubDisplayList(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, bufferPointer, IntPtr.Zero);


            return (bufferHandle, dataHandle, ms, nativeDisplayList, nativePage, nativeDocument, nativeStream, nativeContext);
        }

        [TestMethod]
        public void CurrentStoreSizeGetterWithOpenDocument()
        {
            (GCHandle bufferHandle, GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = RenderSampleDisplayList();

            ulong result = NativeMethods.GetCurrentStoreSize(nativeContext);

            Assert.IsTrue(result > 0, "GetCurrentStoreSize returned the wrong store size.");

            try
            {
                bufferHandle.Free();
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StoreShrinkageWithOpenDocument()
        {
            (GCHandle bufferHandle, GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = RenderSampleDisplayList();

            ulong preSize = NativeMethods.GetCurrentStoreSize(nativeContext);

            int result = NativeMethods.ShrinkStore(nativeContext, (uint)75);
            Assert.AreEqual(1, result, "ShrinkStore returned the wrong exit code.");

            ulong postSize = NativeMethods.GetCurrentStoreSize(nativeContext);

            Assert.IsTrue(postSize <= preSize, "The store has not been shrunk.");
            Assert.IsTrue(postSize <= Math.Ceiling(preSize * 0.75), "The store has not been shrunk by the required amount.");

            try
            {
                bufferHandle.Free();
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StoreEmptyingWithOpenDocument()
        {
            (GCHandle bufferHandle, GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = RenderSampleDisplayList();

            NativeMethods.EmptyStore(nativeContext);

            ulong result = NativeMethods.GetCurrentStoreSize(nativeContext);

            Assert.AreEqual((ulong)0, result, "The size of the store is not 0.");

            try
            {
                bufferHandle.Free();
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void ImageSavingAsPNM()
        {
            string tempFile = Path.GetTempFileName();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(tempFile))
            {
                result = NativeMethods.SaveImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, encodedFileName.Address, 0, 90);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveImage returned the wrong exit code.");
            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] actualBytes = File.ReadAllBytes(tempFile);

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x36, 0x0A, 0x34 }, actualBytes[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xF5, 0xF9, 0xFF }, actualBytes[^4..^0], "The end of the rendered image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                File.Delete(tempFile);
            }
            catch { }
        }

        [TestMethod]
        public void ImageSavingAsPAM()
        {
            string tempFile = Path.GetTempFileName();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(tempFile))
            {
                result = NativeMethods.SaveImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 1, encodedFileName.Address, 1, 90);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveImage returned the wrong exit code.");
            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] actualBytes = File.ReadAllBytes(tempFile);

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x37, 0x0A, 0x57 }, actualBytes[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x74, 0xFF, 0x0B }, actualBytes[^4..^0], "The end of the rendered image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                File.Delete(tempFile);
            }
            catch { }
        }

        [TestMethod]
        public void ImageSavingAsPNG()
        {
            string tempFile = Path.GetTempFileName();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(tempFile))
            {
                result = NativeMethods.SaveImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 1, encodedFileName.Address, 2, 90);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveImage returned the wrong exit code.");
            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] actualBytes = File.ReadAllBytes(tempFile);

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, actualBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, actualBytes[^4..^0], "The end of the saved image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                File.Delete(tempFile);
            }
            catch { }
        }

        [TestMethod]
        public void ImageSavingAsPSD()
        {
            string tempFile = Path.GetTempFileName();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(tempFile))
            {
                result = NativeMethods.SaveImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, encodedFileName.Address, 3, 90);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveImage returned the wrong exit code.");
            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] actualBytes = File.ReadAllBytes(tempFile);

            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53, }, actualBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, actualBytes[^4..^0], "The end of the saved image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                File.Delete(tempFile);
            }
            catch { }
        }

        [TestMethod]
        public void ImageSavingAsJPEG()
        {
            string tempFile = Path.GetTempFileName();

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(tempFile))
            {
                result = NativeMethods.SaveImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, encodedFileName.Address, 4, 50);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveImage returned the wrong exit code.");
            Assert.IsTrue(File.Exists(tempFile), "The output file has not been created.");

            byte[] actualBytes = File.ReadAllBytes(tempFile);

            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, actualBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD9 }, actualBytes[^2..^0], "The end of the saved image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                File.Delete(tempFile);
            }
            catch { }
        }

        [TestMethod]
        public void ImageWritingAsPNM()
        {
            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result = NativeMethods.WriteImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, 0, 90, ref outputBuffer, ref outputData, ref outputDataLength);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteImage returned the wrong exit code.");

            byte[] actualBytes = new byte[(int)outputDataLength];

            Marshal.Copy(outputData, actualBytes, 0, actualBytes.Length);

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x36, 0x0A, 0x34 }, actualBytes[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xF5, 0xF9, 0xFF }, actualBytes[^4..^0], "The end of the rendered image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeBuffer(nativeContext, outputBuffer);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void ImageWritingAsPAM()
        {
            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result = NativeMethods.WriteImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 1, 1, 90, ref outputBuffer, ref outputData, ref outputDataLength);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteImage returned the wrong exit code.");

            byte[] actualBytes = new byte[(int)outputDataLength];

            Marshal.Copy(outputData, actualBytes, 0, actualBytes.Length);

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x37, 0x0A, 0x57 }, actualBytes[0..4], "The start of the rendered image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x17, 0x74, 0xFF, 0x0B }, actualBytes[^4..^0], "The end of the rendered image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeBuffer(nativeContext, outputBuffer);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void ImageWritingAsPNG()
        {
            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result = NativeMethods.WriteImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, 2, 90, ref outputBuffer, ref outputData, ref outputDataLength);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteImage returned the wrong exit code.");

            byte[] actualBytes = new byte[(int)outputDataLength];

            Marshal.Copy(outputData, actualBytes, 0, actualBytes.Length);

            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, actualBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xAE, 0x42, 0x60, 0x82 }, actualBytes[^4..^0], "The end of the saved image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeBuffer(nativeContext, outputBuffer);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void ImageWritingAsPSD()
        {
            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result = NativeMethods.WriteImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, 3, 90, ref outputBuffer, ref outputData, ref outputDataLength);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteImage returned the wrong exit code.");

            byte[] actualBytes = new byte[(int)outputDataLength];

            Marshal.Copy(outputData, actualBytes, 0, actualBytes.Length);

            CollectionAssert.AreEqual(new byte[] { 0x38, 0x42, 0x50, 0x53, }, actualBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, actualBytes[^4..^0], "The end of the saved image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeBuffer(nativeContext, outputBuffer);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void ImageWritingAsJPEG()
        {
            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            int result = NativeMethods.WriteImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, 4, 50, ref outputBuffer, ref outputData, ref outputDataLength);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteImage returned the wrong exit code.");

            byte[] actualBytes = new byte[(int)outputDataLength];

            Marshal.Copy(outputData, actualBytes, 0, actualBytes.Length);

            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, actualBytes[0..4], "The start of the saved image appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD9 }, actualBytes[^2..^0], "The end of the saved image appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeBuffer(nativeContext, outputBuffer);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void BufferDisposal()
        {
            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            _ = NativeMethods.WriteImage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, 0, 2, 90, ref outputBuffer, ref outputData, ref outputDataLength);

            int result = NativeMethods.DisposeBuffer(nativeContext, outputBuffer);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "DisposeBuffer returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void DocumentWriterCreationPDF()
        {
            IntPtr nativeContext = IntPtr.Zero;
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            using (UTF8EncodedString encodedOptions = new UTF8EncodedString(new PDFCreationOptions().GetOptionString()))
            {
                result = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 0, encodedOptions.Address, ref documentWriter);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateDocumentWriter for PDF returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, documentWriter, "The native document writer pointer for PDF is null.");
            Assert.IsTrue(File.Exists(fileName), "The output file for the PDF document writer has not been created.");

            try
            {
                _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void DocumentWriterCreationSVG()
        {
            IntPtr nativeContext = IntPtr.Zero;
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            using (UTF8EncodedString encodedOptions = new UTF8EncodedString(new SVGCreationOptions().GetOptionString()))
            {
                result = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 1, encodedOptions.Address, ref documentWriter);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateDocumentWriter for SVG returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, documentWriter, "The native document writer pointer for SVG is null.");
            Assert.IsTrue(File.Exists(fileName), "The output file for the SVG document writer has not been created.");

            try
            {
                _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void DocumentWriterCreationCBZ()
        {
            IntPtr nativeContext = IntPtr.Zero;
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            int result;

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            using (UTF8EncodedString encodedOptions = new UTF8EncodedString(new CBZCreationOptions().GetOptionString()))
            {
                result = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 2, encodedOptions.Address, ref documentWriter);
            }

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "CreateDocumentWriter for CBZ returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, documentWriter, "The native document writer pointer for CBZ is null.");
            Assert.IsTrue(File.Exists(fileName), "The output file for the CBZ document writer has not been created.");

            try
            {
                _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        private (IntPtr documentWriter, string fileName) CreateDocumentWriterPDF(IntPtr nativeContext)
        {
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            using (UTF8EncodedString encodedOptions = new UTF8EncodedString(new PDFCreationOptions().GetOptionString()))
            {
                _ = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 0, encodedOptions.Address, ref documentWriter);
            }

            return (documentWriter, fileName);
        }

        private (IntPtr documentWriter, string fileName) CreateDocumentWriterSVG(IntPtr nativeContext)
        {
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            using (UTF8EncodedString encodedOptions = new UTF8EncodedString(new SVGCreationOptions().GetOptionString()))
            {
                _ = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 1, encodedOptions.Address, ref documentWriter);
            }

            return (documentWriter, fileName);
        }

        private (IntPtr documentWriter, string fileName) CreateDocumentWriterCBZ(IntPtr nativeContext)
        {
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            using (UTF8EncodedString encodedOptions = new UTF8EncodedString(new CBZCreationOptions().GetOptionString()))
            {
                _ = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 2, encodedOptions.Address, ref documentWriter);
            }

            return (documentWriter, fileName);
        }

        [TestMethod]
        public void SubDisplayListAsPageWritingPDF()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            (IntPtr documentWriter, string fileName) = CreateDocumentWriterPDF(nativeContext);

            int result = NativeMethods.WriteSubDisplayListAsPage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, documentWriter);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteSubDisplayListAsPage for PDF returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void SubDisplayListAsPageWritingSVG()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            (IntPtr documentWriter, string fileName) = CreateDocumentWriterSVG(nativeContext);

            int result = NativeMethods.WriteSubDisplayListAsPage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, documentWriter);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteSubDisplayListAsPage for SVG returned the wrong exit code.");

            string tempFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "1" + Path.GetExtension(fileName));

            Assert.IsTrue(File.Exists(tempFileName), "The output file for the SVG document writer has not been created.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void SubDisplayListAsPageWritingCBZ()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            (IntPtr documentWriter, string fileName) = CreateDocumentWriterCBZ(nativeContext);

            int result = NativeMethods.WriteSubDisplayListAsPage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, documentWriter);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "WriteSubDisplayListAsPage for CBZ returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr documentWriter, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, string fileName) WriteSubDisplayListAsPage(Func<IntPtr, (IntPtr documentWriter, string fileName)> createDocumentWriter)
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSampleDisplayList();

            (IntPtr documentWriter, string fileName) = createDocumentWriter(nativeContext);

            _ = NativeMethods.WriteSubDisplayListAsPage(nativeContext, nativeDisplayList, x0, y0, x1, y1, 1, documentWriter);

            return (dataHandle, ms, documentWriter, nativeDisplayList, nativePage, nativeDocument, nativeStream, nativeContext, fileName);
        }

        [TestMethod]
        public void DocumentWriterFinalizationPDF()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr documentWriter, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, string fileName) = WriteSubDisplayListAsPage(CreateDocumentWriterPDF);

            int result = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "FinalizeDocumentWriter for PDF returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void DocumentWriterFinalizationSVG()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr documentWriter, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, string fileName) = WriteSubDisplayListAsPage(CreateDocumentWriterSVG);

            int result = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "FinalizeDocumentWriter for SVG returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void DocumentWriterFinalizationCBZ()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr documentWriter, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, string fileName) = WriteSubDisplayListAsPage(CreateDocumentWriterCBZ);

            int result = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "FinalizeDocumentWriter for CBZ returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void DocumentCreationPDF()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr documentWriter, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, string fileName) = WriteSubDisplayListAsPage(CreateDocumentWriterPDF);

            _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);

            byte[] actualBytes = File.ReadAllBytes(fileName);

            CollectionAssert.AreEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }, actualBytes[0..4], "The start of the created document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x45, 0x4F, 0x46, 0x0A }, actualBytes[^4..^0], "The end of the created document appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void DocumentCreationSVG()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr documentWriter, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, string fileName) = WriteSubDisplayListAsPage(CreateDocumentWriterSVG);

            _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);

            string tempFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "1" + Path.GetExtension(fileName));
            byte[] actualBytes = File.ReadAllBytes(tempFileName);

            CollectionAssert.AreEqual(new byte[] { 0x3C, 0x73, 0x76, 0x67 }, actualBytes[0..4], "The start of the created document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x76, 0x67, 0x3E, 0x0A }, actualBytes[^4..^0], "The end of the created document appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (File.Exists(tempFileName))
                {
                    File.Delete(tempFileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void DocumentCreationCBZ()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr documentWriter, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, string fileName) = WriteSubDisplayListAsPage(CreateDocumentWriterCBZ);

            _ = NativeMethods.FinalizeDocumentWriter(nativeContext, documentWriter);

            byte[] actualBytes = File.ReadAllBytes(fileName);

            CollectionAssert.AreEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }, actualBytes[0..4], "The start of the document appears to be wrong.");
            CollectionAssert.AreEqual(new byte[] { 0x75, 0x50, 0x44, 0x46 }, actualBytes[^4..^0], "The end of the document appears to be wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            catch { }
        }

        [TestMethod]
        public void StructuredTextPageGetter()
        {
            IntPtr nativeSTextPage = IntPtr.Zero;
            int sTextBlockCount = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSampleDisplayList();

            int result = NativeMethods.GetStructuredTextPage(nativeContext, nativeDisplayList, 1, ref nativeSTextPage, ref sTextBlockCount);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextPage returned the wrong exit code.");
            Assert.IsTrue(sTextBlockCount > 0, "The number of text blocks in the page is wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeSTextPage, int sTextBlockCount, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) CreateSampleStructuredTextPage()
        {
            IntPtr nativeSTextPage = IntPtr.Zero;
            int sTextBlockCount = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSampleDisplayList();

            _ = NativeMethods.GetStructuredTextPage(nativeContext, nativeDisplayList, (int)StructuredTextFlags.PreserveImages, ref nativeSTextPage, ref sTextBlockCount);

            return (dataHandle, ms, nativeSTextPage, sTextBlockCount, nativeDisplayList, nativePage, nativeDocument, nativeStream, nativeContext);
        }

        [TestMethod]
        public void StructuredTextPageDisposal()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeSTextPage, int _, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextPage();

            int result = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "DisposeStructuredTextPage returned the wrong exit code.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StructuredTextBlocksGetter()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeSTextPage, int sTextBlockCount, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextPage();

            IntPtr[] blockPointers = new IntPtr[sTextBlockCount];
            GCHandle blocksHandle = GCHandle.Alloc(blockPointers, GCHandleType.Pinned);

            int result = NativeMethods.GetStructuredTextBlocks(nativeSTextPage, blocksHandle.AddrOfPinnedObject());

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextBlocks returned the wrong exit code.");
            CollectionAssert.DoesNotContain(blockPointers, IntPtr.Zero, "Some structured text block pointers are zero!");

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        private static (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) CreateSampleStructuredTextBlocks()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeSTextPage, int sTextBlockCount, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextPage();

            IntPtr[] blockPointers = new IntPtr[sTextBlockCount];
            GCHandle blocksHandle = GCHandle.Alloc(blockPointers, GCHandleType.Pinned);

            _ = NativeMethods.GetStructuredTextBlocks(nativeSTextPage, blocksHandle.AddrOfPinnedObject());

            return (blocksHandle, dataHandle, ms, blockPointers, nativeSTextPage, nativeDisplayList, nativePage, nativeDocument, nativeStream, nativeContext);
        }

        [TestMethod]
        public void StructuredTextBlockGetter()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                int result = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextBlock returned the wrong exit code.");
                Assert.IsTrue(x0 >= 0, "The " + i.ToString() + "th block's left coordinate is out of range.");
                Assert.IsTrue(y0 >= 0, "The " + i.ToString() + "th block's top coordinate is out of range.");
                Assert.IsTrue(x1 >= x0, "The " + i.ToString() + "th block's right coordinate is out of range.");
                Assert.IsTrue(y1 >= y0, "The " + i.ToString() + "th block's bottom coordinate is out of range.");
                Assert.IsTrue(type == 0 || type == 1, "The " + i.ToString() + "th block's type coordinate is wrong (" + type.ToString() + ").");

                if (type == 1)
                {
                    Assert.AreNotEqual(IntPtr.Zero, image, "The " + i.ToString() + "th block's image pointer is NULL.");
                    Assert.IsTrue(a >= 0, "The " + i.ToString() + "th block's transform matrix a component is out of range.");
                    Assert.IsTrue(b >= 0, "The " + i.ToString() + "th block's transform matrix b component is out of range.");
                    Assert.IsTrue(c >= 0, "The " + i.ToString() + "th block's transform matrix c component is out of range.");
                    Assert.IsTrue(d >= 0, "The " + i.ToString() + "th block's transform matrix d component is out of range.");
                    Assert.IsTrue(e >= 0, "The " + i.ToString() + "th block's transform matrix e component is out of range.");
                    Assert.IsTrue(f >= 0, "The " + i.ToString() + "th block's transform matrix f component is out of range.");
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StructuredTextLinesGetter()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 0)
                {
                    IntPtr[] linePointers = new IntPtr[lineCount];
                    GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

                    int result = NativeMethods.GetStructuredTextLines(blockPointers[i], linesHandle.AddrOfPinnedObject());

                    Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextLines returned the wrong exit code.");
                    CollectionAssert.DoesNotContain(linePointers, IntPtr.Zero, "Some structured text line pointers are zero!");

                    try
                    {
                        linesHandle.Free();
                    }
                    catch { }
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StructuredTextLineGetter()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 0)
                {
                    IntPtr[] linePointers = new IntPtr[lineCount];
                    GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

                    _ = NativeMethods.GetStructuredTextLines(blockPointers[i], linesHandle.AddrOfPinnedObject());

                    for (int j = 0; j < lineCount; j++)
                    {
                        int wmode = -1;
                        x0 = -1;
                        y0 = -1;
                        x1 = -1;
                        y1 = -1;

                        float x = -1;
                        float y = -1;

                        int charCount = -1;

                        int result = NativeMethods.GetStructuredTextLine(linePointers[j], ref wmode, ref x0, ref y0, ref x1, ref y1, ref x, ref y, ref charCount);

                        Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextLine returned the wrong exit code.");
                        Assert.IsTrue(x0 >= 0, "The left coordinate of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                        Assert.IsTrue(y0 >= 0, "The top coordinate of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                        Assert.IsTrue(x1 >= x0, "The right coordinate of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                        Assert.IsTrue(y1 >= y0, "The bottom coordinate of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                        Assert.IsFalse(float.IsNaN(x), "The x component of the direction of the " + j.ToString() + "th line of the " + i.ToString() + "th block is NaN.");
                        Assert.IsFalse(float.IsNaN(y), "The y component of the direction of the " + j.ToString() + "th line of the " + i.ToString() + "th block is NaN.");
                        Assert.AreEqual(1, Math.Sqrt(x * x + y * y), 0.01, "The modulus of the direction of the " + j.ToString() + "th line of the " + i.ToString() + "th block is not 1.");
                        Assert.IsTrue(wmode == 0 || wmode == 1, "The writing mode for the " + j.ToString() + "th line of the " + i.ToString() + "th block is not valid (" + wmode.ToString() + ").");
                        Assert.IsTrue(charCount > 0, "The number of characters of the " + j.ToString() + "th line of the " + i.ToString() + "th block is <= 0.");
                    }

                    try
                    {
                        linesHandle.Free();
                    }
                    catch { }
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StructuredTextCharsGetter()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 0)
                {
                    IntPtr[] linePointers = new IntPtr[lineCount];
                    GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

                    _ = NativeMethods.GetStructuredTextLines(blockPointers[i], linesHandle.AddrOfPinnedObject());

                    for (int j = 0; j < lineCount; j++)
                    {
                        int wmode = -1;
                        x0 = -1;
                        y0 = -1;
                        x1 = -1;
                        y1 = -1;

                        float x = -1;
                        float y = -1;

                        int charCount = -1;

                        _ = NativeMethods.GetStructuredTextLine(linePointers[j], ref wmode, ref x0, ref y0, ref x1, ref y1, ref x, ref y, ref charCount);

                        IntPtr[] charPointers = new IntPtr[charCount];
                        GCHandle charsHandle = GCHandle.Alloc(charPointers, GCHandleType.Pinned);

                        int result = NativeMethods.GetStructuredTextChars(linePointers[j], charsHandle.AddrOfPinnedObject());

                        Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextChars returned the wrong exit code.");
                        CollectionAssert.DoesNotContain(linePointers, IntPtr.Zero, "Some structured text character pointers are zero!");

                        try
                        {
                            charsHandle.Free();
                        }
                        catch { }
                    }

                    try
                    {
                        linesHandle.Free();
                    }
                    catch { }
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StructuredTextCharGetter()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 0)
                {
                    IntPtr[] linePointers = new IntPtr[lineCount];
                    GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

                    _ = NativeMethods.GetStructuredTextLines(blockPointers[i], linesHandle.AddrOfPinnedObject());

                    for (int j = 0; j < lineCount; j++)
                    {
                        int wmode = -1;
                        x0 = -1;
                        y0 = -1;
                        x1 = -1;
                        y1 = -1;

                        float x = -1;
                        float y = -1;

                        int charCount = -1;

                        _ = NativeMethods.GetStructuredTextLine(linePointers[j], ref wmode, ref x0, ref y0, ref x1, ref y1, ref x, ref y, ref charCount);

                        IntPtr[] charPointers = new IntPtr[charCount];
                        GCHandle charsHandle = GCHandle.Alloc(charPointers, GCHandleType.Pinned);

                        _ = NativeMethods.GetStructuredTextChars(linePointers[j], charsHandle.AddrOfPinnedObject());


                        double theta = Math.Atan2(y, x);

                        while (theta < 0)
                        {
                            theta += 2 * Math.PI;
                        }

                        while (theta > 2 * Math.PI)
                        {
                            theta -= 2 * Math.PI;
                        }

                        int dir;

                        if (theta >= 0 && theta <= Math.PI / 2 || theta > 1.5 * Math.PI)
                        {
                            dir = 1;
                        }
                        else
                        {
                            dir = -1;
                        }

                        for (int k = 0; k < charCount; k++)
                        {
                            int codePoint = -1;
                            uint color = 0;
                            float originX = -1;
                            float originY = -1;
                            float size = -1;
                            float llX = -1;
                            float llY = -1;
                            float ulX = -1;
                            float ulY = -1;
                            float urX = -1;
                            float urY = -1;
                            float lrX = -1;
                            float lrY = -1;
                            int bidi = -1;
                            IntPtr font = IntPtr.Zero;

                            int result = NativeMethods.GetStructuredTextChar(nativeContext, charPointers[k], ref codePoint, ref color, ref originX, ref originY, ref size, ref llX, ref llY, ref ulX, ref ulY, ref urX, ref urY, ref lrX, ref lrY, ref bidi, ref font);

                            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextChar returned the wrong exit code.");

                            Assert.IsTrue(ulX >= 0, "The top-left x coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                            Assert.IsTrue(ulY >= 0, "The top-left y coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(urX * dir >= ulX * dir, "The top-right x coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                            Assert.IsTrue(urY >= 0, "The top-right y coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(llX >= 0, "The bottom-left x coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                            Assert.IsTrue(llY * dir >= ulY * dir, "The bottom-left y coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range (" + (llY * dir).ToString() + " >= " + (ulY * dir).ToString() + " should be true).");

                            Assert.IsTrue(lrX * dir >= llX * dir, "The bottom-right x coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                            Assert.IsTrue(lrY * dir >= urY * dir, "The bottom-right y coordinate of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(size >= 0, "The size of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(originX >= 0, "The x origin of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
                            Assert.IsTrue(originY >= 0, "The y origin of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(codePoint >= 0, "The code point of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(color >= 0, "The colour of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(bidi >= 0, "The text direction of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range (" + bidi.ToString() + ").");

                            Assert.AreNotEqual(IntPtr.Zero, font, "The font of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is NULL.");
                        }

                        try
                        {
                            charsHandle.Free();
                        }
                        catch { }
                    }

                    try
                    {
                        linesHandle.Free();
                    }
                    catch { }
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        [DeploymentItem("Data/eng.traineddata")]
        public void StructuredTextPageGetterWithOCR()
        {
            IntPtr nativeSTextPage = IntPtr.Zero;
            int sTextBlockCount = -1;
            string tesseractFileName = Path.GetFullPath("eng.traineddata");
            string prefix = Path.GetDirectoryName(tesseractFileName);

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) = CreateSamplePNGDisplayList();

            int progressCount = 0;

            int result = NativeMethods.GetStructuredTextPageWithOCR(nativeContext, nativeDisplayList, 1, ref nativeSTextPage, ref sTextBlockCount, 1, x0, y0, x1, y1, "TESSDATA_PREFIX=" + prefix, "eng", prog => { progressCount++; return 0; });

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextPage returned the wrong exit code.");
            Assert.IsTrue(sTextBlockCount > 0, "The number of text blocks in the page is wrong.");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.ProcessArchitecture != Architecture.X86)
            {
                Assert.IsTrue(progressCount > 0, "The progress callback was not called.");
            }

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void LoadEmptyOutline()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            IntPtr outline = NativeMethods.LoadOutline(nativeContext, nativeDocument);

            Assert.AreEqual(IntPtr.Zero, outline, "Loading the outline from a document without an outline did not return NULL.");

            try
            {
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void LoadOutline()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument("Tests.Data.mupdf_explored.pdf");

            IntPtr outline = NativeMethods.LoadOutline(nativeContext, nativeDocument);

            Assert.AreNotEqual(IntPtr.Zero, outline, "Loading the outline from a document with an outline returned NULL.");

            NativeMethods.DisposeOutline(nativeContext, outline);

            try
            {
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) CreateSampleImage(string resource = "Tests.Data.Sample.RGB.pdf")
        {
            IntPtr nativeSTextPage = IntPtr.Zero;
            int sTextBlockCount = -1;

            IntPtr nativeDisplayList = IntPtr.Zero;

            float x0 = -1;
            float y0 = -1;
            float x1 = -1;
            float y1 = -1;

            IntPtr nativePage = IntPtr.Zero;

            float x = -1;
            float y = -1;
            float w = -1;
            float h = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument(resource);

            _ = NativeMethods.LoadPage(nativeContext, nativeDocument, 0, ref nativePage, ref x, ref y, ref w, ref h);

            _ = NativeMethods.GetDisplayList(nativeContext, nativePage, 1, ref nativeDisplayList, ref x0, ref y0, ref x1, ref y1);

            _ = NativeMethods.GetStructuredTextPage(nativeContext, nativeDisplayList, (int)StructuredTextFlags.PreserveImages, ref nativeSTextPage, ref sTextBlockCount);

            IntPtr[] blockPointers = new IntPtr[sTextBlockCount];
            GCHandle blocksHandle = GCHandle.Alloc(blockPointers, GCHandleType.Pinned);

            _ = NativeMethods.GetStructuredTextBlocks(nativeSTextPage, blocksHandle.AddrOfPinnedObject());

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 1)
                {
                    return (dataHandle, ms, nativeDocument, nativeStream, nativePage, nativeDisplayList, nativeSTextPage, blocksHandle, image, nativeContext);
                }
            }

            throw new NotImplementedException();
        }

        [TestMethod]
        public void GetImageMetadata()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage();

            int w = -1;
            int h = -1;
            int xres = -1;
            int yres = -1;
            byte orientation = 255;
            IntPtr colorspace = IntPtr.Zero;

            int result = NativeMethods.GetImageMetadata(nativeContext, image, ref w, ref h, ref xres, ref yres, ref orientation, ref colorspace);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetImageMetadata returned the wrong exit code.");
            Assert.IsTrue(w > 0, "The image width is wrong.");
            Assert.IsTrue(h > 0, "The image height is wrong.");
            Assert.IsTrue(xres > 0, "The image horizontal resolution is wrong.");
            Assert.IsTrue(yres > 0, "The image vertical resolution is wrong.");
            Assert.IsTrue(orientation < 255, "The image orientation is wrong.");
            Assert.AreNotEqual(IntPtr.Zero, colorspace, "The colour space pointer is NULL.");

            try
            {
                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void LoadPixmap_SampleRGB()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.RGB.pdf");

            IntPtr pixmap = IntPtr.Zero;
            IntPtr samples = IntPtr.Zero;
            int sampleCount = 0;

            int result = NativeMethods.LoadPixmap(nativeContext, image, ref pixmap, ref samples, ref sampleCount);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "LoadPixmap returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, pixmap, "The pixmap pointer is NULL.");
            Assert.AreNotEqual(IntPtr.Zero, samples, "The data pointer is NULL.");
            Assert.AreEqual(1024 * 800 * 3, sampleCount, "The number of pixel samples is wrong.");

            try
            {
                NativeMethods.DisposePixmap(nativeContext, pixmap);
                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void LoadPixmap_SampleGray()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.Gray.pdf");

            IntPtr pixmap = IntPtr.Zero;
            IntPtr samples = IntPtr.Zero;
            int sampleCount = 0;

            int result = NativeMethods.LoadPixmap(nativeContext, image, ref pixmap, ref samples, ref sampleCount);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "LoadPixmap returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, pixmap, "The pixmap pointer is NULL.");
            Assert.AreNotEqual(IntPtr.Zero, samples, "The data pointer is NULL.");
            Assert.AreEqual(1024 * 800, sampleCount, "The number of pixel samples is wrong.");

            try
            {
                NativeMethods.DisposePixmap(nativeContext, pixmap);
                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void LoadPixmap_SampleCMYK()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.CMYK.pdf");

            IntPtr pixmap = IntPtr.Zero;
            IntPtr samples = IntPtr.Zero;
            int sampleCount = 0;

            int result = NativeMethods.LoadPixmap(nativeContext, image, ref pixmap, ref samples, ref sampleCount);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "LoadPixmap returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, pixmap, "The pixmap pointer is NULL.");
            Assert.AreNotEqual(IntPtr.Zero, samples, "The data pointer is NULL.");
            Assert.AreEqual(1024 * 800 * 4, sampleCount, "The number of pixel samples is wrong.");

            try
            {
                NativeMethods.DisposePixmap(nativeContext, pixmap);
                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void LoadPixmapRGB_SampleCMYK()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.CMYK.pdf");

            IntPtr pixmap = IntPtr.Zero;
            IntPtr samples = IntPtr.Zero;
            int sampleCount = 0;

            int result = NativeMethods.LoadPixmapRGB(nativeContext, image, 0, ref pixmap, ref samples, ref sampleCount);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "LoadPixmapRGB returned the wrong exit code.");
            Assert.AreNotEqual(IntPtr.Zero, pixmap, "The pixmap pointer is NULL.");
            Assert.AreNotEqual(IntPtr.Zero, samples, "The data pointer is NULL.");
            Assert.AreEqual(1024 * 800 * 3, sampleCount, "The number of pixel samples is wrong.");

            try
            {
                NativeMethods.DisposePixmap(nativeContext, pixmap);
                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }


        [TestMethod]
        public void SaveRasterImage_SampleRGB()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.RGB.pdf");

            string tempFile = Path.GetTempFileName();

            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                int result = NativeMethods.SaveRasterImage(nativeContext, image, tempFile, i, 90, 0);

                Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveRasterImage (RGB, " + Enum.GetName((RasterOutputFileTypes)i) + ") returned the wrong exit code.");
                Assert.IsTrue(File.Exists(tempFile), "The output file does not exist.");
                Assert.IsTrue(new FileInfo(tempFile).Length > 1024, "The output file is too small.");
            }

            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void SaveRasterImage_SampleGray()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.Gray.pdf");

            string tempFile = Path.GetTempFileName();

            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                int result = NativeMethods.SaveRasterImage(nativeContext, image, tempFile, i, 90, 0);

                Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveRasterImage (Gray, " + Enum.GetName((RasterOutputFileTypes)i) + ") returned the wrong exit code.");
                Assert.IsTrue(File.Exists(tempFile), "The output file does not exist.");
                Assert.IsTrue(new FileInfo(tempFile).Length > 1024, "The output file is too small.");
            }

            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void SaveRasterImage_SampleCMYK()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.CMYK.pdf");

            string tempFile = Path.GetTempFileName();

            for (int i = 0; i < 5; i++)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                int result = NativeMethods.SaveRasterImage(nativeContext, image, tempFile, i, 90, i == (int)RasterOutputFileTypes.PSD || i == (int)RasterOutputFileTypes.JPEG ? 0 : 1);

                Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "SaveRasterImage (CMYK, " + Enum.GetName((RasterOutputFileTypes)i) + ") returned the wrong exit code.");
                Assert.IsTrue(File.Exists(tempFile), "The output file does not exist.");
                Assert.IsTrue(new FileInfo(tempFile).Length > 1024, "The output file is too small.");
            }

            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void GetColorSpaceData()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.Gray.pdf");

            int w = -1;
            int h = -1;
            int xres = -1;
            int yres = -1;
            byte orientation = 255;
            IntPtr colorspace = IntPtr.Zero;

            _ = NativeMethods.GetImageMetadata(nativeContext, image, ref w, ref h, ref xres, ref yres, ref orientation, ref colorspace);

            int csType = -1;
            int nameLength = -1;
            IntPtr baseCs = IntPtr.Zero;
            int lookupSize = -1;
            IntPtr lookupTable = IntPtr.Zero;

            int result = NativeMethods.GetColorSpaceData(nativeContext, colorspace, ref csType, ref nameLength, ref baseCs, ref lookupSize, ref lookupTable);

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetColorSpaceData returned the wrong exit code.");
            Assert.IsTrue(csType > 0, "The colour space type is wrong.");
            Assert.IsTrue(nameLength >= 0, "The colour space name length is wrong.");

            try
            {
                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void GetColorSpaceName()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativePage, IntPtr nativeDisplayList, IntPtr nativeSTextPage, GCHandle blocksHandle, IntPtr image, IntPtr nativeContext) = CreateSampleImage("Tests.Data.Sample.CMYK.pdf");

            int w = -1;
            int h = -1;
            int xres = -1;
            int yres = -1;
            byte orientation = 255;
            IntPtr colorspace = IntPtr.Zero;

            _ = NativeMethods.GetImageMetadata(nativeContext, image, ref w, ref h, ref xres, ref yres, ref orientation, ref colorspace);

            int csType = -1;
            int nameLength = -1;
            IntPtr baseCs = IntPtr.Zero;
            int lookupSize = -1;
            IntPtr lookupTable = IntPtr.Zero;

            _ = NativeMethods.GetColorSpaceData(nativeContext, colorspace, ref csType, ref nameLength, ref baseCs, ref lookupSize, ref lookupTable);

            byte[] nameBytes = new byte[nameLength];

            GCHandle nameBytesHandle = GCHandle.Alloc(nameBytes, GCHandleType.Pinned);

            int result = NativeMethods.GetColorSpaceName(nativeContext, colorspace, nameLength, nameBytesHandle.AddrOfPinnedObject());

            nameBytesHandle.Free();

            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetColorSpaceName returned the wrong exit code.");
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("DeviceCMYK"), nameBytes, "The colour space name length is wrong.");

            try
            {
                blocksHandle.Free();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                ms.Dispose();
                _ = NativeMethods.DisposeContext(nativeContext);
                dataHandle.Free();
            }
            catch { }
        }

        [TestMethod]
        public void GetFontMetadata()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 0)
                {
                    IntPtr[] linePointers = new IntPtr[lineCount];
                    GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

                    _ = NativeMethods.GetStructuredTextLines(blockPointers[i], linesHandle.AddrOfPinnedObject());

                    for (int j = 0; j < lineCount; j++)
                    {
                        int wmode = -1;
                        x0 = -1;
                        y0 = -1;
                        x1 = -1;
                        y1 = -1;

                        float x = -1;
                        float y = -1;

                        int charCount = -1;

                        _ = NativeMethods.GetStructuredTextLine(linePointers[j], ref wmode, ref x0, ref y0, ref x1, ref y1, ref x, ref y, ref charCount);

                        IntPtr[] charPointers = new IntPtr[charCount];
                        GCHandle charsHandle = GCHandle.Alloc(charPointers, GCHandleType.Pinned);

                        _ = NativeMethods.GetStructuredTextChars(linePointers[j], charsHandle.AddrOfPinnedObject());

                        for (int k = 0; k < charCount; k++)
                        {
                            int codePoint = -1;
                            uint color = 0;
                            float originX = -1;
                            float originY = -1;
                            float size = -1;
                            float llX = -1;
                            float llY = -1;
                            float ulX = -1;
                            float ulY = -1;
                            float urX = -1;
                            float urY = -1;
                            float lrX = -1;
                            float lrY = -1;
                            int bidi = -1;
                            IntPtr font = IntPtr.Zero;

                            _ = NativeMethods.GetStructuredTextChar(nativeContext, charPointers[k], ref codePoint, ref color, ref originX, ref originY, ref size, ref llX, ref llY, ref ulX, ref ulY, ref urX, ref urY, ref lrX, ref lrY, ref bidi, ref font);

                            int fontNameLength = -1;
                            int bold = -1;
                            int italic = -1;
                            int serif = -1;
                            int monospaced = -1;

                            int result = NativeMethods.GetFontMetadata(nativeContext, font, ref fontNameLength, ref bold, ref italic, ref serif, ref monospaced);

                            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetFontMetadata returned the wrong exit code.");

                            Assert.IsTrue(fontNameLength >= 0, "The font name length is wrong.");
                            Assert.IsTrue(bold == 1 || bold == 0, "The font bold style is wrong.");
                            Assert.IsTrue(italic == 1 || italic == 0, "The font italic style is wrong.");
                            Assert.IsTrue(serif == 1 || serif == 0, "The font serif style is wrong.");
                            Assert.IsTrue(monospaced == 1 || monospaced == 0, "The font monospaced style is wrong.");
                        }

                        try
                        {
                            charsHandle.Free();
                        }
                        catch { }
                    }

                    try
                    {
                        linesHandle.Free();
                    }
                    catch { }
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }


        [TestMethod]
        public void GetFontName()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 0)
                {
                    IntPtr[] linePointers = new IntPtr[lineCount];
                    GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

                    _ = NativeMethods.GetStructuredTextLines(blockPointers[i], linesHandle.AddrOfPinnedObject());

                    for (int j = 0; j < lineCount; j++)
                    {
                        int wmode = -1;
                        x0 = -1;
                        y0 = -1;
                        x1 = -1;
                        y1 = -1;

                        float x = -1;
                        float y = -1;

                        int charCount = -1;

                        _ = NativeMethods.GetStructuredTextLine(linePointers[j], ref wmode, ref x0, ref y0, ref x1, ref y1, ref x, ref y, ref charCount);

                        IntPtr[] charPointers = new IntPtr[charCount];
                        GCHandle charsHandle = GCHandle.Alloc(charPointers, GCHandleType.Pinned);

                        _ = NativeMethods.GetStructuredTextChars(linePointers[j], charsHandle.AddrOfPinnedObject());

                        for (int k = 0; k < charCount; k++)
                        {
                            int codePoint = -1;
                            uint color = 0;
                            float originX = -1;
                            float originY = -1;
                            float size = -1;
                            float llX = -1;
                            float llY = -1;
                            float ulX = -1;
                            float ulY = -1;
                            float urX = -1;
                            float urY = -1;
                            float lrX = -1;
                            float lrY = -1;
                            int bidi = -1;
                            IntPtr font = IntPtr.Zero;

                            _ = NativeMethods.GetStructuredTextChar(nativeContext, charPointers[k], ref codePoint, ref color, ref originX, ref originY, ref size, ref llX, ref llY, ref ulX, ref ulY, ref urX, ref urY, ref lrX, ref lrY, ref bidi, ref font);

                            int fontNameLength = -1;
                            int bold = -1;
                            int italic = -1;
                            int serif = -1;
                            int monospaced = -1;

                            _ = NativeMethods.GetFontMetadata(nativeContext, font, ref fontNameLength, ref bold, ref italic, ref serif, ref monospaced);

                            byte[] fontNameBytes = new byte[fontNameLength];

                            GCHandle fontNameBytesHandle = GCHandle.Alloc(fontNameBytes, GCHandleType.Pinned);
                            int result = NativeMethods.GetFontName(nativeContext, font, fontNameLength, fontNameBytesHandle.AddrOfPinnedObject());
                            fontNameBytesHandle.Free();

                            string fontName = Encoding.ASCII.GetString(fontNameBytes);

                            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetFontName returned the wrong exit code.");

                            Assert.IsTrue(fontName.All(x => char.IsLetterOrDigit(x) || x == '+' || x == '-'), "The font name is wrong.");
                        }

                        try
                        {
                            charsHandle.Free();
                        }
                        catch { }
                    }

                    try
                    {
                        linesHandle.Free();
                    }
                    catch { }
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void GetFontHandles()
        {
            (GCHandle blocksHandle, GCHandle dataHandle, MemoryStream ms, IntPtr[] blockPointers, IntPtr nativeSTextPage, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext) = CreateSampleStructuredTextBlocks();

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                _ = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                if (type == 0)
                {
                    IntPtr[] linePointers = new IntPtr[lineCount];
                    GCHandle linesHandle = GCHandle.Alloc(linePointers, GCHandleType.Pinned);

                    _ = NativeMethods.GetStructuredTextLines(blockPointers[i], linesHandle.AddrOfPinnedObject());

                    for (int j = 0; j < lineCount; j++)
                    {
                        int wmode = -1;
                        x0 = -1;
                        y0 = -1;
                        x1 = -1;
                        y1 = -1;

                        float x = -1;
                        float y = -1;

                        int charCount = -1;

                        _ = NativeMethods.GetStructuredTextLine(linePointers[j], ref wmode, ref x0, ref y0, ref x1, ref y1, ref x, ref y, ref charCount);

                        IntPtr[] charPointers = new IntPtr[charCount];
                        GCHandle charsHandle = GCHandle.Alloc(charPointers, GCHandleType.Pinned);

                        _ = NativeMethods.GetStructuredTextChars(linePointers[j], charsHandle.AddrOfPinnedObject());

                        for (int k = 0; k < charCount; k++)
                        {
                            int codePoint = -1;
                            uint color = 0;
                            float originX = -1;
                            float originY = -1;
                            float size = -1;
                            float llX = -1;
                            float llY = -1;
                            float ulX = -1;
                            float ulY = -1;
                            float urX = -1;
                            float urY = -1;
                            float lrX = -1;
                            float lrY = -1;
                            int bidi = -1;
                            IntPtr font = IntPtr.Zero;

                            _ = NativeMethods.GetStructuredTextChar(nativeContext, charPointers[k], ref codePoint, ref color, ref originX, ref originY, ref size, ref llX, ref llY, ref ulX, ref ulY, ref urX, ref urY, ref lrX, ref lrY, ref bidi, ref font);

                            IntPtr t3Procs = IntPtr.Zero;
                            IntPtr FTHandle = IntPtr.Zero;

                            int result = NativeMethods.GetFTHandle(nativeContext, font, ref FTHandle);
                            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetFTHandle returned the wrong exit code.");

                            result = NativeMethods.GetT3Procs(nativeContext, font, ref t3Procs);
                            Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetT3Procs returned the wrong exit code.");

                            Assert.IsTrue(t3Procs != IntPtr.Zero || FTHandle != IntPtr.Zero, "The font handles are both NULL.");
                        }

                        try
                        {
                            charsHandle.Free();
                        }
                        catch { }
                    }

                    try
                    {
                        linesHandle.Free();
                    }
                    catch { }
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeStructuredTextPage(nativeContext, nativeSTextPage);
                _ = NativeMethods.DisposeDisplayList(nativeContext, nativeDisplayList);
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void GetPageBox()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x, float y, float w, float h) = CreateSamplePage("Tests.Data.Sample.PageBoxes.pdf");

            ExitCodes result = (ExitCodes)NativeMethods.GetPageBox(nativeContext, nativePage, (int)BoxType.MediaBox, ref x, ref y, ref w, ref h);
            Assert.AreEqual(ExitCodes.EXIT_SUCCESS, result, "GetPageBox (MediaBox) returned an error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");
            Assert.AreEqual(-2, x, "The MediaBox X is incorrect.");
            Assert.AreEqual(-2, y, "The MediaBox Y is incorrect.");
            Assert.AreEqual(99, w, "The MediaBox W is incorrect.");
            Assert.AreEqual(97, h, "The MediaBox H is incorrect.");

            result = (ExitCodes)NativeMethods.GetPageBox(nativeContext, nativePage, (int)BoxType.CropBox, ref x, ref y, ref w, ref h);
            Assert.AreEqual(ExitCodes.EXIT_SUCCESS, result, "GetPageBox (CropBox) returned an error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");
            Assert.AreEqual(0, x, "The CropBox X is incorrect.");
            Assert.AreEqual(0, y, "The CropBox Y is incorrect.");
            Assert.AreEqual(95, w, "The CropBox W is incorrect.");
            Assert.AreEqual(93, h, "The CropBox H is incorrect.");

            result = (ExitCodes)NativeMethods.GetPageBox(nativeContext, nativePage, (int)BoxType.TrimBox, ref x, ref y, ref w, ref h);
            Assert.AreEqual(ExitCodes.EXIT_SUCCESS, result, "GetPageBox (TrimBox) returned an error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");
            Assert.AreEqual(2, x, "The TrimBox X is incorrect.");
            Assert.AreEqual(2, y, "The TrimBox Y is incorrect.");
            Assert.AreEqual(91, w, "The TrimBox W is incorrect.");
            Assert.AreEqual(89, h, "The TrimBox H is incorrect.");

            result = (ExitCodes)NativeMethods.GetPageBox(nativeContext, nativePage, (int)BoxType.ArtBox, ref x, ref y, ref w, ref h);
            Assert.AreEqual(ExitCodes.EXIT_SUCCESS, result, "GetPageBox (ArtBox) returned an error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");
            Assert.AreEqual(4, x, "The ArtBox X is incorrect.");
            Assert.AreEqual(4, y, "The ArtBox Y is incorrect.");
            Assert.AreEqual(87, w, "The ArtBox W is incorrect.");
            Assert.AreEqual(85, h, "The ArtBox H is incorrect.");

            result = (ExitCodes)NativeMethods.GetPageBox(nativeContext, nativePage, (int)BoxType.BleedBox, ref x, ref y, ref w, ref h);
            Assert.AreEqual(ExitCodes.EXIT_SUCCESS, result, "GetPageBox (BleedBox) returned an error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");
            Assert.AreEqual(6, x, "The BleedBox X is incorrect.");
            Assert.AreEqual(6, y, "The BleedBox Y is incorrect.");
            Assert.AreEqual(83, w, "The BleedBox W is incorrect.");
            Assert.AreEqual(81, h, "The BleedBox H is incorrect.");

            result = (ExitCodes)NativeMethods.GetPageBox(nativeContext, nativePage, (int)BoxType.UnknownBox, ref x, ref y, ref w, ref h);
            Assert.AreEqual(ExitCodes.EXIT_SUCCESS, result, "GetPageBox (UnknownBox) returned an error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");
            Assert.AreEqual(0, x, "The UnknownBox X is incorrect.");
            Assert.AreEqual(0, y, "The UnknownBox Y is incorrect.");
            Assert.AreEqual(95, w, "The UnknownBox W is incorrect.");
            Assert.AreEqual(93, h, "The UnknownBox H is incorrect.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void GetPDFDocument()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();
            IntPtr pdfDoc = IntPtr.Zero;

            ExitCodes result = (ExitCodes)NativeMethods.GetPDFDocument(nativeContext, nativeDocument, ref pdfDoc);

            Assert.AreEqual(ExitCodes.EXIT_SUCCESS, result, "GetPDFDocument returned an error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");
            Assert.AreNotEqual(IntPtr.Zero, pdfDoc, "The PDF document pointer is null.");
            Assert.AreEqual(nativeDocument, pdfDoc, "The PDF document pointer for the PDF document is not the same as the document pointer.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, pdfDoc);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }

            (dataHandle, ms, nativeContext, nativeDocument, nativeStream) = CreateSamplePNGDocument();
            pdfDoc = IntPtr.Zero;

            result = (ExitCodes)NativeMethods.GetPDFDocument(nativeContext, nativeDocument, ref pdfDoc);

            Assert.AreEqual(ExitCodes.ERR_CANNOT_CONVERT_TO_PDF, result, "GetPDFDocument returned the wrong error code: " + result.ToString() + "(" + ((int)result).ToString() + ").");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void DefaultOCGConfig()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            int nameLength = -1;
            int creatorLength = -1;
            NativeMethods.ReadDefaultOCGConfigNameLength(nativeContext, nativeDocument, ref nameLength, ref creatorLength);
            Assert.AreEqual(0, nameLength, "Reading the default OCG configuration for a document without OCGs did not return 0.");
            Assert.AreEqual(0, creatorLength, "Reading the default OCG configuration for a document without OCGs did not return 0.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }

            (dataHandle, ms, nativeContext, nativeDocument, nativeStream) = CreateSampleDocument("Tests.Data.Sample.OCGTree.pdf");

            nameLength = -1;
            creatorLength = -1;
            NativeMethods.ReadDefaultOCGConfigNameLength(nativeContext, nativeDocument, ref nameLength, ref creatorLength);
            Assert.AreEqual("Default".Length, nameLength, "The default OCG configuration name length is wrong.");
            Assert.AreEqual(Encoding.UTF8.GetBytes("VectSharp.PDF v3.1.0").Length, creatorLength, "The default OCG configuration creator length is wrong.");

            byte[] nameBytes = new byte[nameLength];
            byte[] creatorBytes = new byte[creatorLength];

            GCHandle nameHandle = GCHandle.Alloc(nameBytes, GCHandleType.Pinned);
            GCHandle creatorHandle = GCHandle.Alloc(creatorBytes, GCHandleType.Pinned);

            NativeMethods.ReadDefaultOCGConfig(nativeContext, nativeDocument, nameLength, creatorLength, nameHandle.AddrOfPinnedObject(), creatorHandle.AddrOfPinnedObject());

            nameHandle.Free();
            creatorHandle.Free();

            Assert.AreEqual("Default", Encoding.UTF8.GetString(nameBytes), "The default OCG configuration name is wrong.");
            Assert.AreEqual("VectSharp.PDF v3.1.0", Encoding.UTF8.GetString(creatorBytes), "The default OCG configuration creator is wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void AlternativeOCGConfigs()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            int count = NativeMethods.CountAlternativeOCGConfigs(nativeContext, nativeDocument);
            Assert.AreEqual(0, count, "Reading the number of alternative OCG configurations for a document without OCGs did not return 0.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }

            (dataHandle, ms, nativeContext, nativeDocument, nativeStream) = CreateSampleDocument("Tests.Data.Sample.OCG.pdf");

            count = NativeMethods.CountAlternativeOCGConfigs(nativeContext, nativeDocument);
            Assert.AreEqual(0, count, "Reading the number of alternative OCG configurations for a document with only a default OCG did not return 0.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }

            (dataHandle, ms, nativeContext, nativeDocument, nativeStream) = CreateSampleDocument("Tests.Data.Sample.OCGTree.pdf");

            count = NativeMethods.CountAlternativeOCGConfigs(nativeContext, nativeDocument);
            Assert.AreEqual(3, count, "Reading the number of alternative OCG configurations returned the wrong value.");

            string[] expectedConfigNames = new string[] { "Rando1", "Rand2", "Ran3" };
            string[] expectedConfigCreators = new string[] { "VectSharp.PDFv3.1.0", "VectSharp.PDF3.1.0", "VectSharp.PDF.1.0" };

            for (int i = 0; i < count; i++)
            {
                int nameLength = -1;
                int creatorLength = -1;
                NativeMethods.ReadOCGConfigNameLength(nativeContext, nativeDocument, i, ref nameLength, ref creatorLength);
                Assert.AreEqual(Encoding.UTF8.GetBytes(expectedConfigNames[i]).Length, nameLength, "The OCG configuration name length is wrong.");
                Assert.AreEqual(Encoding.UTF8.GetBytes(expectedConfigCreators[i]).Length, creatorLength, "The OCG configuration creator length is wrong.");

                byte[] nameBytes = new byte[nameLength];
                byte[] creatorBytes = new byte[creatorLength];

                GCHandle nameHandle = GCHandle.Alloc(nameBytes, GCHandleType.Pinned);
                GCHandle creatorHandle = GCHandle.Alloc(creatorBytes, GCHandleType.Pinned);

                NativeMethods.ReadOCGConfig(nativeContext, nativeDocument, i, nameLength, creatorLength, nameHandle.AddrOfPinnedObject(), creatorHandle.AddrOfPinnedObject());

                nameHandle.Free();
                creatorHandle.Free();

                Assert.AreEqual(expectedConfigNames[i], Encoding.UTF8.GetString(nameBytes), "The OCG configuration name is wrong.");
                Assert.AreEqual(expectedConfigCreators[i], Encoding.UTF8.GetString(creatorBytes), "The OCG configuration creator is wrong.");
            }

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void OptionalContentGroups()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            int count = NativeMethods.CountOptionalContentGroups(nativeContext, nativeDocument);
            Assert.AreEqual(0, count, "Reading the number of OCGs for a document without OCGs did not return 0.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }

            (dataHandle, ms, nativeContext, nativeDocument, nativeStream) = CreateSampleDocument("Tests.Data.Sample.OCG.pdf");

            count = NativeMethods.CountOptionalContentGroups(nativeContext, nativeDocument);
            Assert.AreEqual(3, count, "The number of OCGs in the document is wrong.");

            int[] nameLengths = new int[count];
            GCHandle nameLengthHandle = GCHandle.Alloc(nameLengths, GCHandleType.Pinned);
            NativeMethods.GetOptionalContentGroupNameLengths(nativeContext, nativeDocument, count, nameLengthHandle.AddrOfPinnedObject());
            nameLengthHandle.Free();
            CollectionAssert.AreEqual(new int[] { "Blue".Length, "Green".Length, "Red".Length }, nameLengths, "The OCG name lengths are wrong.");

            byte[][] names = new byte[count][];
            GCHandle[] nameHandles = new GCHandle[count];
            IntPtr[] nameAddresses = new IntPtr[count];

            for (int i = 0; i < count; i++)
            {
                names[i] = new byte[nameLengths[i]];
                nameHandles[i] = GCHandle.Alloc(names[i], GCHandleType.Pinned);
                nameAddresses[i] = nameHandles[i].AddrOfPinnedObject();
            }

            GCHandle nameAddressesHandle = GCHandle.Alloc(nameAddresses, GCHandleType.Pinned);

            NativeMethods.GetOptionalContentGroups(nativeContext, nativeDocument, count, nameAddressesHandle.AddrOfPinnedObject());

            string[] nameStrings = new string[count];
            nameAddressesHandle.Free();
            for (int i = 0; i < count; i++)
            {
                nameHandles[i].Free();
                nameStrings[i] = Encoding.UTF8.GetString(names[i]);
            }

            CollectionAssert.AreEqual(new string[] { "Blue", "Green", "Red" }, nameStrings, "The OCG names are wrong.");

            int[] ocgStates = new int[count];

            for (int i = 0; i < count; i++)
            {
                ocgStates[i] = NativeMethods.GetOptionalContentGroupState(nativeContext, nativeDocument, i);
            }

            CollectionAssert.AreEqual(new int[] { 0, 1, 1 }, ocgStates, "The OCG states are wrong.");

            for (int i = 0; i < count; i++)
            {
                NativeMethods.SetOptionalContentGroupState(nativeContext, nativeDocument, i, 1 - ocgStates[i]);
            }

            for (int i = 0; i < count; i++)
            {
                ocgStates[i] = NativeMethods.GetOptionalContentGroupState(nativeContext, nativeDocument, i);
            }

            CollectionAssert.AreEqual(new int[] { 1, 0, 0 }, ocgStates, "The OCG states are wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void OptionalContentGroupUI()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

            int count = NativeMethods.CountOptionalContentGroupConfigUI(nativeContext, nativeDocument);
            Assert.AreEqual(0, count, "Reading the number of OCG UIs for a document without OCGs did not return 0.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }

            (dataHandle, ms, nativeContext, nativeDocument, nativeStream) = CreateSampleDocument("Tests.Data.Sample.OCGTree.pdf");

            count = NativeMethods.CountOptionalContentGroupConfigUI(nativeContext, nativeDocument);
            Assert.AreEqual(14, count, "The number of OCG UIs is wrong.");

            int[] labelLengths = new int[count];
            GCHandle labelLengthsHandle = GCHandle.Alloc(labelLengths, GCHandleType.Pinned);

            NativeMethods.ReadOptionalContentGroupUILabelLengths(nativeContext, nativeDocument, count, labelLengthsHandle.AddrOfPinnedObject());
            labelLengthsHandle.Free();
            CollectionAssert.AreEqual(new int[] { 12, 3, 4, 5, 6, 4, 5, 12, 4, 4, 5, 5, 4, 5 }, labelLengths, "The OCG UI label lengths are wrong.");

            byte[][] labelBytes = new byte[count][];
            GCHandle[] labelByteHandles = new GCHandle[count];
            IntPtr[] labelByteAddresses = new IntPtr[count];

            for (int i = 0; i < count; i++)
            {
                labelBytes[i] = new byte[labelLengths[i]];
                labelByteHandles[i] = GCHandle.Alloc(labelBytes[i], GCHandleType.Pinned);
                labelByteAddresses[i] = labelByteHandles[i].AddrOfPinnedObject();
            }

            GCHandle labelByteAddressesHandle = GCHandle.Alloc(labelByteAddresses, GCHandleType.Pinned);

            int[] depths = new int[count];
            int[] types = new int[count];
            int[] lockeds = new int[count];
            GCHandle depthsHandle = GCHandle.Alloc(depths, GCHandleType.Pinned);
            GCHandle typesHandle = GCHandle.Alloc(types, GCHandleType.Pinned);
            GCHandle lockedsHandle = GCHandle.Alloc(lockeds, GCHandleType.Pinned);

            NativeMethods.ReadOptionalContentGroupUIs(nativeContext, nativeDocument, count, labelByteAddressesHandle.AddrOfPinnedObject(), depthsHandle.AddrOfPinnedObject(), typesHandle.AddrOfPinnedObject(), lockedsHandle.AddrOfPinnedObject());

            lockedsHandle.Free();
            typesHandle.Free();
            depthsHandle.Free();
            labelByteAddressesHandle.Free();

            string[] labels = new string[count];

            for (int i = 0; i < count; i++)
            {
                labelByteHandles[i].Free();
                labels[i] = Encoding.UTF8.GetString(labelBytes[i]);
            }

            CollectionAssert.AreEqual(new string[] { "Warm colours", "Red", "Dark", "Light", "Orange", "Dark", "Light", "Cold colours", "Blue", "Dark", "Light", "Green", "Dark", "Light" }, labels, "The OCG UI labels are wrong.");
            CollectionAssert.AreEqual(new int[] { 1, 1, 2, 2, 1, 2, 2, 1, 1, 2, 2, 1, 2, 2 }, depths, "The OCG UI depths are wrong.");
            CollectionAssert.AreEqual(new int[] { 0, 1, 2, 2, 1, 2, 2, 0, 1, 2, 2, 1, 2, 2 }, types, "The OCG UI types are wrong.");
            CollectionAssert.AreEqual(new int[] { 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 }, lockeds, "The OCG UI locked states are wrong.");

            int[] states = new int[count];

            for (int i = 0; i < count; i++)
            {
                if (lockeds[i] == 0)
                {
                    states[i] = NativeMethods.ReadOptionalContentGroupUIState(nativeContext, nativeDocument, i);
                }
            }

            CollectionAssert.AreEqual(new int[] { 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1 }, states, "The OCG UI states are wrong.");

            for (int i = 0; i < count; i++)
            {
                if (lockeds[i] == 0)
                {
                    NativeMethods.SetOptionalContentGroupUIState(nativeContext, nativeDocument, i, 1 - states[i]);
                }
            }

            for (int i = 0; i < count; i++)
            {
                if (lockeds[i] == 0)
                {
                    states[i] = NativeMethods.ReadOptionalContentGroupUIState(nativeContext, nativeDocument, i);
                }
            }

            CollectionAssert.AreEqual(new int[] { 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0 }, states, "The OCG UI states are wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void OCGConfigEnabling()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument("Tests.Data.Sample.OCGTree.pdf");

            int count = 14;
            int[] states = new int[count];

            NativeMethods.EnableOCGConfig(nativeContext, nativeDocument, 0);

            for (int i = 0; i < count; i++)
            {
                if (i != 0 && i != 7)
                {
                    states[i] = NativeMethods.ReadOptionalContentGroupUIState(nativeContext, nativeDocument, i);
                }
            }

            CollectionAssert.AreEqual(new int[] { 0, 0, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 0, 1 }, states, "The OCG UI states are wrong.");

            NativeMethods.EnableOCGConfig(nativeContext, nativeDocument, 1);

            for (int i = 0; i < count; i++)
            {
                if (i != 0 && i != 7)
                {
                    states[i] = NativeMethods.ReadOptionalContentGroupUIState(nativeContext, nativeDocument, i);
                }
            }

            CollectionAssert.AreEqual(new int[] { 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1 }, states, "The OCG UI states are wrong.");

            NativeMethods.EnableOCGConfig(nativeContext, nativeDocument, 2);

            for (int i = 0; i < count; i++)
            {
                if (i != 0 && i != 7)
                {
                    states[i] = NativeMethods.ReadOptionalContentGroupUIState(nativeContext, nativeDocument, i);
                }
            }

            CollectionAssert.AreEqual(new int[] { 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 1, 1 }, states, "The OCG UI states are wrong.");

            NativeMethods.EnableDefaultOCGConfig(nativeContext, nativeDocument);

            for (int i = 0; i < count; i++)
            {
                if (i != 0 && i != 7)
                {
                    states[i] = NativeMethods.ReadOptionalContentGroupUIState(nativeContext, nativeDocument, i);
                }
            }

            CollectionAssert.AreEqual(new int[] { 0, 1, 1, 0, 1, 1, 0, 0, 1, 0, 1, 1, 0, 1 }, states, "The OCG UI states are wrong.");

            try
            {
                dataHandle.Free();
                ms.Dispose();
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void CountLinks()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x, float y, float w, float h) = CreateSamplePage();

            IntPtr firstLink = IntPtr.Zero;
            int count = NativeMethods.CountLinks(nativeContext, nativePage, ref firstLink);

            Assert.AreEqual(0, count, "Counting links on a page that does not contain links returned the wrong number.");
            Assert.AreEqual(IntPtr.Zero, firstLink, "Counting links on a page that does not contain links returned a link pointer.");

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }

            (dataHandle, ms, nativePage, nativeDocument, nativeStream, nativeContext, x, y, w, h) = CreateSamplePage("Tests.Data.VectSharp.Markdown.pdf");

            firstLink = IntPtr.Zero;
            count = NativeMethods.CountLinks(nativeContext, nativePage, ref firstLink);
            Assert.AreEqual(11, count, "CountLinks returned the wrong value.");
            Assert.AreNotEqual(IntPtr.Zero, firstLink, "A null link pointer was returned.");

            NativeMethods.DisposeLinks(nativeContext, firstLink);

            try
            {
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void LoadLinks()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x, float y, float w, float h) = CreateSamplePage("Tests.Data.VectSharp.Markdown.pdf");

            IntPtr firstLink = IntPtr.Zero;
            int count = NativeMethods.CountLinks(nativeContext, nativePage, ref firstLink);

            int[] uriLengths = new int[count];
            IntPtr[] linksAddresses = new IntPtr[count];

            GCHandle uriLengthsHandle = GCHandle.Alloc(uriLengths, GCHandleType.Pinned);
            GCHandle linksAddressesHandle = GCHandle.Alloc(linksAddresses, GCHandleType.Pinned);

            NativeMethods.LoadLinks(firstLink, linksAddressesHandle.AddrOfPinnedObject(), uriLengthsHandle.AddrOfPinnedObject());

            linksAddressesHandle.Free();
            uriLengthsHandle.Free();

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

            CollectionAssert.AreEqual(expectedLinkUris.Select(x => Encoding.UTF8.GetBytes(x).Length).ToArray(), uriLengths, "The link Uri lengths are wrong.");
            CollectionAssert.DoesNotContain(linksAddresses, IntPtr.Zero, "A null link pointer was returned.");

            for (int i = 0; i < count; i++)
            {
                float x0, y0, x1, y1, destX, destY, destW, destH, destZoom;
                x0 = y0 = x1 = y1 = destX = destY = destW = destH = destZoom = -1;

                byte[] uriBytes = new byte[uriLengths[i]];
                int isExternal, isSetOCGState, destType, destPage, destChapter;
                isExternal = isSetOCGState = destType = destPage = destChapter = -1;

                GCHandle uriBytesHandle = GCHandle.Alloc(uriBytes, GCHandleType.Pinned);
                NativeMethods.LoadLink(nativeContext, nativeDocument, linksAddresses[i], uriLengths[i], 1, ref x0, ref y0, ref x1, ref y1, uriBytesHandle.AddrOfPinnedObject(), ref isExternal, ref isSetOCGState, ref destType, ref destX, ref destY, ref destW, ref destH, ref destZoom, ref destPage, ref destChapter);
                uriBytesHandle.Free();

                string uri = Encoding.UTF8.GetString(uriBytes);

                Assert.AreEqual(expectedLinkUris[i], uri, "The link Uri is wrong.");
                Assert.IsTrue(x0 > 0, "The link x0 is wrong.");
                Assert.IsTrue(y0 > 0, "The link y0 is wrong.");
                Assert.IsTrue(x1 > 0, "The link x1 is wrong.");
                Assert.IsTrue(y1 > 0, "The link y1 is wrong.");
                Assert.AreEqual(1, isExternal, "The link should be external.");
                Assert.AreEqual(0, isSetOCGState, "The link should not be a SetOCGState link.");
            }

            try
            {
                NativeMethods.DisposeLinks(nativeContext, firstLink);
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void LinksOCGs()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x, float y, float w, float h) = CreateSamplePage("Tests.Data.NoughtsCrosses.pdf");

            IntPtr firstLink = IntPtr.Zero;
            int count = NativeMethods.CountLinks(nativeContext, nativePage, ref firstLink);

            int[] uriLengths = new int[count];
            IntPtr[] linksAddresses = new IntPtr[count];

            GCHandle uriLengthsHandle = GCHandle.Alloc(uriLengths, GCHandleType.Pinned);
            GCHandle linksAddressesHandle = GCHandle.Alloc(linksAddresses, GCHandleType.Pinned);

            NativeMethods.LoadLinks(firstLink, linksAddressesHandle.AddrOfPinnedObject(), uriLengthsHandle.AddrOfPinnedObject());

            linksAddressesHandle.Free();
            uriLengthsHandle.Free();

            CollectionAssert.AreEqual(Enumerable.Repeat("#SetOCGState".Length, count).ToArray(), uriLengths, "The link Uri lengths are wrong.");

            int[] expectedHiddens = new int[]
            {
                1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1
            };

            for (int i = 0; i < count; i++)
            {
                float x0, y0, x1, y1, destX, destY, destW, destH, destZoom;
                x0 = y0 = x1 = y1 = destX = destY = destW = destH = destZoom = -1;

                byte[] uriBytes = new byte[uriLengths[i]];
                int isExternal, isSetOCGState, destType, destPage, destChapter;
                isExternal = isSetOCGState = destType = destPage = destChapter = -1;

                GCHandle uriBytesHandle = GCHandle.Alloc(uriBytes, GCHandleType.Pinned);
                NativeMethods.LoadLink(nativeContext, nativeDocument, linksAddresses[i], uriLengths[i], 1, ref x0, ref y0, ref x1, ref y1, uriBytesHandle.AddrOfPinnedObject(), ref isExternal, ref isSetOCGState, ref destType, ref destX, ref destY, ref destW, ref destH, ref destZoom, ref destPage, ref destChapter);
                uriBytesHandle.Free();

                string uri = Encoding.UTF8.GetString(uriBytes);

                Assert.AreEqual("#SetOCGState", uri, "The link Uri is wrong.");
                Assert.IsTrue(x0 > 0, "The link x0 is wrong.");
                Assert.IsTrue(y0 > 0, "The link y0 is wrong.");
                Assert.IsTrue(x1 > 0, "The link x1 is wrong.");
                Assert.IsTrue(y1 > 0, "The link y1 is wrong.");
                Assert.AreEqual(0, isExternal, "The link should not be external.");
                Assert.AreEqual(1, isSetOCGState, "The link should be a SetOCGState link.");

                int isHidden = NativeMethods.IsLinkHidden(nativeContext, "View", linksAddresses[i]);
                Assert.AreEqual(expectedHiddens[i], isHidden, "The link's visibility is wrong.");
            }

            NativeMethods.ActivateLinkSetOCGState(nativeContext, nativeDocument, linksAddresses[1]);

            expectedHiddens = new int[]
            {
                1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1
            };

            for (int i = 0; i < count; i++)
            {
                int isHidden = NativeMethods.IsLinkHidden(nativeContext, "View", linksAddresses[i]);
                Assert.AreEqual(expectedHiddens[i], isHidden, "The link's visibility is wrong.");
            }

            NativeMethods.EnableDefaultOCGConfig(nativeContext, nativeDocument);

            expectedHiddens = new int[]
            {
                1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1
            };

            for (int i = 0; i < count; i++)
            {
                int isHidden = NativeMethods.IsLinkHidden(nativeContext, "View", linksAddresses[i]);
                Assert.AreEqual(expectedHiddens[i], isHidden, "The link's visibility is wrong.");
            }

            try
            {
                NativeMethods.DisposeLinks(nativeContext, firstLink);
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void GetPageNumber()
        {
            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument("Tests.Data.mupdf_explored.pdf");

            int pageNum = NativeMethods.GetPageNumber(nativeContext, nativeDocument, 0, 0);
            Assert.AreEqual(0, pageNum, "The page number is wrong.");

            pageNum = NativeMethods.GetPageNumber(nativeContext, nativeDocument, 0, 2);
            Assert.AreEqual(2, pageNum, "The page number is wrong.");

            pageNum = NativeMethods.GetPageNumber(nativeContext, nativeDocument, 1, 0);
            Assert.AreEqual(-1, pageNum, "The page number is wrong.");

            try
            {
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }

        [TestMethod]
        public void StructureStructuredTextPage()
        {
            IntPtr nativeSTextPage = IntPtr.Zero;
            int sTextBlockCount = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSampleDisplayList("Tests.Data.mupdf_explored.pdf");

            _ = NativeMethods.GetStructuredTextPage(nativeContext, nativeDisplayList, (int)(StructuredTextFlags.CollectStructure | StructuredTextFlags.Segment), ref nativeSTextPage, ref sTextBlockCount);

            IntPtr[] blockPointers = new IntPtr[sTextBlockCount];
            GCHandle blocksHandle = GCHandle.Alloc(blockPointers, GCHandleType.Pinned);

            _ = NativeMethods.GetStructuredTextBlocks(nativeSTextPage, blocksHandle.AddrOfPinnedObject());

            for (int i = 0; i < blockPointers.Length; i++)
            {
                int type = -1;
                float x0 = -1;
                float y0 = -1;
                float x1 = -1;
                float y1 = -1;
                int lineCount = -1;
                IntPtr image = IntPtr.Zero;
                float a = -1;
                float b = -1;
                float c = -1;
                float d = -1;
                float e = -1;
                float f = -1;

                byte stroked = 0;
                uint argb = 0;

                int xs_len = -1;
                int ys_len = -1;
                IntPtr down = IntPtr.Zero;
                int index = -1;

                int result = NativeMethods.GetStructuredTextBlock(nativeContext, blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount, ref image, ref a, ref b, ref c, ref d, ref e, ref f, ref stroked, ref argb, ref xs_len, ref ys_len, ref down, ref index);

                Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextBlock returned the wrong exit code.");

                if (type == 2)
                {
                    int children = NativeMethods.CountStructStructuredTextBlockChildren(down);

                    Assert.IsTrue(children > 0, "The structural structured text block does not have any children.");

                    IntPtr[] childBlocks = new IntPtr[children];
                    GCHandle childBlocksHandle = GCHandle.Alloc(childBlocks, GCHandleType.Pinned);

                    int rawLength = 0;
                    int standard = -1;
                    IntPtr parent = IntPtr.Zero;
                    result = NativeMethods.GetStructStructuredTextBlock(down, ref rawLength, ref standard, ref parent, childBlocksHandle.AddrOfPinnedObject());
                    childBlocksHandle.Free();

                    Assert.IsTrue(rawLength > 0, "The group's raw type length is zero.");
                    Assert.AreEqual((int)StructureType.Division, standard, "The group's structure type is wrong.");
                    CollectionAssert.DoesNotContain(childBlocks, IntPtr.Zero, "The group has a NULL child.");

                    byte[] rawStructureName = new byte[rawLength];
                    GCHandle rawStructureNameHandle = GCHandle.Alloc(rawStructureName, GCHandleType.Pinned);
                    NativeMethods.GetStructStructuredTextBlockRawStructure(down, rawLength, rawStructureNameHandle.AddrOfPinnedObject());
                    rawStructureNameHandle.Free();

                    Assert.AreEqual("Split", Encoding.UTF8.GetString(rawStructureName), "The group's raw structure name is wrong.");
                }
            }

            try
            {
                blocksHandle.Free();
                dataHandle.Free();
                ms.Dispose();

                _ = NativeMethods.DisposePage(nativeContext, nativePage);
                _ = NativeMethods.DisposeDocument(nativeContext, nativeDocument);
                _ = NativeMethods.DisposeStream(nativeContext, nativeStream);
                _ = NativeMethods.DisposeContext(nativeContext);
            }
            catch { }
        }
    }
}
