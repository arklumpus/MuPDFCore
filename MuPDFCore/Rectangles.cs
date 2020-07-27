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
    /// Represents the size of a rectangle.
    /// </summary>
    public struct Size
    {
        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public float Width;

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public float Height;

        /// <summary>
        /// Create a new <see cref="Size"/> with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public Size(float width, float height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Create a new <see cref="Size"/> with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public Size(double width, double height)
        {
            Width = (float)width;
            Height = (float)height;
        }

        /// <summary>
        /// Split the size into the specified number of <see cref="Rectangle"/>s.
        /// </summary>
        /// <param name="divisions">The number of rectangles in which the size should be split. This must be factorisable using only powers of 2, 3, 5 or 7. Otherwise, the biggest number smaller than <paramref name="divisions"/> that satisfies this condition is used.</param>
        /// <returns>An array of <see cref="Rectangle"/>s that when positioned properly cover an area of the size of this object.</returns>
        public Rectangle[] Split(int divisions)
        {
            divisions = Utils.GetAcceptableNumber(divisions);

            Rectangle[] tbr = new Rectangle[divisions];

            bool isVertical = this.Height > this.Width;

            if (divisions == 1)
            {
                tbr[0] = new Rectangle(0, 0, Width, Height);
            }
            else if (divisions == 2)
            {
                if (isVertical)
                {
                    tbr[0] = new Rectangle(0, 0, Width, Height / 2);
                    tbr[1] = new Rectangle(0, Height / 2, Width, Height);
                }
                else
                {
                    tbr[0] = new Rectangle(0, 0, Width / 2, Height);
                    tbr[1] = new Rectangle(Width / 2, 0, Width, Height);
                }
            }
            else if (divisions == 3)
            {
                if (isVertical)
                {
                    tbr[0] = new Rectangle(0, 0, Width / 2, 2 * Height / 3);
                    tbr[1] = new Rectangle(Width / 2, 0, Width, 2 * Height / 3);
                    tbr[2] = new Rectangle(0, 2 * Height / 3, Width, Height);
                }
                else
                {
                    tbr[0] = new Rectangle(0, 0, 2 * Width / 3, Height / 2);
                    tbr[1] = new Rectangle(0, Height / 2, 2 * Width / 3, Height);
                    tbr[2] = new Rectangle(2 * Width / 3, 0, Width, Height);
                }
            }
            else if (divisions == 5)
            {
                if (isVertical)
                {
                    tbr[0] = new Rectangle(0, 0, Width / 2, 2 * Height / 5);
                    tbr[1] = new Rectangle(Width / 2, 0, Width, 2 * Height / 5);
                    tbr[2] = new Rectangle(0, 2 * Height / 5, Width / 2, 4 * Height / 5);
                    tbr[3] = new Rectangle(Width / 2, 2 * Height / 5, Width, 4 * Height / 5);
                    tbr[4] = new Rectangle(0, 4 * Height / 5, Width, Height);
                }
                else
                {
                    tbr[0] = new Rectangle(0, 0, 2 * Width / 5, Height / 2);
                    tbr[1] = new Rectangle(0, Height / 2, 2 * Width / 5, Height);
                    tbr[2] = new Rectangle(2 * Width / 5, 0, 4 * Width / 5, Height / 2);
                    tbr[3] = new Rectangle(2 * Width / 5, Height / 2, 4 * Width / 5, Height);
                    tbr[4] = new Rectangle(4 * Width / 5, 0, Width, Height);
                }
            }
            else if (divisions == 7)
            {
                if (isVertical)
                {
                    tbr[0] = new Rectangle(0, 0, Width / 2, 2 * Height / 7);
                    tbr[1] = new Rectangle(Width / 2, 0, Width, 2 * Height / 7);
                    tbr[2] = new Rectangle(0, 2 * Height / 7, Width / 2, 4 * Height / 7);
                    tbr[3] = new Rectangle(Width / 2, 2 * Height / 7, Width, 4 * Height / 7);
                    tbr[4] = new Rectangle(0, 4 * Height / 7, Width / 2, 6 * Height / 7);
                    tbr[5] = new Rectangle(Width / 2, 4 * Height / 7, Width, 6 * Height / 7);
                    tbr[6] = new Rectangle(0, 6 * Height / 7, Width, Height);
                }
                else
                {
                    tbr[0] = new Rectangle(0, 0, 2 * Width / 7, Height / 2);
                    tbr[1] = new Rectangle(0, Height / 2, 2 * Width / 7, Height);
                    tbr[2] = new Rectangle(2 * Width / 7, 0, 4 * Width / 7, Height / 2);
                    tbr[3] = new Rectangle(2 * Width / 7, Height / 2, 4 * Width / 7, Height);
                    tbr[4] = new Rectangle(4 * Width / 7, 0, 6 * Width / 7, Height / 2);
                    tbr[5] = new Rectangle(4 * Width / 7, Height / 2, 6 * Width / 7, Height);
                    tbr[6] = new Rectangle(6 * Width / 7, 0, Width, Height);
                }
            }
            else
            {
                for (int divisorInd = 0; divisorInd < Utils.AcceptableDivisors.Length; divisorInd++)
                {
                    if (divisions % Utils.AcceptableDivisors[divisorInd] == 0)
                    {
                        Rectangle[] largerDivisions = this.Split(divisions / Utils.AcceptableDivisors[divisorInd]);

                        int pos = 0;

                        for (int i = 0; i < largerDivisions.Length; i++)
                        {
                            Size s = new Size(largerDivisions[i].Width, largerDivisions[i].Height);
                            Rectangle[] currDivision = s.Split(Utils.AcceptableDivisors[divisorInd]);

                            for (int j = 0; j < currDivision.Length; j++)
                            {
                                tbr[pos] = new Rectangle(largerDivisions[i].X0 + currDivision[j].X0, largerDivisions[i].Y0 + currDivision[j].Y0, largerDivisions[i].X0 + currDivision[j].X1, largerDivisions[i].Y0 + currDivision[j].Y1);
                                pos++;
                            }
                        }

                        break;
                    }
                }
            }

            return tbr;

        }
    }

    /// <summary>
    /// Represents the size of a rectangle using only integer numbers.
    /// </summary>
    public struct RoundedSize
    {
        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public int Height;

        /// <summary>
        /// Create a new <see cref="RoundedSize"/> with the specified width and height.
        /// </summary>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public RoundedSize(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Split the size into the specified number of <see cref="RoundedRectangle"/>s.
        /// </summary>
        /// <param name="divisions">The number of rectangles in which the size should be split. This must be factorisable using only powers of 2, 3, 5 or 7. Otherwise, the biggest number smaller than <paramref name="divisions"/> that satisfies this condition is used.</param>
        /// <returns>An array of <see cref="RoundedRectangle"/>s that when positioned properly cover an area of the size of this object.</returns>
        public RoundedRectangle[] Split(int divisions)
        {
            divisions = Utils.GetAcceptableNumber(divisions);

            RoundedRectangle[] tbr = new RoundedRectangle[divisions];

            bool isVertical = this.Height > this.Width;

            if (divisions == 1)
            {
                tbr[0] = new RoundedRectangle(0, 0, Width, Height);
            }
            else if (divisions == 2)
            {
                if (isVertical)
                {
                    tbr[0] = new RoundedRectangle(0, 0, Width, Height / 2);
                    tbr[1] = new RoundedRectangle(0, Height / 2, Width, Height);
                }
                else
                {
                    tbr[0] = new RoundedRectangle(0, 0, Width / 2, Height);
                    tbr[1] = new RoundedRectangle(Width / 2, 0, Width, Height);
                }
            }
            else if (divisions == 3)
            {
                if (isVertical)
                {
                    tbr[0] = new RoundedRectangle(0, 0, Width / 2, 2 * Height / 3);
                    tbr[1] = new RoundedRectangle(Width / 2, 0, Width, 2 * Height / 3);
                    tbr[2] = new RoundedRectangle(0, 2 * Height / 3, Width, Height);
                }
                else
                {
                    tbr[0] = new RoundedRectangle(0, 0, 2 * Width / 3, Height / 2);
                    tbr[1] = new RoundedRectangle(0, Height / 2, 2 * Width / 3, Height);
                    tbr[2] = new RoundedRectangle(2 * Width / 3, 0, Width, Height);
                }
            }
            else if (divisions == 5)
            {
                if (isVertical)
                {
                    tbr[0] = new RoundedRectangle(0, 0, Width / 2, 2 * Height / 5);
                    tbr[1] = new RoundedRectangle(Width / 2, 0, Width, 2 * Height / 5);
                    tbr[2] = new RoundedRectangle(0, 2 * Height / 5, Width / 2, 4 * Height / 5);
                    tbr[3] = new RoundedRectangle(Width / 2, 2 * Height / 5, Width, 4 * Height / 5);
                    tbr[4] = new RoundedRectangle(0, 4 * Height / 5, Width, Height);
                }
                else
                {
                    tbr[0] = new RoundedRectangle(0, 0, 2 * Width / 5, Height / 2);
                    tbr[1] = new RoundedRectangle(0, Height / 2, 2 * Width / 5, Height);
                    tbr[2] = new RoundedRectangle(2 * Width / 5, 0, 4 * Width / 5, Height / 2);
                    tbr[3] = new RoundedRectangle(2 * Width / 5, Height / 2, 4 * Width / 5, Height);
                    tbr[4] = new RoundedRectangle(4 * Width / 5, 0, Width, Height);
                }
            }
            else if (divisions == 7)
            {
                if (isVertical)
                {
                    tbr[0] = new RoundedRectangle(0, 0, Width / 2, 2 * Height / 7);
                    tbr[1] = new RoundedRectangle(Width / 2, 0, Width, 2 * Height / 7);
                    tbr[2] = new RoundedRectangle(0, 2 * Height / 7, Width / 2, 4 * Height / 7);
                    tbr[3] = new RoundedRectangle(Width / 2, 2 * Height / 7, Width, 4 * Height / 7);
                    tbr[4] = new RoundedRectangle(0, 4 * Height / 7, Width / 2, 6 * Height / 7);
                    tbr[5] = new RoundedRectangle(Width / 2, 4 * Height / 7, Width, 6 * Height / 7);
                    tbr[6] = new RoundedRectangle(0, 6 * Height / 7, Width, Height);
                }
                else
                {
                    tbr[0] = new RoundedRectangle(0, 0, 2 * Width / 7, Height / 2);
                    tbr[1] = new RoundedRectangle(0, Height / 2, 2 * Width / 7, Height);
                    tbr[2] = new RoundedRectangle(2 * Width / 7, 0, 4 * Width / 7, Height / 2);
                    tbr[3] = new RoundedRectangle(2 * Width / 7, Height / 2, 4 * Width / 7, Height);
                    tbr[4] = new RoundedRectangle(4 * Width / 7, 0, 6 * Width / 7, Height / 2);
                    tbr[5] = new RoundedRectangle(4 * Width / 7, Height / 2, 6 * Width / 7, Height);
                    tbr[6] = new RoundedRectangle(6 * Width / 7, 0, Width, Height);
                }
            }
            else
            {
                for (int divisorInd = 0; divisorInd < Utils.AcceptableDivisors.Length; divisorInd++)
                {
                    if (divisions % Utils.AcceptableDivisors[divisorInd] == 0)
                    {
                        RoundedRectangle[] largerDivisions = this.Split(divisions / Utils.AcceptableDivisors[divisorInd]);

                        int pos = 0;

                        for (int i = 0; i < largerDivisions.Length; i++)
                        {
                            RoundedSize s = new RoundedSize(largerDivisions[i].Width, largerDivisions[i].Height);
                            RoundedRectangle[] currDivision = s.Split(Utils.AcceptableDivisors[divisorInd]);

                            for (int j = 0; j < currDivision.Length; j++)
                            {
                                tbr[pos] = new RoundedRectangle(largerDivisions[i].X0 + currDivision[j].X0, largerDivisions[i].Y0 + currDivision[j].Y0, largerDivisions[i].X0 + currDivision[j].X1, largerDivisions[i].Y0 + currDivision[j].Y1);
                                pos++;
                            }
                        }

                        break;
                    }
                }
            }

            return tbr;

        }
    }

    /// <summary>
    /// Represents a rectangle.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// The left coordinate of the rectangle.
        /// </summary>
        public float X0;

        /// <summary>
        /// The top coordinate of the rectangle.
        /// </summary>
        public float Y0;

        /// <summary>
        /// The right coordinate of the rectangle.
        /// </summary>
        public float X1;

        /// <summary>
        /// The bottom coordinate of the rectangle.
        /// </summary>
        public float Y1;

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public float Width => X1 - X0;

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public float Height => Y1 - Y0;

        /// <summary>
        /// Create a new <see cref="Rectangle"/> from the specified coordinates.
        /// </summary>
        /// <param name="x0">The left coordinate of the rectangle.</param>
        /// <param name="y0">The top coordinate of the rectangle.</param>
        /// <param name="x1">The right coordinate of the rectangle.</param>
        /// <param name="y1">The bottom coordinate of the rectangle.</param>
        public Rectangle(float x0, float y0, float x1, float y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        /// <summary>
        /// Create a new <see cref="Rectangle"/> from the specified coordinates.
        /// </summary>
        /// <param name="x0">The left coordinate of the rectangle.</param>
        /// <param name="y0">The top coordinate of the rectangle.</param>
        /// <param name="x1">The right coordinate of the rectangle.</param>
        /// <param name="y1">The bottom coordinate of the rectangle.</param>
        public Rectangle(double x0, double y0, double x1, double y1)
        {
            X0 = (float)x0;
            Y0 = (float)y0;
            X1 = (float)x1;
            Y1 = (float)y1;
        }

        /// <summary>
        /// Round the rectangle's coordinates to the closest integers.
        /// </summary>
        /// <returns>A <see cref="RoundedRectangle"/> with the rounded coordinates.</returns>
        public RoundedRectangle Round()
        {
            return new RoundedRectangle(
               (int)Math.Floor(X0 + 0.001),
               (int)Math.Floor(Y0 + 0.001),
               (int)Math.Ceiling(X1 - 0.001),
               (int)Math.Ceiling(Y1 - 0.001)
           );
        }

        /// <summary>
        /// Round the rectangle's coordinates to the closest integers, applying the specified zoom factor.
        /// </summary>
        /// <param name="zoom">The zoom factor to apply.</param>
        /// <returns>A <see cref="RoundedRectangle"/> with the rounded coordinates.</returns>
        public RoundedRectangle Round(double zoom)
        {
            return new RoundedRectangle(
               (int)Math.Floor(X0 * (float)zoom + 0.001),
               (int)Math.Floor(Y0 * (float)zoom + 0.001),
               (int)Math.Ceiling(X1 * (float)zoom - 0.001),
               (int)Math.Ceiling(Y1 * (float)zoom - 0.001)
           );
        }

        /// <summary>
        /// Split the rectangle into the specified number of <see cref="Rectangle"/>s.
        /// </summary>
        /// <param name="divisions">The number of rectangles in which the rectangle should be split. This must be factorisable using only powers of 2, 3, 5 or 7. Otherwise, the biggest number smaller than <paramref name="divisions"/> that satisfies this condition is used.</param>
        /// <returns>An array of <see cref="Rectangle"/>s that when positioned properly cover the same area as this object.</returns>
        public Rectangle[] Split(int divisions)
        {
            Size s = new Size(this.Width, this.Height);

            Rectangle[] splitSize = s.Split(divisions);

            Rectangle[] tbr = new Rectangle[divisions];

            for (int i = 0; i < splitSize.Length; i++)
            {
                tbr[i] = new Rectangle(this.X0 + splitSize[i].X0, this.Y0 + splitSize[i].Y0, this.X0 + splitSize[i].X1, this.Y0 + splitSize[i].Y1);
            }

            return tbr;
        }

        /// <summary>
        /// Compute the intersection between this <see cref="Rectangle"/> and another one.
        /// </summary>
        /// <param name="other">The other <see cref="Rectangle"/> to intersect with this instance.</param>
        /// <returns>The intersection between the two <see cref="Rectangle"/>s.</returns>
        public Rectangle Intersect(Rectangle other)
        {
            float x0 = Math.Max(this.X0, other.X0);
            float y0 = Math.Max(this.Y0, other.Y0);

            float x1 = Math.Min(this.X1, other.X1);
            float y1 = Math.Min(this.Y1, other.Y1);

            if (x1 <= x0 || y1 <= y0)
            {
                return new Rectangle(0, 0, 0, 0);
            }
            else
            {
                return new Rectangle(x0, y0, x1, y1);
            }
        }

        /// <summary>
        /// Checks whether this <see cref="Rectangle"/> contains another <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="other">The <see cref="Rectangle"/> to check.</param>
        /// <returns>A boolean value indicating whether this <see cref="Rectangle"/> contains the <paramref name="other"/> <see cref="Rectangle"/>.</returns>
        public bool Contains(Rectangle other)
        {
            return other.X0 >= this.X0 && other.X1 <= this.X1 && other.Y0 >= this.Y0 && other.Y1 <= this.Y1;
        }
    }

    /// <summary>
    /// Represents a rectangle using only integer numbers.
    /// </summary>
    public struct RoundedRectangle
    {
        /// <summary>
        /// The left coordinate of the rectangle.
        /// </summary>
        public int X0;

        /// <summary>
        /// The top coordinate of the rectangle.
        /// </summary>
        public int Y0;

        /// <summary>
        /// The right coordinate of the rectangle.
        /// </summary>
        public int X1;

        /// <summary>
        /// The bottom coordinate of the rectangle.
        /// </summary>
        public int Y1;

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public int Width => X1 - X0;

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public int Height => Y1 - Y0;

        /// <summary>
        /// Create a new <see cref="RoundedRectangle"/> from the specified coordinates.
        /// </summary>
        /// <param name="x0">The left coordinate of the rectangle.</param>
        /// <param name="y0">The top coordinate of the rectangle.</param>
        /// <param name="x1">The right coordinate of the rectangle.</param>
        /// <param name="y1">The bottom coordinate of the rectangle.</param>
        public RoundedRectangle(int x0, int y0, int x1, int y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        /// <summary>
        /// Split the rectangle into the specified number of <see cref="RoundedRectangle"/>s.
        /// </summary>
        /// <param name="divisions">The number of rectangles in which the rectangle should be split. This must be factorisable using only powers of 2, 3, 5 or 7. Otherwise, the biggest number smaller than <paramref name="divisions"/> that satisfies this condition is used.</param>
        /// <returns>An array of <see cref="RoundedRectangle"/>s that when positioned properly cover the same area as this object.</returns>
        public RoundedRectangle[] Split(int divisions)
        {
            RoundedSize s = new RoundedSize(this.Width, this.Height);

            RoundedRectangle[] splitSize = s.Split(divisions);

            RoundedRectangle[] tbr = new RoundedRectangle[divisions];

            for (int i = 0; i < splitSize.Length; i++)
            {
                tbr[i] = new RoundedRectangle(this.X0 + splitSize[i].X0, this.Y0 + splitSize[i].Y0, this.X0 + splitSize[i].X1, this.Y0 + splitSize[i].Y1);
            }

            return tbr;
        }
    }

}
