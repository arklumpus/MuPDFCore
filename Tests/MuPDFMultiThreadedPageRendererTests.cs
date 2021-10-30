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
    public class MuPDFMultiThreadedPageRendererTests
    {
        [TestMethod]
        public void MultiThreadedPageRendererThreadCount()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFMultiThreadedPageRenderer renderer = document.GetMultiThreadedRenderer(0, 11);

            Assert.IsNotNull(renderer, "The multi-threaded page renderer is null.");
            Assert.AreEqual(10, renderer.ThreadCount, "The thread count for the renderer is wrong.");
        }

        [TestMethod]
        public void MultiThreadedPageRendererRendering()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            int threadCount = 4;

            RoundedSize targetSize = new RoundedSize(4000, 2600);
            RoundedRectangle[] splitSize = targetSize.Split(threadCount);

            IntPtr[] destinations = new IntPtr[threadCount];

            for (int i = 0; i < destinations.Length; i++)
            {
                destinations[i] = Marshal.AllocHGlobal(splitSize[i].Width * splitSize[i].Height * 4);
            }

            byte[][] expectedStart = new byte[][]
            {
                new byte[] { 0x17, 0x73, 0xFF, 0x0B },
                new byte[] { 0x17, 0x73, 0xFF, 0x0B },
                new byte[] { 0x17, 0x73, 0xFF, 0x0B },
                new byte[] { 0x17, 0x73, 0xFF, 0x0B }
            };

            byte[][] expectedEnd = new byte[][]
            {
                new byte[] { 0x17, 0x73, 0xFF, 0x0B },
                new byte[] { 0x17, 0x73, 0xFF, 0x0B },
                new byte[] { 0x17, 0x73, 0xFF, 0x0B },
                new byte[] { 0x17, 0x73, 0xFF, 0x0B }
            };


            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFMultiThreadedPageRenderer renderer = document.GetMultiThreadedRenderer(0, 4);

            renderer.Render(targetSize, new Rectangle(0, 0, 4000, 2600), destinations, PixelFormats.RGBA);

            byte[][] rendered = new byte[threadCount][];

            for (int i = 0; i < destinations.Length; i++)
            {
                rendered[i] = new byte[splitSize[i].Width * splitSize[i].Height * 4];

                Marshal.Copy(destinations[i], rendered[i], 0, rendered[i].Length);
                Marshal.FreeHGlobal(destinations[i]);
            }

            for (int i = 0; i < destinations.Length; i++)
            {
                Assert.AreEqual(10400000, rendered[i].Length, "The size of tile " + i.ToString() + " appears to be wrong.");
                CollectionAssert.AreEqual(expectedStart[i], rendered[i][0..4], "The start of tile " + i.ToString() + " appears to be wrong.");
                CollectionAssert.AreEqual(expectedEnd[i], rendered[i][^4..^0], "The end of tile " + i.ToString() + " appears to be wrong.");
            }
        }

        [TestMethod]
        public async Task MultiThreadedPageRendererProgressGetter()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            int threadCount = 4;

            RoundedSize targetSize = new RoundedSize(4000, 2600);
            RoundedRectangle[] splitSize = targetSize.Split(threadCount);

            IntPtr[] destinations = new IntPtr[threadCount];

            for (int i = 0; i < destinations.Length; i++)
            {
                destinations[i] = Marshal.AllocHGlobal(splitSize[i].Width * splitSize[i].Height * 4);
            }

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFMultiThreadedPageRenderer renderer = document.GetMultiThreadedRenderer(0, 4);

            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            Task renderTask = Task.Run(() =>
            {
                renderer.Render(targetSize, new Rectangle(0, 0, 4000, 2600), destinations, PixelFormats.RGBA);
                semaphore.Release();
            });

            bool result = false;

            double prevPerc = 0;

            while (!result)
            {
                RenderProgress progress = renderer.GetProgress();

                long totalProgress = 0;
                long currProgress = 0;

                for (int i = 0; i < progress.ThreadRenderProgresses.Length; i++)
                {
                    totalProgress += progress.ThreadRenderProgresses[i].MaxProgress;
                    currProgress += progress.ThreadRenderProgresses[i].Progress;
                }

                double percProgress = (double)currProgress / totalProgress;

                if (percProgress >= 0 && prevPerc <= 1)
                {
                    Assert.IsTrue(percProgress >= prevPerc);
                    prevPerc = percProgress;
                }

                result = await semaphore.WaitAsync(3);
            }

            for (int i = 0; i < destinations.Length; i++)
            {
                Marshal.FreeHGlobal(destinations[i]);
            }
        }

        [TestMethod]
        public async Task MultiThreadedPageRendererAbortion()
        {
            using Stream pdfDataStream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Data.Sample.pdf");
            MemoryStream pdfStream = new MemoryStream();
            pdfDataStream.CopyTo(pdfStream);

            int threadCount = 4;

            RoundedSize targetSize = new RoundedSize(8000, 5200);
            RoundedRectangle[] splitSize = targetSize.Split(threadCount);

            IntPtr[] destinations = new IntPtr[threadCount];

            for (int i = 0; i < destinations.Length; i++)
            {
                destinations[i] = Marshal.AllocHGlobal(splitSize[i].Width * splitSize[i].Height * 4);
            }

            using MuPDFContext context = new MuPDFContext();
            using MuPDFDocument document = new MuPDFDocument(context, ref pdfStream, InputFileTypes.PDF);

            using MuPDFMultiThreadedPageRenderer renderer = document.GetMultiThreadedRenderer(0, 4);

            Task renderTask = Task.Run(() =>
            {
                renderer.Render(targetSize, new Rectangle(0, 0, 4000, 2600), destinations, PixelFormats.RGBA);
            });

            renderer.Abort();

            await renderTask;

            for (int i = 0; i < destinations.Length; i++)
            {
                Marshal.FreeHGlobal(destinations[i]);
            }
        }
    }
}
