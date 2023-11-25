using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class MuPDFContextTests
    {
        [TestMethod]
        public void MuPDFContextCreationWithDefaultStoreSize()
        {
            using MuPDFContext context = new MuPDFContext();
            Assert.AreNotEqual(IntPtr.Zero, context.NativeContext, "The native context pointer is null.");
        }

        [TestMethod]
        public void MuPDFContextCreationWithEmptyStoreSize()
        {
            using MuPDFContext context = new MuPDFContext((uint)0);
            Assert.AreNotEqual(IntPtr.Zero, context.NativeContext, "The native context pointer is null.");
        }

        [TestMethod]
        public void MuPDFContextCurrentStoreSizeGetter()
        {
            using MuPDFContext context = new MuPDFContext();
            Assert.AreEqual(0, context.StoreSize, "MuPDFContext.StoreSize returned the wrong store size.");
        }

        [TestMethod]
        public void MuPDFContextMaxStoreSizeGetter()
        {
            using MuPDFContext context = new MuPDFContext();
            Assert.AreEqual(256 << 20, context.StoreMaxSize, "MuPDFContext.StoreMaxSize returned the wrong store size.");
        }

        [TestMethod]
        public void MuPDFContextStoreShrinkageWhenEmpty()
        {
            using MuPDFContext context = new MuPDFContext();
            context.ShrinkStore(1);
            context.ShrinkStore(0.5);
            context.ShrinkStore(0);
        }

        [TestMethod]
        public void MuPDFContextStoreEmptyingWhenEmpty()
        {
            using MuPDFContext context = new MuPDFContext();
            context.ClearStore();
            Assert.AreEqual(0, context.StoreSize, "The size of the store is not 0.");
        }

        [TestMethod]
        public void MuPDFContextGetAntialiasing()
        {
            using MuPDFContext context = new MuPDFContext();
            Assert.AreEqual(8, context.GraphicsAntiAliasing, "The graphics anti-aliasing level is not 8.");
            Assert.AreEqual(8, context.TextAntiAliasing, "The text anti-aliasing level is not 8.");
        }

        [TestMethod]
        public void MuPDFContextSetAntialiasing()
        {
            using MuPDFContext context = new MuPDFContext();
            context.AntiAliasing = 4;
            Assert.AreEqual(4, context.GraphicsAntiAliasing, "The graphics anti-aliasing level was not set correctly.");
            Assert.AreEqual(4, context.TextAntiAliasing, "The text anti-aliasing level was not set correctly.");
        }

        [TestMethod]
        public void MuPDFContextSetGraphicsAntialiasing()
        {
            using MuPDFContext context = new MuPDFContext();
            context.GraphicsAntiAliasing = 4;
            Assert.AreEqual(4, context.GraphicsAntiAliasing, "The graphics anti-aliasing level was not set correctly.");
            Assert.AreEqual(8, context.TextAntiAliasing, "The text anti-aliasing level is not 8.");
        }

        [TestMethod]
        public void MuPDFContextSetTextAntialiasing()
        {
            using MuPDFContext context = new MuPDFContext();
            context.TextAntiAliasing = 4;
            Assert.AreEqual(8, context.GraphicsAntiAliasing, "The graphics anti-aliasing level is not 8.");
            Assert.AreEqual(4, context.TextAntiAliasing, "The text anti-aliasing level was not set correctly.");
        }

        [TestMethod]
        public void MuPDFContextSetAntialiasingOutOfRange()
        {
            using MuPDFContext context = new MuPDFContext();
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => context.AntiAliasing = 9, "Setting an invalid anti-aliasing level did not fail!");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => context.GraphicsAntiAliasing = 9, "Setting an invalid graphics anti-aliasing level did not fail!");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => context.TextAntiAliasing = 9, "Setting an invalid text anti-aliasing level did not fail!");
        }
    }
}
