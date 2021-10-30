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
            using MuPDFContext context = new MuPDFContext(0);
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
    }
}
