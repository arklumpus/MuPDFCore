using Microsoft.VisualStudio.TestTools.UnitTesting;
using MuPDFCore;
using System;

#pragma warning disable IDE0090 // Use 'new(...)'

namespace Tests
{
    [TestClass]
    public class RectanglesTests
    {
        [TestMethod]
        public void SizeSplittingIn3Horiz()
        {
            Size size = new Size(150, 100);

            Rectangle[] splitSize = size.Split(3);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(100, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(100, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(100, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
        }

        [TestMethod]
        public void SizeSplittingIn4Horiz()
        {
            Size size = new Size(150, 100);

            Rectangle[] splitSize = size.Split(4);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(75, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(75, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(75, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(50, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(75, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(50, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(100, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
        }

        [TestMethod]
        public void SizeSplittingIn5Horiz()
        {
            Size size = new Size(150, 100);

            Rectangle[] splitSize = size.Split(5);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(60, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(60, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(50, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(60, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(50, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(100, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(120, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(0, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(100, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");
        }

        [TestMethod]
        public void SizeSplittingIn7Horiz()
        {
            Size size = new Size(140, 100);

            Rectangle[] splitSize = size.Split(7);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(40, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(40, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(40, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(80, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(50, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(40, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(50, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(80, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(100, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(80, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(0, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(50, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");

            Assert.AreEqual(80, splitSize[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(50, splitSize[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[5].X1, "The split coordinate 5.X1 is wrong.");
            Assert.AreEqual(100, splitSize[5].Y1, "The split coordinate 5.Y1 is wrong.");

            Assert.AreEqual(120, splitSize[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(0, splitSize[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(140, splitSize[6].X1, "The split coordinate 6.X1 is wrong.");
            Assert.AreEqual(100, splitSize[6].Y1, "The split coordinate 6.Y1 is wrong.");
        }

        [TestMethod]
        public void SizeSplittingIn3Vert()
        {
            Size size = new Size(100, 150);

            Rectangle[] splitSize = size.Split(3);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(100, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(100, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(150, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
        }

        [TestMethod]
        public void SizeSplittingIn4Vert()
        {
            Size size = new Size(100, 150);

            Rectangle[] splitSize = size.Split(4);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(75, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(75, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(75, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(150, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(75, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(150, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
        }

        [TestMethod]
        public void SizeSplittingIn5Vert()
        {
            Size size = new Size(100, 150);

            Rectangle[] splitSize = size.Split(5);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(60, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(60, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(120, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(60, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(120, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(120, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(150, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");
        }

        [TestMethod]
        public void SizeSplittingIn7Vert()
        {
            Size size = new Size(100, 140);

            Rectangle[] splitSize = size.Split(7);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(40, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(40, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(40, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(80, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(40, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(80, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(80, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(120, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");

            Assert.AreEqual(80, splitSize[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(120, splitSize[5].Y1, "The split coordinate 5.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[5].X1, "The split coordinate 5.X1 is wrong.");

            Assert.AreEqual(120, splitSize[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(140, splitSize[6].Y1, "The split coordinate 6.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[6].X1, "The split coordinate 6.X1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn3Horiz()
        {
            RoundedSize size = new RoundedSize(150, 100);

            RoundedRectangle[] splitSize = size.Split(3);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(100, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(100, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(100, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn4Horiz()
        {
            RoundedSize size = new RoundedSize(150, 100);

            RoundedRectangle[] splitSize = size.Split(4);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(75, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(75, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(75, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(50, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(75, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(50, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(100, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn5Horiz()
        {
            RoundedSize size = new RoundedSize(150, 100);

            RoundedRectangle[] splitSize = size.Split(5);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(60, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(60, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(50, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(60, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(50, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(100, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(120, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(0, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(150, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(100, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn7Horiz()
        {
            RoundedSize size = new RoundedSize(140, 100);

            RoundedRectangle[] splitSize = size.Split(7);

            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(40, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(50, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(0, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(40, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(40, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(0, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(80, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(50, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(40, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(50, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(80, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(100, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(80, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(0, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(50, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");

            Assert.AreEqual(80, splitSize[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(50, splitSize[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(120, splitSize[5].X1, "The split coordinate 5.X1 is wrong.");
            Assert.AreEqual(100, splitSize[5].Y1, "The split coordinate 5.Y1 is wrong.");

            Assert.AreEqual(120, splitSize[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(0, splitSize[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(140, splitSize[6].X1, "The split coordinate 6.X1 is wrong.");
            Assert.AreEqual(100, splitSize[6].Y1, "The split coordinate 6.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn3Vert()
        {
            RoundedSize size = new RoundedSize(100, 150);

            RoundedRectangle[] splitSize = size.Split(3);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(100, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(100, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(100, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(150, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn4Vert()
        {
            RoundedSize size = new RoundedSize(100, 150);

            RoundedRectangle[] splitSize = size.Split(4);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(75, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(75, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(75, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(150, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(75, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(150, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn5Vert()
        {
            RoundedSize size = new RoundedSize(100, 150);

            RoundedRectangle[] splitSize = size.Split(5);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(60, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(60, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(120, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(60, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(120, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(120, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(150, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");
        }

        [TestMethod]
        public void RoundedSizeSplittingIn7Vert()
        {
            RoundedSize size = new RoundedSize(100, 140);

            RoundedRectangle[] splitSize = size.Split(7);

            Assert.AreEqual(0, splitSize[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(40, splitSize[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(0, splitSize[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(40, splitSize[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(40, splitSize[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(80, splitSize[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(40, splitSize[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(80, splitSize[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(80, splitSize[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(120, splitSize[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(50, splitSize[4].X1, "The split coordinate 4.X1 is wrong.");

            Assert.AreEqual(80, splitSize[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(50, splitSize[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(120, splitSize[5].Y1, "The split coordinate 5.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[5].X1, "The split coordinate 5.X1 is wrong.");

            Assert.AreEqual(120, splitSize[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(0, splitSize[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(140, splitSize[6].Y1, "The split coordinate 6.Y1 is wrong.");
            Assert.AreEqual(100, splitSize[6].X1, "The split coordinate 6.X1 is wrong.");
        }

        [TestMethod]
        public void RectangleWidthHeight()
        {
            Rectangle rect = new Rectangle(123, 456, 789, 1011);

            Assert.AreEqual(666, rect.Width, "The rectangle's width is wrong.");
            Assert.AreEqual(555, rect.Height, "The rectangle's height is wrong.");
        }

        [TestMethod]
        public void RectangleRounding()
        {
            Rectangle rect = new Rectangle(123.3, 456.4, 789.5, 1011.6);

            RoundedRectangle rounded = rect.Round();

            Assert.AreEqual(123, rounded.X0, "The rectangle's width is wrong.");
            Assert.AreEqual(456, rounded.Y0, "The rectangle's height is wrong.");
            Assert.AreEqual(790, rounded.X1, "The rectangle's width is wrong.");
            Assert.AreEqual(1012, rounded.Y1, "The rectangle's height is wrong.");
        }

        [TestMethod]
        public void RectangleRoundingWithZoom()
        {
            Rectangle rect = new Rectangle(123.3, 456.4, 789.5, 1011.6);

            RoundedRectangle rounded = rect.Round(Math.Sqrt(2));

            Assert.AreEqual(174, rounded.X0, "The rectangle's width is wrong.");
            Assert.AreEqual(645, rounded.Y0, "The rectangle's height is wrong.");
            Assert.AreEqual(1117, rounded.X1, "The rectangle's width is wrong.");
            Assert.AreEqual(1431, rounded.Y1, "The rectangle's height is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn3Horiz()
        {
            Rectangle rect = new Rectangle(10, 10, 160, 110);

            Rectangle[] splitRect = rect.Split(3);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(110, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(110, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(110, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn4Horiz()
        {
            Rectangle rect = new Rectangle(10, 10, 160, 110);

            Rectangle[] splitRect = rect.Split(4);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(85, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(85, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(85, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(60, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(85, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(60, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(110, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn5Horiz()
        {
            Rectangle rect = new Rectangle(10, 10, 160, 110);

            Rectangle[] splitRect = rect.Split(5);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(70, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(70, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(70, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(60, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(70, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(60, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(110, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(130, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(10, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(110, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn7Horiz()
        {
            Rectangle rect = new Rectangle(10, 10, 150, 110);

            Rectangle[] splitRect = rect.Split(7);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(50, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(50, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(90, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(60, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(50, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(60, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(90, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(110, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(90, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(10, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(60, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");

            Assert.AreEqual(90, splitRect[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(60, splitRect[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[5].X1, "The split coordinate 5.X1 is wrong.");
            Assert.AreEqual(110, splitRect[5].Y1, "The split coordinate 5.Y1 is wrong.");

            Assert.AreEqual(130, splitRect[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(10, splitRect[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(150, splitRect[6].X1, "The split coordinate 6.X1 is wrong.");
            Assert.AreEqual(110, splitRect[6].Y1, "The split coordinate 6.Y1 is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn3Vert()
        {
            Rectangle rect = new Rectangle(10, 10, 110, 160);

            Rectangle[] splitRect = rect.Split(3);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(110, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(110, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(160, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn4Vert()
        {
            Rectangle rect = new Rectangle(10, 10, 110, 160);

            Rectangle[] splitRect = rect.Split(4);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(85, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(85, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(85, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(160, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(85, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(160, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn5Vert()
        {
            Rectangle rect = new Rectangle(10, 10, 110, 160);

            Rectangle[] splitRect = rect.Split(5);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(70, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(70, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(70, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(130, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(70, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(130, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(130, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(160, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");
        }

        [TestMethod]
        public void RectangleSplittingIn7Vert()
        {
            Rectangle rect = new Rectangle(10, 10, 110, 150);

            Rectangle[] splitRect = rect.Split(7);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(50, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(50, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(90, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(50, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(90, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(90, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(130, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");

            Assert.AreEqual(90, splitRect[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(130, splitRect[5].Y1, "The split coordinate 5.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[5].X1, "The split coordinate 5.X1 is wrong.");

            Assert.AreEqual(130, splitRect[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(150, splitRect[6].Y1, "The split coordinate 6.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[6].X1, "The split coordinate 6.X1 is wrong.");
        }

        [TestMethod]
        public void RectangleIntersection()
        {
            Rectangle rect1 = new Rectangle(0, 0, 100, 150);
            Rectangle rect2 = new Rectangle(40, 70, 140, 200);
            Rectangle rect3 = new Rectangle(110, 20, 160, 140);

            Rectangle intersection = rect1.Intersect(rect2);

            Assert.AreEqual(40, intersection.X0, "The 1-2 intersection X0 is wrong.");
            Assert.AreEqual(70, intersection.Y0, "The 1-2 intersection Y0 is wrong.");
            Assert.AreEqual(100, intersection.X1, "The 1-2 intersection X1 is wrong.");
            Assert.AreEqual(150, intersection.Y1, "The 1-2 intersection Y1 is wrong.");

            intersection = rect1.Intersect(rect3);

            Assert.AreEqual(0, intersection.Width, "The 1-3 intersection width is wrong.");
            Assert.AreEqual(0, intersection.Height, "The 1-3 intersection height is wrong.");

            intersection = rect2.Intersect(rect3);

            Assert.AreEqual(110, intersection.X0, "The 2-3 intersection X0 is wrong.");
            Assert.AreEqual(70, intersection.Y0, "The 2-3 intersection Y0 is wrong.");
            Assert.AreEqual(140, intersection.X1, "The 2-3 intersection X1 is wrong.");
            Assert.AreEqual(140, intersection.Y1, "The 2-3 intersection Y1 is wrong.");
        }

        [TestMethod]
        public void RectangleContainingRectangle()
        {
            Rectangle rect1 = new Rectangle(10, 10, 100, 150);
            Rectangle rect2 = new Rectangle(40, 70, 140, 200);
            Rectangle rect3 = new Rectangle(110, 20, 160, 140);
            Rectangle rect4 = new Rectangle(30, 30, 50, 50);
            Rectangle rect5 = new Rectangle(0, 0, 110, 160);

            Assert.IsFalse(rect1.Contains(rect2), "Rectangle 1 contains rectangle 2 (it shouldn't).");
            Assert.IsFalse(rect1.Contains(rect3), "Rectangle 1 contains rectangle 3 (it shouldn't).");
            Assert.IsTrue(rect1.Contains(rect4), "Rectangle 1 doesn't contain rectangle 4 (it should).");
            Assert.IsFalse(rect1.Contains(rect5), "Rectangle 1 contains rectangle 5 (it shouldn't).");
            Assert.IsTrue(rect5.Contains(rect1), "Rectangle 5 doesn't contain rectangle 1 (it should).");
        }

        [TestMethod]
        public void RectangleContainingPoint()
        {
            Rectangle rect = new Rectangle(10, 10, 100, 150);
            PointF p1 = new PointF(5, 5);
            PointF p2 = new PointF(50, 5);
            PointF p3 = new PointF(110, 5);

            PointF p4 = new PointF(5, 50);
            PointF p5 = new PointF(50, 50);
            PointF p6 = new PointF(110, 50);

            PointF p7 = new PointF(5, 155);
            PointF p8 = new PointF(50, 155);
            PointF p9 = new PointF(110, 155);

            PointF p10 = new PointF(50, 10);
            PointF p11 = new PointF(10, 75);
            PointF p12 = new PointF(50, 150);
            PointF p13 = new PointF(100, 75);

            Assert.IsFalse(rect.Contains(p1), "The rectangle contains point 1 (it shouldn't).");
            Assert.IsFalse(rect.Contains(p2), "The rectangle contains point 2 (it shouldn't).");
            Assert.IsFalse(rect.Contains(p3), "The rectangle contains point 3 (it shouldn't).");

            Assert.IsFalse(rect.Contains(p4), "The rectangle contains point 4 (it shouldn't).");
            Assert.IsTrue(rect.Contains(p5), "The rectangle doesn't contain point 5 (it should).");
            Assert.IsFalse(rect.Contains(p6), "The rectangle contains point 6 (it shouldn't).");

            Assert.IsFalse(rect.Contains(p7), "The rectangle contains point 7 (it shouldn't).");
            Assert.IsFalse(rect.Contains(p8), "The rectangle contains point 8 (it shouldn't).");
            Assert.IsFalse(rect.Contains(p9), "The rectangle contains point 9 (it shouldn't).");

            Assert.IsTrue(rect.Contains(p10), "The rectangle doesn't contain point 10 (it should).");
            Assert.IsTrue(rect.Contains(p11), "The rectangle doesn't contain point 11 (it should).");
            Assert.IsTrue(rect.Contains(p12), "The rectangle doesn't contain point 12 (it should).");
            Assert.IsTrue(rect.Contains(p13), "The rectangle doesn't contain point 13 (it should).");
        }

        [TestMethod]
        public void RectangleToQuad()
        {
            Rectangle rect = new Rectangle(10, 10, 100, 150);
            Quad quad = rect.ToQuad();

            Assert.AreEqual(quad.LowerLeft, new PointF(10, 150), "The lower left corner of the quad does not correspond to the rectangle.");
            Assert.AreEqual(quad.UpperLeft, new PointF(10, 10), "The upper left corner of the quad does not correspond to the rectangle.");
            Assert.AreEqual(quad.UpperRight, new PointF(100, 10), "The upper right corner of the quad does not correspond to the rectangle.");
            Assert.AreEqual(quad.LowerRight, new PointF(100, 150), "The lower right corner of the quad does not correspond to the rectangle.");
        }

        [TestMethod]
        public void RoundedRectangleWidthHeight()
        {
            RoundedRectangle rect = new RoundedRectangle(123, 456, 789, 1011);

            Assert.AreEqual(666, rect.Width, "The rectangle's width is wrong.");
            Assert.AreEqual(555, rect.Height, "The rectangle's height is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn3Horiz()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 160, 110);

            RoundedRectangle[] splitRect = rect.Split(3);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(110, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(110, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(110, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn4Horiz()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 160, 110);

            RoundedRectangle[] splitRect = rect.Split(4);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(85, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(85, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(85, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(60, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(85, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(60, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(110, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn5Horiz()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 160, 110);

            RoundedRectangle[] splitRect = rect.Split(5);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(70, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(70, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(70, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(60, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(70, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(60, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(110, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(130, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(10, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(160, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(110, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn7Horiz()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 150, 110);

            RoundedRectangle[] splitRect = rect.Split(7);

            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(50, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");
            Assert.AreEqual(60, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");

            Assert.AreEqual(10, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(60, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(50, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");

            Assert.AreEqual(50, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(10, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(90, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
            Assert.AreEqual(60, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");

            Assert.AreEqual(50, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(60, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(90, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
            Assert.AreEqual(110, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");

            Assert.AreEqual(90, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(10, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");
            Assert.AreEqual(60, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");

            Assert.AreEqual(90, splitRect[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(60, splitRect[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(130, splitRect[5].X1, "The split coordinate 5.X1 is wrong.");
            Assert.AreEqual(110, splitRect[5].Y1, "The split coordinate 5.Y1 is wrong.");

            Assert.AreEqual(130, splitRect[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(10, splitRect[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(150, splitRect[6].X1, "The split coordinate 6.X1 is wrong.");
            Assert.AreEqual(110, splitRect[6].Y1, "The split coordinate 6.Y1 is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn3Vert()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 110, 160);

            RoundedRectangle[] splitRect = rect.Split(3);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(110, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(110, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(110, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(160, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn4Vert()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 110, 160);

            RoundedRectangle[] splitRect = rect.Split(4);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(85, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(85, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(85, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(160, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(85, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(160, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn5Vert()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 110, 160);

            RoundedRectangle[] splitRect = rect.Split(5);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(70, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(70, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(70, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(130, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(70, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(130, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(130, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(160, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");
        }

        [TestMethod]
        public void RoundedRectangleSplittingIn7Vert()
        {
            RoundedRectangle rect = new RoundedRectangle(10, 10, 110, 150);

            RoundedRectangle[] splitRect = rect.Split(7);

            Assert.AreEqual(10, splitRect[0].Y0, "The split coordinate 0.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[0].X0, "The split coordinate 0.X0 is wrong.");
            Assert.AreEqual(50, splitRect[0].Y1, "The split coordinate 0.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[0].X1, "The split coordinate 0.X1 is wrong.");

            Assert.AreEqual(10, splitRect[1].Y0, "The split coordinate 1.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[1].X0, "The split coordinate 1.X0 is wrong.");
            Assert.AreEqual(50, splitRect[1].Y1, "The split coordinate 1.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[1].X1, "The split coordinate 1.X1 is wrong.");

            Assert.AreEqual(50, splitRect[2].Y0, "The split coordinate 2.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[2].X0, "The split coordinate 2.X0 is wrong.");
            Assert.AreEqual(90, splitRect[2].Y1, "The split coordinate 2.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[2].X1, "The split coordinate 2.X1 is wrong.");

            Assert.AreEqual(50, splitRect[3].Y0, "The split coordinate 3.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[3].X0, "The split coordinate 3.X0 is wrong.");
            Assert.AreEqual(90, splitRect[3].Y1, "The split coordinate 3.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[3].X1, "The split coordinate 3.X1 is wrong.");

            Assert.AreEqual(90, splitRect[4].Y0, "The split coordinate 4.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[4].X0, "The split coordinate 4.X0 is wrong.");
            Assert.AreEqual(130, splitRect[4].Y1, "The split coordinate 4.Y1 is wrong.");
            Assert.AreEqual(60, splitRect[4].X1, "The split coordinate 4.X1 is wrong.");

            Assert.AreEqual(90, splitRect[5].Y0, "The split coordinate 5.Y0 is wrong.");
            Assert.AreEqual(60, splitRect[5].X0, "The split coordinate 5.X0 is wrong.");
            Assert.AreEqual(130, splitRect[5].Y1, "The split coordinate 5.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[5].X1, "The split coordinate 5.X1 is wrong.");

            Assert.AreEqual(130, splitRect[6].Y0, "The split coordinate 6.Y0 is wrong.");
            Assert.AreEqual(10, splitRect[6].X0, "The split coordinate 6.X0 is wrong.");
            Assert.AreEqual(150, splitRect[6].Y1, "The split coordinate 6.Y1 is wrong.");
            Assert.AreEqual(110, splitRect[6].X1, "The split coordinate 6.X1 is wrong.");
        }

        [TestMethod]
        public void QuadContainsPoint()
        {
            Quad quad = new Quad(new PointF(10, 40), new PointF(60, 10), new PointF(100, 55), new PointF(45, 100));

            PointF p1 = new PointF(10, 20);
            PointF p2 = new PointF(60, 20);
            PointF p3 = new PointF(100, 20);

            PointF p4 = new PointF(10, 50);
            PointF p5 = new PointF(70, 50);
            PointF p6 = new PointF(100, 50);

            PointF p7 = new PointF(10, 90);
            PointF p8 = new PointF(50, 90);
            PointF p9 = new PointF(100, 90);

            Assert.IsFalse(quad.Contains(p1), "The quad contains point 1 (it shouldn't).");
            Assert.IsTrue(quad.Contains(p2), "The quad doesn't contain point 2 (it should).");
            Assert.IsFalse(quad.Contains(p3), "The quad contains point 3 (it shouldn't).");

            Assert.IsFalse(quad.Contains(p4), "The quad contains point 4 (it shouldn't).");
            Assert.IsTrue(quad.Contains(p5), "The quad doesn't contain point 5 (it should).");
            Assert.IsFalse(quad.Contains(p6), "The quad contains point 6 (it shouldn't).");

            Assert.IsFalse(quad.Contains(p7), "The quad contains point 7 (it shouldn't).");
            Assert.IsTrue(quad.Contains(p8), "The quad doesn't contain point 8 (it should).");
            Assert.IsFalse(quad.Contains(p9), "The quad contains point 9 (it shouldn't).");
        }
    }
}
