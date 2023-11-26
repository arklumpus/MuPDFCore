using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

#pragma warning disable IDE0090 // Use 'new(...)'
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

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) CreateSampleDocument()
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

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x, float y, float w, float h) CreateSamplePage()
        {
            IntPtr nativePage = IntPtr.Zero;

            float x = -1;
            float y = -1;
            float w = -1;
            float h = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativeContext, IntPtr nativeDocument, IntPtr nativeStream) = CreateSampleDocument();

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

        private static (GCHandle dataHandle, MemoryStream ms, IntPtr nativeDisplayList, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float x0, float y0, float x1, float y1) CreateSampleDisplayList()
        {
            IntPtr nativeDisplayList = IntPtr.Zero;

            float x0 = -1;
            float y0 = -1;
            float x1 = -1;
            float y1 = -1;

            (GCHandle dataHandle, MemoryStream ms, IntPtr nativePage, IntPtr nativeDocument, IntPtr nativeStream, IntPtr nativeContext, float _, float _, float _, float _) = CreateSamplePage();

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

            int result = NativeMethods.ShrinkStore(nativeContext, (uint)50);
            Assert.AreEqual(1, result, "ShrinkStore returned the wrong exit code.");

            ulong postSize = NativeMethods.GetCurrentStoreSize(nativeContext);

            Assert.IsTrue(postSize <= preSize, "The store has not been shrunk.");
            Assert.IsTrue(postSize <= Math.Ceiling(preSize * 0.5), "The store has not been shrunk by the required amount.");

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
            {
                result = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 0, ref documentWriter);
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
            {
                result = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 1, ref documentWriter);
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
            {
                result = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 2, ref documentWriter);
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
            {
                _ = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 0, ref documentWriter);
            }

            return (documentWriter, fileName);
        }

        private (IntPtr documentWriter, string fileName) CreateDocumentWriterSVG(IntPtr nativeContext)
        {
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            {
                _ = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 1, ref documentWriter);
            }

            return (documentWriter, fileName);
        }

        private (IntPtr documentWriter, string fileName) CreateDocumentWriterCBZ(IntPtr nativeContext)
        {
            IntPtr documentWriter = IntPtr.Zero;
            string fileName = Path.GetTempFileName();

            _ = NativeMethods.CreateContext(256 << 20, ref nativeContext);

            using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
            {
                _ = NativeMethods.CreateDocumentWriter(nativeContext, encodedFileName.Address, 2, ref documentWriter);
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

            int result = NativeMethods.GetStructuredTextPage(nativeContext, nativeDisplayList, ref nativeSTextPage, ref sTextBlockCount);

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

            _ = NativeMethods.GetStructuredTextPage(nativeContext, nativeDisplayList, ref nativeSTextPage, ref sTextBlockCount);

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

                int result = NativeMethods.GetStructuredTextBlock(blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount);

                Assert.AreEqual((int)ExitCodes.EXIT_SUCCESS, result, "GetStructuredTextBlock returned the wrong exit code.");
                Assert.IsTrue(x0 >= 0, "The " + i.ToString() + "th block's left coordinate is out of range.");
                Assert.IsTrue(y0 >= 0, "The " + i.ToString() + "th block's top coordinate is out of range.");
                Assert.IsTrue(x1 >= x0, "The " + i.ToString() + "th block's right coordinate is out of range.");
                Assert.IsTrue(y1 >= y0, "The " + i.ToString() + "th block's bottom coordinate is out of range.");
                Assert.IsTrue(type == 0 || type == 1, "The " + i.ToString() + "th block's type coordinate is wrong (" + type.ToString() + ").");
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

                _ = NativeMethods.GetStructuredTextBlock(blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount);

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

                _ = NativeMethods.GetStructuredTextBlock(blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount);

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

                _ = NativeMethods.GetStructuredTextBlock(blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount);

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

                _ = NativeMethods.GetStructuredTextBlock(blockPointers[i], ref type, ref x0, ref y0, ref x1, ref y1, ref lineCount);

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
                            int c = -1;
                            int color = -1;
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

                            int result = NativeMethods.GetStructuredTextChar(charPointers[k], ref c, ref color, ref originX, ref originY, ref size, ref llX, ref llY, ref ulX, ref ulY, ref urX, ref urY, ref lrX, ref lrY);

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

                            Assert.IsTrue(c >= 0, "The code point of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");

                            Assert.IsTrue(color >= 0, "The colour of the " + k.ToString() + "th character of the " + j.ToString() + "th line of the " + i.ToString() + "th block is out of range.");
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

            int result = NativeMethods.GetStructuredTextPageWithOCR(nativeContext, nativeDisplayList, ref nativeSTextPage, ref sTextBlockCount, 1, x0, y0, x1, y1, "TESSDATA_PREFIX=" + prefix, "eng", prog => { progressCount++; return 0; } );

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
    }
}
