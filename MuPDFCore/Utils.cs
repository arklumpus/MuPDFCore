/*
    MuPDFCore - A set of multiplatform .NET Core bindings for MuPDF.
    Copyright (C) 2020  Giorgio Bianchini

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

using System;

namespace MuPDFCore
{
    /// <summary>
    /// Contains useful methods.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// The factors by which we can divide a rectangle/size.
        /// </summary>
        public static readonly int[] AcceptableDivisors = new[] { 2, 3, 5, 7 };

        /// <summary>
        /// Computes the biggest number smaller than or equal to the specified value that is factorisable using only the <see cref="AcceptableDivisors"/>.
        /// </summary>
        /// <param name="n">The maximum number.</param>
        /// <returns>A number that is &lt;= <paramref name="n"/> and is factorisable using only the <see cref="AcceptableDivisors"/>.</returns>
        public static int GetAcceptableNumber(int n)
        {
            for (int i = n; n >= 1; n--)
            {
                if (IsAcceptableNumber(i))
                {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(n), n, "The number must be strictly higher than 0!");
        }

        /// <summary>
        /// Determine whether a number is factorisable using only the <see cref="AcceptableDivisors"/>.
        /// </summary>
        /// <param name="n">The number to analyse.</param>
        /// <returns>A boolean value indicating whether the number is factorisable using only the <see cref="AcceptableDivisors"/>.</returns>
        public static bool IsAcceptableNumber(int n)
        {
            if (n == 0)
            {
                return false;
            }
            else if (n == 1)
            {
                return true;
            }
            else
            {
                for (int i = 0; i < AcceptableDivisors.Length; i++)
                {
                    bool divided = false;

                    while (n % AcceptableDivisors[i] == 0)
                    {
                        n /= AcceptableDivisors[i];
                        divided = true;
                    }

                    if (divided)
                    {
                        return IsAcceptableNumber(n);
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Clear all pixels outside of a specified region.
        /// </summary>
        /// <param name="image">A pointer to the address where the pixel data is stored.</param>
        /// <param name="imageSize">The size in pixels of the image.</param>
        /// <param name="imageArea">The area represented by the image.</param>
        /// <param name="clipArea">The region outside which all pixels will be cleared, in image units.</param>
        /// <param name="pixelFormat">The format of the pixel data.</param>
        public static void ClipImage(IntPtr image, RoundedSize imageSize, Rectangle imageArea, Rectangle clipArea, PixelFormats pixelFormat)
        {
            int clipLeft = Math.Max(0, (int)Math.Ceiling((clipArea.X0 - imageArea.X0) / imageArea.Width * imageSize.Width - 0.001));
            int clipRight = Math.Max(0, (int)Math.Floor(imageSize.Width - (imageArea.X1 - clipArea.X1) / imageArea.Width * imageSize.Width + 0.001));

            int clipTop = Math.Max(0, (int)Math.Ceiling((clipArea.Y0 - imageArea.Y0) / imageArea.Height * imageSize.Height - 0.001));
            int clipBottom = Math.Max(0, (int)Math.Floor(imageSize.Height - (imageArea.Y1 - clipArea.Y1) / imageArea.Height * imageSize.Height + 0.001));

            int pixelSize = -1;
            byte clearValue = 0;

            switch (pixelFormat)
            {
                case PixelFormats.RGB:
                case PixelFormats.BGR:
                    pixelSize = 3;
                    clearValue = 255;
                    break;
                case PixelFormats.RGBA:
                case PixelFormats.BGRA:
                    pixelSize = 4;
                    clearValue = 0;
                    break;
            }

            int stride = imageSize.Width * pixelSize;

            if (clipLeft > 0 || clipRight < imageSize.Width || clipTop > 0 || clipBottom < imageSize.Height)
            {
                unsafe
                {
                    byte* imageData = (byte*)image;

                    for (int y = 0; y < imageSize.Height; y++)
                    {
                        if (y < clipTop || y >= clipBottom)
                        {
                            for (int x = 0; x < imageSize.Width; x++)
                            {
                                for (int i = 0; i < pixelSize; i++)
                                {
                                    imageData[y * stride + x * pixelSize + i] = clearValue;
                                }
                            }
                        }
                        else
                        {
                            for (int x = 0; x < Math.Min(clipLeft, imageSize.Width); x++)
                            {
                                for (int i = 0; i < pixelSize; i++)
                                {
                                    imageData[y * stride + x * pixelSize + i] = clearValue;
                                }
                            }

                            for (int x = Math.Max(0, clipRight); x < imageSize.Width; x++)
                            {
                                for (int i = 0; i < pixelSize; i++)
                                {
                                    imageData[y * stride + x * pixelSize + i] = clearValue;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Converts an image with premultiplied alpha values into an image with unpremultiplied alpha values.
        /// </summary>
        /// <param name="image">A pointer to the address where the pixel data is stored.</param>
        /// <param name="imageSize">The size in pixels of the image.</param>
        public static void UnpremultiplyAlpha(IntPtr image, RoundedSize imageSize)
        {
            int stride = imageSize.Width * 4;

            unsafe
            {
                byte* imageData = (byte*)image;

                for (int y = 0; y < imageSize.Height; y++)
                {
                    for (int x = 0; x < imageSize.Width; x++)
                    {
                        if (imageData[y * stride + x * 4 + 3] > 0)
                        {
                            imageData[y * stride + x * 4] = (byte)(imageData[y * stride + x * 4] * 255 / imageData[y * stride + x * 4 + 3]);
                            imageData[y * stride + x * 4 + 1] = (byte)(imageData[y * stride + x * 4 + 1] * 255 / imageData[y * stride + x * 4 + 3]);
                            imageData[y * stride + x * 4 + 2] = (byte)(imageData[y * stride + x * 4 + 2] * 255 / imageData[y * stride + x * 4 + 3]);
                        }
                    }
                }
            }
        }
    }
}
