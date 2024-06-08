/*
    MuPDFCore - A set of multiplatform .NET Core bindings for MuPDF.
    Copyright (C) 2024  Giorgio Bianchini

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
using System.Text;

namespace MuPDFCore
{

    /// <summary>
    /// Colour space used by the image.
    /// </summary>
    public enum ColorSpaceType
    {
        /// <summary>
        /// Not specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Grayscale. Each pixel is stored as a single byte.
        /// </summary>
        Gray = 1,

        /// <summary>
        /// RGB colour space. Each pixel is stored as three bytes representing the R, G, and B components of the colour in this order.
        /// </summary>
        RGB = 2,

        /// <summary>
        /// BGR colour space. Each pixel is stored as three bytes representing the B, G, and R components of the colour in this order.
        /// </summary>
        BGR = 3,

        /// <summary>
        /// CMYK colour space. Each pixel is stored as four bytes representing the C, M, Y, and K components of the colour in this order.
        /// </summary>
        CMYK = 4,

        /// <summary>
        /// Lab colour space. Each pixel is stored as three bytes representing the L*, a, and b components of the colour in this order.
        /// </summary>
        Lab = 5,

        /// <summary>
        /// Indexed colour space. Each pixel is stored as a single byte mapping to an entry in the colour space palette.
        /// </summary>
        Indexed = 6,

        /// <summary>
        /// Separation colour space.
        /// </summary>
        Separation = 7
    }

    /// <summary>
    /// Represents a colour space.
    /// </summary>
    public abstract class MuPDFColorSpace
    {
        /// <summary>
        /// The colour space type.
        /// </summary>
        public abstract ColorSpaceType Type { get; }

        /// <summary>
        /// The number of bytes necessary to represent a single pixel in this colour space.
        /// </summary>
        public abstract int NumBytes { get; }

        /// <summary>
        /// The name of the colour space.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        /// Create a new <see cref="MuPDFColorSpace"/> base instance.
        /// </summary>
        /// <param name="name">The name of the colour space.</param>
        protected internal MuPDFColorSpace(string name)
        {
            Name = name;
        }

        /// <summary>
        /// The final colour space in which all pixels will eventually be represented.
        /// </summary>
        public virtual MuPDFColorSpace RootColorSpace
        {
            get
            {
                if (this is IndexedMuPDFColorSpace indexed)
                {
                    return indexed.BaseColorSpace.RootColorSpace;
                }
                else
                {
                    return this;
                }
            }
        }

        internal static unsafe MuPDFColorSpace Create(IntPtr nativeContext, IntPtr nativePointer)
        {
            int csType = 0;
            IntPtr baseColorSpace = IntPtr.Zero;
            IntPtr lookupPointer = IntPtr.Zero;
            int lookupLength = 0;
            int nameLength = 0;

            ExitCodes result = (ExitCodes)NativeMethods.GetColorSpaceData(nativeContext, nativePointer, ref csType, ref nameLength, ref baseColorSpace, ref lookupLength, ref lookupPointer);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_COLORSPACE_METADATA:
                    throw new MuPDFException("An error occurred while retrieving colour space information!", result);
                default:
                    throw new MuPDFException("Unknown error!", result);
            }

            byte[] nameArray = new byte[nameLength];

            fixed (byte* nameArrayPtr = nameArray)
            {
                result = (ExitCodes)NativeMethods.GetColorSpaceName(nativeContext, nativePointer, nameLength, (IntPtr)nameArrayPtr);
            }

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_COLORSPACE_METADATA:
                    throw new MuPDFException("An error occurred while retrieving colour space information!", result);
                default:
                    throw new MuPDFException("Unknown error!", result);
            }

            string name = Encoding.ASCII.GetString(nameArray);

            ColorSpaceType type = (ColorSpaceType)csType;

            switch (type)
            {
                case ColorSpaceType.None:
                    return null;
                case ColorSpaceType.Gray:
                    return new GrayscaleMuPDFColorSpace(name);
                case ColorSpaceType.RGB:
                    return new RGBMuPDFColorSpace(name);
                case ColorSpaceType.BGR:
                    return new BGRMuPDFColorSpace(name);
                case ColorSpaceType.CMYK:
                    return new CMYKMuPDFColorSpace(name);
                case ColorSpaceType.Lab:
                    return new LabMuPDFColorSpace(name);
                case ColorSpaceType.Indexed:
                    {
                        MuPDFColorSpace baseMuPDFColorSpace = Create(nativeContext, baseColorSpace);
                        byte[] lookupTable = new byte[lookupLength * baseMuPDFColorSpace.NumBytes];

                        fixed (byte* lookupTablePtr = lookupTable)
                        {
                            Buffer.MemoryCopy((byte*)lookupPointer, lookupTablePtr, lookupLength * baseMuPDFColorSpace.NumBytes, lookupLength * baseMuPDFColorSpace.NumBytes);
                        }

                        return new IndexedMuPDFColorSpace(lookupTable, baseMuPDFColorSpace, name);
                    }
                case ColorSpaceType.Separation:
                    {
                        string[] colorantNames = new string[lookupLength];

                        for (int i = 0; i < lookupLength; i++)
                        {
                            int colorantNameLength = 0;

                            result = (ExitCodes)NativeMethods.GetColorantNameLength(nativeContext, nativePointer, i, ref colorantNameLength);
                            switch (result)
                            {
                                case ExitCodes.EXIT_SUCCESS:
                                    break;
                                case ExitCodes.ERR_COLORSPACE_METADATA:
                                    throw new MuPDFException("An error occurred while retrieving colorant names!", result);
                                default:
                                    throw new MuPDFException("Unknown error!", result);
                            }

                            byte[] colorantNameArray = new byte[colorantNameLength];

                            fixed (byte* colorantNamePtr = colorantNameArray)
                            {
                                result = (ExitCodes)NativeMethods.GetColorantName(nativeContext, nativePointer, i, colorantNameLength, (IntPtr)colorantNamePtr);
                            }

                            switch (result)
                            {
                                case ExitCodes.EXIT_SUCCESS:
                                    break;
                                case ExitCodes.ERR_COLORSPACE_METADATA:
                                    throw new MuPDFException("An error occurred while retrieving colorant names!", result);
                                default:
                                    throw new MuPDFException("Unknown error!", result);
                            }

                            colorantNames[i] = Encoding.ASCII.GetString(colorantNameArray);
                        }

                        MuPDFColorSpace alternateColorSpace = MuPDFColorSpace.Create(nativeContext, baseColorSpace);

                        return new SeparationColorSpace(name, colorantNames, alternateColorSpace);
                    }
                default:
                    throw new MuPDFException("Unknown colour space type!", ExitCodes.UNKNOWN_ERROR);
            }
        }

        /// <inheritdoc/>
        public override string ToString() => this.Name;
    }

    /// <summary>
    /// Represents a colour space where each pixel is stored as a single byte.
    /// </summary>
    public class GrayscaleMuPDFColorSpace : MuPDFColorSpace
    {
        /// <inheritdoc/>
        public override ColorSpaceType Type => ColorSpaceType.Gray;

        /// <inheritdoc/>
        public override int NumBytes => 1;

        internal GrayscaleMuPDFColorSpace(string name) : base(name)
        {

        }
    }

    /// <summary>
    /// Represents a colour space where each pixel is stored as three bytes encoding the R, G, and B components.
    /// </summary>
    public class RGBMuPDFColorSpace : MuPDFColorSpace
    {
        /// <inheritdoc/>
        public override ColorSpaceType Type => ColorSpaceType.RGB;

        /// <inheritdoc/>
        public override int NumBytes => 3;

        internal RGBMuPDFColorSpace(string name) : base(name)
        {

        }
    }

    /// <summary>
    /// Represents a colour space where each pixel is stored as three bytes encoding the B, G, and R components.
    /// </summary>
    public class BGRMuPDFColorSpace : MuPDFColorSpace
    {
        /// <inheritdoc/>
        public override ColorSpaceType Type => ColorSpaceType.BGR;

        /// <inheritdoc/>
        public override int NumBytes => 3;

        internal BGRMuPDFColorSpace(string name) : base(name)
        {

        }
    }

    /// <summary>
    /// Represents a colour space where each pixel is stored as three bytes encoding the L*, a, and b components.
    /// </summary>
    public class LabMuPDFColorSpace : MuPDFColorSpace
    {
        /// <inheritdoc/>
        public override ColorSpaceType Type => ColorSpaceType.Lab;

        /// <inheritdoc/>
        public override int NumBytes => 3;

        internal LabMuPDFColorSpace(string name) : base(name)
        {

        }
    }

    /// <summary>
    /// Represents a colour space where each pixel is stored as four bytes encoding the C, M, Y, and K components.
    /// </summary>
    public class CMYKMuPDFColorSpace : MuPDFColorSpace
    {
        /// <inheritdoc/>
        public override ColorSpaceType Type => ColorSpaceType.CMYK;

        /// <inheritdoc/>
        public override int NumBytes => 4;

        internal CMYKMuPDFColorSpace(string name) : base(name)
        {

        }
    }

    /// <summary>
    /// Represents a colour space where each pixel is stored as a single byte mapping the pixel to a colour in the lookup table.
    /// </summary>
    public class IndexedMuPDFColorSpace : MuPDFColorSpace
    {
        /// <inheritdoc/>
        public override ColorSpaceType Type => ColorSpaceType.Indexed;

        /// <inheritdoc/>
        public override int NumBytes => 1;

        /// <summary>
        /// The lookup table containing the colour index.
        /// </summary>
        public byte[] LookupTable { get; }

        /// <summary>
        /// The colour space in which the colours in the <see cref="LookupTable"/> are expressed.
        /// </summary>
        public MuPDFColorSpace BaseColorSpace { get; }

        internal IndexedMuPDFColorSpace(byte[] lookupTable, MuPDFColorSpace baseColorSpace, string name) : base(name)
        {
            this.LookupTable = lookupTable;
            this.BaseColorSpace = baseColorSpace;
        }
    }

    /// <summary>
    /// Represents a separation colour space.
    /// </summary>
    public class SeparationColorSpace : MuPDFColorSpace
    {
        /// <inheritdoc/>
        public override ColorSpaceType Type => ColorSpaceType.Separation;

        /// <inheritdoc/>
        public override int NumBytes => 1;

        /// <summary>
        /// Names of the separation colorants.
        /// </summary>
        public string[] ColorantNames { get; }

        /// <summary>
        /// Alternate colour space.
        /// </summary>
        public MuPDFColorSpace AlternateColorSpace { get; }

        internal SeparationColorSpace(string name, string[] colorantNames, MuPDFColorSpace alternateColorSpace) : base(name)
        {
            this.ColorantNames = colorantNames;
            this.AlternateColorSpace = alternateColorSpace;
        }
    }
}
