using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace MuPDFCore
{
    /// <summary>
    /// Represents an image embedded within a document.
    /// </summary>
    public class MuPDFImage
    {
        /// <summary>
        /// Describes the orientation of the image (as encoded within the image file).
        /// </summary>
        public enum ImageOrientation
        {
            /// <summary>
            /// Undefined.
            /// </summary>
            Undefined = 0,

            /// <summary>
            /// 0 degree counter clockwise rotation.
            /// </summary>
            CCW_0_Degrees = 1,

            /// <summary>
            /// 90 degree counter clockwise rotation.
            /// </summary>
            CCW_90_Degrees = 2,

            /// <summary>
            /// 180 degree counter clockwise rotation.
            /// </summary>
            CCW_180_Degrees = 3,

            /// <summary>
            /// 270 degree counter clockwise rotation.
            /// </summary>
            CCW_270_Degrees = 4,

            /// <summary>
            /// Flip on X.
            /// </summary>
            FlipX = 5,

            /// <summary>
            /// Flip on X, then rotate counter clockwise by 90 degrees.
            /// </summary>
            FlipX_CCW_90_Degrees = 6,

            /// <summary>
            /// Flip on X, then rotate counter clockwise by 180 degrees.
            /// </summary>
            FlipX_CCW_180_Degrees = 7,

            /// <summary>
            /// Flip on X, then rotate counter clockwise by 270 degrees.
            /// </summary>
            FlipX_CCW_270_Degrees = 8
        }

        /// <summary>
        /// Width of the image in pixels.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of the image in pixels.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Horizontal resolution of the image.
        /// </summary>
        public int XRes { get; }

        /// <summary>
        /// Vertical resolution of the image.
        /// </summary>
        public int YRes { get; }

        /// <summary>
        /// Orientation of the image (as encoded within the image file).
        /// </summary>
        public ImageOrientation Orientation { get; }

        /// <summary>
        /// The colour space in which the image is defined.
        /// </summary>
        public MuPDFColorSpace ColorSpace { get; }

        private IntPtr NativePointer { get; }
        private IntPtr NativeContext { get; }

        internal MuPDFImage(IntPtr nativePointer, MuPDFContext context)
        {
            this.NativePointer = nativePointer;
            this.NativeContext = context.NativeContext;

            int w = 0;
            int h = 0;
            int xres = 0;
            int yres = 0;
            byte orientation = 0;
            IntPtr colorspace = IntPtr.Zero;

            ExitCodes result = (ExitCodes)NativeMethods.GetImageMetadata(context.NativeContext, nativePointer, ref w, ref h, ref xres, ref yres, ref orientation, ref colorspace);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_IMAGE_METADATA:
                    throw new MuPDFException("Error while gathering image metadata.", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            this.Width = w;
            this.Height = h;
            this.XRes = xres;
            this.YRes = yres;

            this.Orientation = (ImageOrientation)orientation;

            this.ColorSpace = MuPDFColorSpace.Create(this.NativeContext, colorspace);
        }

        /// <summary>
        /// Save the image to a file.
        /// </summary>
        /// <param name="fileName">The name of the output file.</param>
        /// <param name="fileType">The output file format.</param>
        /// <param name="convertToRGB">If this is <see langword="true"/>, the image is converted to the RGB colour space before being saved. If this is <see langword="false"/>, the image is saved in the same colour space as it was encoded in the document. If this is <see langword="null"/> (the default), the image is converted to RGB only if the target colour space does not support the colour space of the image.</param>
        /// <exception cref="MuPDFException">Thrown if an error occurs while rendering the image or saving it.</exception>
        /// <exception cref="ArgumentException">Thrown if attempting to export an image in a format that does not support the colour space of the image.</exception>
        public void Save(string fileName, RasterOutputFileTypes fileType, bool? convertToRGB = null)
        {
            if (this.ColorSpace.RootColorSpace.Type == ColorSpaceType.CMYK && (fileType == RasterOutputFileTypes.PNG || fileType == RasterOutputFileTypes.PNM))
            {
                if (convertToRGB == false)
                {
                    throw new ArgumentException("Images in CMYK colour space cannot be exported to PNG or PNM files!");
                }
                else if (convertToRGB == null)
                {
                    convertToRGB = true;
                }
            }
            else if (this.ColorSpace.RootColorSpace.Type == ColorSpaceType.Separation && fileType != RasterOutputFileTypes.PSD)
            {
                if (convertToRGB == false)
                {
                    throw new ArgumentException("Images in Separation colour space can only be exported in PSD format!");
                }
                else if (convertToRGB == null)
                {
                    convertToRGB = true;
                }
            }

            ExitCodes result = (ExitCodes)NativeMethods.SaveRasterImage(this.NativeContext, this.NativePointer, fileName, (int)fileType, 90, convertToRGB == true ? 1 : 0);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("An error occurred while rendering the image.", result);
                case ExitCodes.ERR_CANNOT_SAVE:
                    throw new MuPDFException("An error occurred while saving the image.", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }
        }

        /// <summary>
        /// Save the image to a JPEG file.
        /// </summary>
        /// <param name="fileName">The name of the output file.</param>
        /// <param name="quality">The quality of the JPEG image (from 0 to 100).</param>
        /// <param name="convertToRGB">If this is <see langword="true"/>, the image is converted to the RGB colour space before being saved. If this is <see langword="false"/>, the image is saved in the same colour space as it was encoded in the document. If this is <see langword="null"/> (the default), the image is converted to RGB only if the target colour space does not support the colour space of the image.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the quality value is &lt; 0 or &gt; 0</exception>
        /// <exception cref="MuPDFException">Thrown if an error occurs while rendering the image or saving it.</exception>
        public void SaveAsJPEG(string fileName, int quality, bool? convertToRGB = null)
        {
            if (quality < 0 || quality > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(quality), quality, "The JPEG quality must range between 0 and 100 (inclusive)!");
            }

            if (this.ColorSpace.RootColorSpace.Type == ColorSpaceType.Separation)
            {
                if (convertToRGB == false)
                {
                    throw new ArgumentException("Images in Separation colour space can only be exported in PSD format!");
                }
                else if (convertToRGB == null)
                {
                    convertToRGB = true;
                }
            }

            ExitCodes result = (ExitCodes)NativeMethods.SaveRasterImage(this.NativeContext, this.NativePointer, fileName, (int)RasterOutputFileTypes.JPEG, quality, convertToRGB == true ? 1 : 0);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("An error occurred while rendering the image.", result);
                case ExitCodes.ERR_CANNOT_SAVE:
                    throw new MuPDFException("An error occurred while saving the image.", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }
        }

        /// <summary>
        /// Write the image to a <see cref="Stream"/>.
        /// </summary>
        /// <param name="outputStream">The output <see cref="Stream"/>.</param>
        /// <param name="fileType">The image format.</param>
        /// <param name="convertToRGB">If this is <see langword="true"/>, the image is converted to the RGB colour space before being saved. If this is <see langword="false"/>, the image is saved in the same colour space as it was encoded in the document. If this is <see langword="null"/> (the default), the image is converted to RGB only if the target colour space does not support the colour space of the image.</param>
        /// <exception cref="MuPDFException">Thrown if an error occurs while rendering the image or saving it.</exception>
        public void Write(Stream outputStream, RasterOutputFileTypes fileType, bool? convertToRGB = null)
        {
            if (this.ColorSpace.RootColorSpace.Type == ColorSpaceType.CMYK && (fileType == RasterOutputFileTypes.PNG || fileType == RasterOutputFileTypes.PNM))
            {
                if (convertToRGB == false)
                {
                    throw new ArgumentException("Images in CMYK colour space cannot be exported to PNG or PNM files!");
                }
                else if (convertToRGB == null)
                {
                    convertToRGB = true;
                }
            }
            else if (this.ColorSpace.RootColorSpace.Type == ColorSpaceType.Separation && fileType != RasterOutputFileTypes.PSD)
            {
                if (convertToRGB == false)
                {
                    throw new ArgumentException("Images in Separation colour space can only be exported in PSD format!");
                }
                else if (convertToRGB == null)
                {
                    convertToRGB = true;
                }
            }

            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            ExitCodes result = (ExitCodes)NativeMethods.WriteRasterImage(this.NativeContext, this.NativePointer, (int)fileType, 90, ref outputBuffer, ref outputData, ref outputDataLength, convertToRGB == true ? 1 : 0);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                case ExitCodes.ERR_CANNOT_CREATE_BUFFER:
                    throw new MuPDFException("Cannot create the output buffer", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            byte[] buffer = new byte[1024];

            while (outputDataLength > 0)
            {
                int bytesToCopy = (int)Math.Min(buffer.Length, (long)outputDataLength);
                Marshal.Copy(outputData, buffer, 0, bytesToCopy);
                outputData = IntPtr.Add(outputData, bytesToCopy);
                outputStream.Write(buffer, 0, bytesToCopy);
                outputDataLength -= (ulong)bytesToCopy;
            }

            NativeMethods.DisposeBuffer(this.NativeContext, outputBuffer);
        }

        /// <summary>
        /// Write the image to a <see cref="Stream"/> in JPEG format.
        /// </summary>
        /// <param name="outputStream">The output <see cref="Stream"/>.</param>
        /// <param name="quality">The quality of the JPEG image (from 0 to 100).</param>
        /// <param name="convertToRGB">If this is <see langword="true"/>, the image is converted to the RGB colour space before being saved. If this is <see langword="false"/>, the image is saved in the same colour space as it was encoded in the document. If this is <see langword="null"/> (the default), the image is converted to RGB only if the target colour space does not support the colour space of the image.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the quality value is &lt; 0 or &gt; 0</exception>
        /// <exception cref="MuPDFException">Thrown if an error occurs while rendering the image or saving it.</exception>
        public void WriteAsJPEG(Stream outputStream, int quality, bool? convertToRGB = null)
        {
            if (quality < 0 || quality > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(quality), quality, "The JPEG quality must range between 0 and 100 (inclusive)!");
            }

            if (this.ColorSpace.RootColorSpace.Type == ColorSpaceType.Separation)
            {
                if (convertToRGB == false)
                {
                    throw new ArgumentException("Images in Separation colour space can only be exported in PSD format!");
                }
                else if (convertToRGB == null)
                {
                    convertToRGB = true;
                }
            }

            IntPtr outputBuffer = IntPtr.Zero;
            IntPtr outputData = IntPtr.Zero;
            ulong outputDataLength = 0;

            ExitCodes result = (ExitCodes)NativeMethods.WriteRasterImage(this.NativeContext, this.NativePointer, (int)RasterOutputFileTypes.JPEG, quality, ref outputBuffer, ref outputData, ref outputDataLength, convertToRGB == true ? 1 : 0);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                case ExitCodes.ERR_CANNOT_CREATE_BUFFER:
                    throw new MuPDFException("Cannot create the output buffer", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            byte[] buffer = new byte[1024];

            while (outputDataLength > 0)
            {
                int bytesToCopy = (int)Math.Min(buffer.Length, (long)outputDataLength);
                Marshal.Copy(outputData, buffer, 0, bytesToCopy);
                outputData = IntPtr.Add(outputData, bytesToCopy);
                outputStream.Write(buffer, 0, bytesToCopy);
                outputDataLength -= (ulong)bytesToCopy;
            }

            NativeMethods.DisposeBuffer(this.NativeContext, outputBuffer);
        }

        /// <summary>
        /// Get a byte representation of the image pixels.
        /// </summary>
        /// <returns>A byte representation of the image pixels in the specified pixel format.</returns>
        /// <exception cref="MuPDFException">Thrown if an error occurs while rendering the image.</exception>
        public unsafe byte[] GetBytes(PixelFormats pixelFormat)
        {
            IntPtr pixmap = IntPtr.Zero;
            IntPtr samples = IntPtr.Zero;
            int sampleCount = 0;

            ExitCodes result = (ExitCodes)NativeMethods.LoadPixmapRGB(this.NativeContext, this.NativePointer, (int)pixelFormat, ref pixmap, ref samples, ref sampleCount);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            bool renderedHasAlpha = sampleCount / (this.Width * this.Height) == 4;

            byte[] tbr;

            if (!renderedHasAlpha && (pixelFormat == PixelFormats.RGBA || pixelFormat == PixelFormats.BGRA))
            {
                tbr = new byte[this.Width * this.Height * 4];

                byte* renderedPixels = (byte*)samples;

                for (int i = 0; i < this.Width * this.Height; i++)
                {
                    tbr[i * 4] = renderedPixels[i * 3];
                    tbr[i * 4 + 1] = renderedPixels[i * 3 + 1];
                    tbr[i * 4 + 2] = renderedPixels[i * 3 + 2];
                    tbr[i * 4 + 3] = 255;
                }
            }
            else
            {
                tbr = new byte[sampleCount];
                fixed (byte* ptr = tbr)
                {
                    Buffer.MemoryCopy((byte*)samples, ptr, sampleCount, sampleCount);
                }
            }

            NativeMethods.DisposePixmap(this.NativeContext, pixmap);

            return tbr;
        }

        /// <summary>
        /// Get a byte representation of the image pixels.
        /// </summary>
        /// <returns>A byte representation of the image pixels, in the native image colour space.</returns>
        /// <exception cref="MuPDFException">Thrown if an error occurs while rendering the image.</exception>
        public unsafe byte[] GetBytes()
        {
            IntPtr pixmap = IntPtr.Zero;
            IntPtr samples = IntPtr.Zero;
            int sampleCount = 0;

            ExitCodes result = (ExitCodes)NativeMethods.LoadPixmap(this.NativeContext, this.NativePointer, ref pixmap, ref samples, ref sampleCount);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_RENDER:
                    throw new MuPDFException("Cannot render page", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }

            byte[] tbr = new byte[sampleCount];

            fixed (byte* ptr = tbr)
            {
                Buffer.MemoryCopy((byte*)samples, ptr, sampleCount, sampleCount);
            }

            NativeMethods.DisposePixmap(this.NativeContext, pixmap);

            return tbr;
        }
    }
}
