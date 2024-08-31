using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MuPDFCore
{
    /// <summary>
    /// Options for creating a PDF document.
    /// </summary>
    public class PDFCreationOptions
    {
        /// <summary>
        /// Stream compression options for PDF documents.
        /// </summary>
        public enum CompressionOptions
        {
            /// <summary>
            /// Use the default settings for each stream.
            /// </summary>
            Preserve = 0,

            /// <summary>
            /// Decompress all streams (except if <see cref="CompressFonts"/> or <see cref="CompressImages"/> are also specified; cannot be specified at the same time as <see cref="Compress"/>).
            /// </summary>
            Decompress = 0b1,

            /// <summary>
            /// Compress all streams (cannot be specified at the same time as <see cref="Decompress"/>).
            /// </summary>
            Compress = 0b10,

            /// <summary>
            /// Compress embedded fonts.
            /// </summary>
            CompressFonts = 0b100,

            /// <summary>
            /// Compress images.
            /// </summary>
            CompressImages = 0b1000
        }

        /// <summary>
        /// Options for garbage collection.
        /// </summary>
        public enum GarbageCollectionOption
        {
            /// <summary>
            /// Do not perform any garbage collection.
            /// </summary>
            None,

            /// <summary>
            /// Garbage collect unused objects.
            /// </summary>
            Collect,

            /// <summary>
            /// Garbage collect unused objects and compact XRef table.
            /// </summary>
            CollectCompact,

            /// <summary>
            /// Garbage collect unused objects, compact XRef table, and remove duplicate objects.
            /// </summary>
            CollectCompactDeduplicate
        }

        /// <summary>
        /// Document encryption options.
        /// </summary>
        public enum Encryption
        {
            /// <summary>
            /// Create an unencrypted document.
            /// </summary>
            None,

            /// <summary>
            /// RC4 cipher with 40-bit key (a <see cref="UserPassword"/> or <see cref="OwnerPassword"/> must be supplied).
            /// </summary>
            RC4_40,

            /// <summary>
            /// RC4 cipher with 128-bit key (a <see cref="UserPassword"/> or <see cref="OwnerPassword"/> must be supplied).
            /// </summary>
            RC4_128,

            /// <summary>
            /// AES cipher with 128-bit key (a <see cref="UserPassword"/> or <see cref="OwnerPassword"/> must be supplied).
            /// </summary>
            AES_128,

            /// <summary>
            /// AES cipher with 256-bit key (a <see cref="UserPassword"/> or <see cref="OwnerPassword"/> must be supplied).
            /// </summary>
            AES_256
        }

        /// <summary>
        /// Specifies which actions can be performed without the owner password.
        /// </summary>
        public class DocumentPermissions
        {
            /// <summary>
            /// Quality of allowed printing operations.
            /// </summary>
            public enum PrintingPermission
            {
                /// <summary>
                /// The document cannot be printed without the owner password.
                /// </summary>
                None,

                /// <summary>
                /// Allow printing, but only in a low-level representation, possibly of degraded quality.
                /// </summary>
                LowFidelity,

                /// <summary>
                /// Allow printing to a representation from which a faithful digital copy of the document can be generated.
                /// </summary>
                HighFidelity
            }

            /// <summary>
            /// Allowed text and graphics copy/extraction operations.
            /// </summary>
            public enum ExtractionPermission
            {
                /// <summary>
                /// Text and graphics cannot be extracted or copied from the document without the owner password.
                /// </summary>
                None,

                /// <summary>
                /// Text and graphics can be extracted from the document (for accesssibility or other purposes).
                /// </summary>
                ExtractTextAndGraphics,

                /// <summary>
                /// Text and graphics can be extracted and copied from the document.
                /// </summary>
                CopyAndExtractTextAndGraphics
            }

            /// <summary>
            /// Allowed form and annotation operations.
            /// </summary>
            public enum FormPermission
            {
                /// <summary>
                /// Forms cannot be filled in without the owner password, and annotations cannot be added.
                /// </summary>
                None,

                /// <summary>
                /// Forms can be filled in without the owner password, but annotations cannot be added.
                /// </summary>
                Forms,

                /// <summary>
                /// Forms can be filled in without the owner password, and annotations can be added.
                /// </summary>
                FormsAnnotations
            }

            /// <summary>
            /// Specifies if printing is allowed without the owner password.
            /// </summary>
            public PrintingPermission Printing { get; set; } = PrintingPermission.HighFidelity;

            /// <summary>
            /// Specifies whether text can be extracted without the owner password.
            /// </summary>
            public ExtractionPermission Extraction { get; set; } = ExtractionPermission.CopyAndExtractTextAndGraphics;

            /// <summary>
            /// Specifies whether the document can be restructured without the owner password (e.g., by inserting, rotating or deleting pages, bookmarks and thumbnail images).
            /// </summary>
            public bool AllowDocumentRestructuring { get; set; }

            /// <summary>
            /// Specifies whether forms can be filled-in and annotations can be added to the document without the owner password.
            /// </summary>
            public FormPermission Annotations { get; set; }

            /// <summary>
            /// Specifies whether the document can be modified without the owner password.
            /// </summary>
            public bool AllowDocumentModification { get; set; }

            internal int GetPermissionNumber()
            {
                uint tbr = 0b11111111111111111111000011000000;

                switch (Printing)
                {
                    case PrintingPermission.None:
                        break;
                    case PrintingPermission.LowFidelity:
                        tbr |= 0b000000000100;
                        break;
                    case PrintingPermission.HighFidelity:
                        tbr |= 0b100000000100;
                        break;
                }

                switch (Extraction)
                {
                    case ExtractionPermission.None:
                        break;
                    case ExtractionPermission.ExtractTextAndGraphics:
                        tbr |= 0b001000000000;
                        break;
                    case ExtractionPermission.CopyAndExtractTextAndGraphics:
                        tbr |= 0b001000010000;
                        break;
                }

                if (AllowDocumentRestructuring)
                {
                    tbr |= 0b010000000000;
                }

                switch (Annotations)
                {
                    case FormPermission.None:
                        break;
                    case FormPermission.Forms:
                        tbr |= 0b000100000000;
                        break;
                    case FormPermission.FormsAnnotations:
                        tbr |= 0b000100100000;
                        break;
                }

                if (AllowDocumentModification)
                {
                    tbr |= 0b000000001000;
                }

                return (int)tbr;
            }
        }

        /// <summary>
        /// If this is <see langword="true" />, annotations (e.g. signatures) are included in the converted document. Otherwise, only the page contents are included.
        /// </summary>
        public bool IncludeAnnotations { get; set; } = true;

        private CompressionOptions compressStreams = CompressionOptions.Preserve;

        /// <summary>
        /// Determines whether streams are compressed in the generated PDF document.
        /// </summary>
        public CompressionOptions CompressStreams
        {
            get => compressStreams;
            set
            {
                if (value.HasFlag(CompressionOptions.Compress) && value.HasFlag(CompressionOptions.Decompress))
                {
                    throw new ArgumentException("CompressionOptions.Compress and CompressionOptions.Decompress may not be specified at the same time!", "value");
                }

                this.compressStreams = value;
            }
        }

        /// <summary>
        /// Use ASCII hex encoding for binary streams.
        /// </summary>
        public bool AsciiEncode { get; set; } = false;

        /// <summary>
        /// Pretty-print objects with indentation.
        /// </summary>
        public bool PrettyPrint { get; set; } = false;

        /// <summary>
        /// Generate a linearized PDF, optimized for loading in web browsers.
        /// </summary>
        public bool Linearize { get; set; } = false;

        /// <summary>
        /// Pretty-print graphics commands in content streams.
        /// </summary>
        public bool Clean { get; set; } = false;

        /// <summary>
        /// Sanitize graphics commands in content streams.
        /// </summary>
        public bool Sanitize { get; set; } = false;

        /// <summary>
        /// Garbage collection options.
        /// </summary>
        public GarbageCollectionOption Garbage { get; set; } = GarbageCollectionOption.None;

        /// <summary>
        /// Type of encryption to use.
        /// </summary>
        public Encryption EncryptionType { get; set; } = Encryption.None;

        /// <summary>
        /// User password required to read the newly created document.
        /// </summary>
        public string UserPassword { get; set; } = null;

        /// <summary>
        /// Owner password required to edit the newly created document.
        /// </summary>
        public string OwnerPassword { get; set; } = null;

        /// <summary>
        /// Actions that can be performed without the owner password (an <see cref="OwnerPassword"/> must be supplied, and an <see cref="EncryptionType"/> must be specified.
        /// </summary>
        public DocumentPermissions Permissions { get; set; } = null;

        /// <summary>
        /// Regenerate document id.
        /// </summary>
        public bool RegenerateId { get; set; } = true;

        internal string GetOptionString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.CompressStreams.HasFlag(CompressionOptions.Compress))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("compress=yes");
            }
            else if (this.CompressStreams.HasFlag(CompressionOptions.Decompress))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("decompress=yes");
            }

            if (this.CompressStreams.HasFlag(CompressionOptions.CompressFonts))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("compress-fonts=yes");
            }

            if (this.CompressStreams.HasFlag(CompressionOptions.CompressImages))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("compress-images=yes");
            }

            if (this.AsciiEncode)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("ascii=yes");
            }

            if (this.PrettyPrint)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("pretty=yes");
            }

            if (this.Linearize)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("linearize=yes");
            }

            if (this.Clean)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("clean=yes");
            }

            if (this.Sanitize)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("sanitize=yes");
            }

            switch (this.Garbage)
            {
                case GarbageCollectionOption.None:
                    break;
                case GarbageCollectionOption.Collect:
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append("garbage=yes");
                    break;
                case GarbageCollectionOption.CollectCompact:
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append("garbage=compact");
                    break;
                case GarbageCollectionOption.CollectCompactDeduplicate:
                    if (sb.Length > 0)
                    {
                        sb.Append(",");
                    }
                    sb.Append("garbage=deduplicate");
                    break;
            }

            if (this.EncryptionType == Encryption.None)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("decrypt=yes");

                if (Permissions != null)
                {
                    throw new InvalidOperationException("Encryption must be enabled and an owner password must be supplied in order to use permissions.");
                }

                if (!string.IsNullOrEmpty(OwnerPassword))
                {
                    throw new InvalidOperationException("Encryption must be enabled and a set of permissions must be supplied in order to use an owner password.");
                }

                if (!string.IsNullOrEmpty(UserPassword))
                {
                    throw new InvalidOperationException("Encryption must be enabled in order to use a user password.");
                }
            }
            else
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                switch (this.EncryptionType)
                {
                    case Encryption.RC4_40:
                        sb.Append("encrypt=rc4-40");
                        break;

                    case Encryption.RC4_128:
                        sb.Append("encrypt=rc4-128");
                        break;

                    case Encryption.AES_128:
                        sb.Append("encrypt=aes-128");
                        break;

                    case Encryption.AES_256:
                        sb.Append("encrypt=aes-256");
                        break;
                }

                if (string.IsNullOrEmpty(UserPassword) && string.IsNullOrEmpty(OwnerPassword))
                {
                    throw new InvalidOperationException("A user password or an owner password must be supplied in order to use encryption.");
                }
            }

            if (!string.IsNullOrEmpty(UserPassword))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("user-password=");
                sb.Append(UserPassword);
            }

            if (!string.IsNullOrEmpty(OwnerPassword))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("owner-password=");
                sb.Append(OwnerPassword);
            }

            if (Permissions != null)
            {
                if (string.IsNullOrEmpty(OwnerPassword))
                {
                    throw new InvalidOperationException("Encryption must be enabled and an owner password must be supplied in order to use permissions.");
                }

                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("permissions=");
                sb.Append(this.Permissions.GetPermissionNumber().ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (!this.RegenerateId)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("regenerate-id=no");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for creating an SVG document.
    /// </summary>
    public class SVGCreationOptions
    {
        /// <summary>
        /// Ways in which text is represented in the SVG document.
        /// </summary>
        public enum TextOption
        {
            /// <summary>
            /// Emit text as &lt;text&gt; elements (inaccurate fonts).
            /// </summary>
            TextAsText,

            /// <summary>
            /// Emit text as &lt;path&gt; elements (accurate fonts, but text is not searchable or selectable).
            /// </summary>
            TextAsPath
        }

        /// <summary>
        /// If this is <see langword="true" />, annotations (e.g. signatures) are included in the converted document. Otherwise, only the page contents are included.
        /// </summary>
        public bool IncludeAnnotations { get; set; } = true;

        /// <summary>
        /// Determines how text is represented in the SVG document.
        /// </summary>
        public TextOption TextRendering { get; set; } = TextOption.TextAsPath;

        /// <summary>
        /// Whether to reuse images using &lt;symbol&gt; definitions.
        /// </summary>
        public bool ReuseImages { get; set; } = true;

        internal string GetOptionString()
        {
            StringBuilder sb = new StringBuilder();

            if (sb.Length > 0)
            {
                sb.Append(",");
            }
            switch (this.TextRendering)
            {
                case TextOption.TextAsPath:
                    sb.Append("text=path");
                    break;
                case TextOption.TextAsText:
                    sb.Append("text=text");
                    break;
            }

            if (!ReuseImages)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("no-reuse-images=yes");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for creating a CBZ document.
    /// </summary>
    public class CBZCreationOptions
    {
        /// <summary>
        /// If this is <see langword="true" />, annotations (e.g. signatures) are included in the converted document. Otherwise, only the page contents are included.
        /// </summary>
        public bool IncludeAnnotations { get; set; } = true;

        /// <summary>
        /// Colour spaces for the rendered images.
        /// </summary>
        public enum ColorSpace
        {
            /// <summary>
            /// Grayscale
            /// </summary>
            Gray,

            /// <summary>
            /// RGB colour space.
            /// </summary>
            RGB,

            /// <summary>
            /// CMYK colour space.
            /// </summary>
            CMYK
        }

        /// <summary>
        /// Rasterizer settings.
        /// </summary>
        public enum RasterizerOption
        {
            /// <summary>
            /// Default rasterizer.
            /// </summary>
            Default,

            /// <summary>
            /// Anti-aliasing with 0 bits.
            /// </summary>
            AntiAliasing_0,

            /// <summary>
            /// Anti-aliasing with 1 bit.
            /// </summary>
            AntiAliasing_1,

            /// <summary>
            /// Anti-aliasing with 2 bits.
            /// </summary>
            AntiAliasing_2,

            /// <summary>
            /// Anti-aliasing with 3 bits.
            /// </summary>
            AntiAliasing_3,

            /// <summary>
            /// Anti-aliasing with 4 bits.
            /// </summary>
            AntiAliasing_4,

            /// <summary>
            /// Anti-aliasing with 5 bits.
            /// </summary>
            AntiAliasing_5,

            /// <summary>
            /// Anti-aliasing with 6 bits.
            /// </summary>
            AntiAliasing_6,

            /// <summary>
            /// Anti-aliasing with 7 bits.
            /// </summary>
            AntiAliasing_7,

            /// <summary>
            /// Anti-aliasing with 8 bits.
            /// </summary>
            AntiAliasing_8,

            /// <summary>
            /// Centre of pixel.
            /// </summary>
            CenterOfPixel,

            /// <summary>
            /// Any part of the pixel.
            /// </summary>
            AnyPartOfPixel
        }


        /// <summary>
        /// Angle (in degrees) by which the rendered pages will be rotated.
        /// </summary>
        public double Rotate { get; set; } = double.NaN;

        /// <summary>
        /// X resolution of the rendered pages in pixels per inch.
        /// </summary>
        public double XResolution { get; set; } = double.NaN;

        /// <summary>
        /// Y resolution of the rendered pages in pixels per inch.
        /// </summary>
        public double YResolution { get; set; } = double.NaN;

        /// <summary>
        /// Render pages to fit the specified width.
        /// </summary>
        public double Width { get; set; } = double.NaN;

        /// <summary>
        /// Render pages to fit the specified height.
        /// </summary>
        public double Height { get; set; } = double.NaN;

        /// <summary>
        /// Colour space for rendering.
        /// </summary>
        public ColorSpace RenderingColorSpace { get; set; } = ColorSpace.RGB;

        /// <summary>
        /// Render pages with an alpha channel and transparent background.
        /// </summary>
        public bool Alpha { get; set; } = false;

        /// <summary>
        /// Rasterizer settings for graphics elements.
        /// </summary>
        public RasterizerOption GraphicsRasterizer { get; set; } = RasterizerOption.Default;

        /// <summary>
        /// Rasterizer settings for text elements.
        /// </summary>
        public RasterizerOption TextRasterizer { get; set; } = RasterizerOption.Default;

        internal string GetOptionString()
        {
            StringBuilder sb = new StringBuilder();

            if (!double.IsNaN(Rotate))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("rotate=" + Rotate.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (!double.IsNaN(XResolution))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("x-resolution=" + XResolution.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (!double.IsNaN(YResolution))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("y-resolution=" + YResolution.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (!double.IsNaN(Width))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("width=" + Width.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (!double.IsNaN(Height))
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("height=" + Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            if (sb.Length > 0)
            {
                sb.Append(",");
            }

            switch (this.RenderingColorSpace)
            {
                case ColorSpace.Gray:
                    sb.Append("colorspace=gray");
                    break;
                case ColorSpace.RGB:
                    sb.Append("colorspace=rgb");
                    break;
                case ColorSpace.CMYK:
                    sb.Append("colorspace=cmyk");
                    break;
            }

            if (Alpha)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("alpha=yes");
            }
            else
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("alpha=no");
            }

            if (GraphicsRasterizer != RasterizerOption.Default)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                switch (GraphicsRasterizer)
                {
                    case RasterizerOption.AntiAliasing_0:
                        sb.Append("graphics=aa0");
                        break;
                    case RasterizerOption.AntiAliasing_1:
                        sb.Append("graphics=aa1");
                        break;
                    case RasterizerOption.AntiAliasing_2:
                        sb.Append("graphics=aa2");
                        break;
                    case RasterizerOption.AntiAliasing_3:
                        sb.Append("graphics=aa3");
                        break;
                    case RasterizerOption.AntiAliasing_4:
                        sb.Append("graphics=aa4");
                        break;
                    case RasterizerOption.AntiAliasing_5:
                        sb.Append("graphics=aa5");
                        break;
                    case RasterizerOption.AntiAliasing_6:
                        sb.Append("graphics=aa6");
                        break;
                    case RasterizerOption.AntiAliasing_7:
                        sb.Append("graphics=aa7");
                        break;
                    case RasterizerOption.AntiAliasing_8:
                        sb.Append("graphics=aa8");
                        break;
                    case RasterizerOption.CenterOfPixel:
                        sb.Append("graphics=cop");
                        break;
                    case RasterizerOption.AnyPartOfPixel:
                        sb.Append("graphics=app");
                        break;
                }
            }

            if (TextRasterizer != RasterizerOption.Default)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }

                switch (TextRasterizer)
                {
                    case RasterizerOption.AntiAliasing_0:
                        sb.Append("text=aa0");
                        break;
                    case RasterizerOption.AntiAliasing_1:
                        sb.Append("text=aa1");
                        break;
                    case RasterizerOption.AntiAliasing_2:
                        sb.Append("text=aa2");
                        break;
                    case RasterizerOption.AntiAliasing_3:
                        sb.Append("text=aa3");
                        break;
                    case RasterizerOption.AntiAliasing_4:
                        sb.Append("text=aa4");
                        break;
                    case RasterizerOption.AntiAliasing_5:
                        sb.Append("text=aa5");
                        break;
                    case RasterizerOption.AntiAliasing_6:
                        sb.Append("text=aa6");
                        break;
                    case RasterizerOption.AntiAliasing_7:
                        sb.Append("text=aa7");
                        break;
                    case RasterizerOption.AntiAliasing_8:
                        sb.Append("text=aa8");
                        break;
                    case RasterizerOption.CenterOfPixel:
                        sb.Append("text=cop");
                        break;
                    case RasterizerOption.AnyPartOfPixel:
                        sb.Append("text=app");
                        break;
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for creating a text or structured text document.
    /// </summary>
    public class TXTCreationOptions
    {
        /// <summary>
        /// If this is <see langword="true" />, annotations (e.g. signatures) are included in the converted document. Otherwise, only the page contents are included.
        /// </summary>
        public bool IncludeAnnotations { get; set; } = true;

        /// <summary>
        /// Do not add spaces between gaps in the text.
        /// </summary>
        public bool InhibitSpaces { get; set; } = false;

        /// <summary>
        /// Do not expand ligatures into constituent characters.
        /// </summary>
        public bool PreserveLigatures { get; set; } = false;

        /// <summary>
        /// Do not convert all whitespace into space characters.
        /// </summary>
        public bool PreserveWhitespace { get; set; } = false;

        /// <summary>
        /// Do not merge spans on the same line.
        /// </summary>
        public bool PreserveSpans { get; set; } = false;

        /// <summary>
        /// Attempt to join up hyphenated words.
        /// </summary>
        public bool Dehyphenate { get; set; } = false;

        /// <summary>
        /// Guess unicode from CID if normal mapping fails.
        /// </summary>
        public bool UseCIDForUnknownUnicode { get; set; } = false;

        /// <summary>
        /// Include characters that are outside of the page's mediabox.
        /// </summary>
        public bool IncludeCharactersOutsideMediaBox { get; set; } = false;

        internal string GetOptionString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.InhibitSpaces)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("inhibit-spaces=yes");
            }

            if (this.PreserveLigatures)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("preserve-ligatures=yes");
            }

            if (this.PreserveWhitespace)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("preserve-whitespace=yes");
            }

            if (this.PreserveSpans)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("preserve-spans=yes");
            }

            if (this.Dehyphenate)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("dehyphenate=yes");
            }

            if (this.UseCIDForUnknownUnicode)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("use-cid-for-unknown-unicode=yes");
            }

            if (this.IncludeCharactersOutsideMediaBox)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("mediabox-clip=no");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Options for creating an HTML or XHTML document.
    /// </summary>
    public class HTMLCreationOptions
    {
        /// <summary>
        /// If this is <see langword="true" />, annotations (e.g. signatures) are included in the converted document. Otherwise, only the page contents are included.
        /// </summary>
        public bool IncludeAnnotations { get; set; } = true;

        /// <summary>
        /// Do not add spaces between gaps in the text.
        /// </summary>
        public bool InhibitSpaces { get; set; } = false;

        /// <summary>
        /// Keep images in the output.
        /// </summary>
        public bool PreserveImages { get; set; } = false;

        /// <summary>
        /// Do not expand ligatures into constituent characters.
        /// </summary>
        public bool PreserveLigatures { get; set; } = false;

        /// <summary>
        /// Do not convert all whitespace into space characters.
        /// </summary>
        public bool PreserveWhitespace { get; set; } = false;

        /// <summary>
        /// Do not merge spans on the same line.
        /// </summary>
        public bool PreserveSpans { get; set; } = false;

        /// <summary>
        /// Attempt to join up hyphenated words.
        /// </summary>
        public bool Dehyphenate { get; set; } = false;

        /// <summary>
        /// Guess unicode from CID if normal mapping fails.
        /// </summary>
        public bool UseCIDForUnknownUnicode { get; set; } = false;

        /// <summary>
        /// Include characters that are outside of the page's mediabox.
        /// </summary>
        public bool IncludeCharactersOutsideMediaBox { get; set; } = false;

        internal string GetOptionString()
        {
            StringBuilder sb = new StringBuilder();

            if (this.InhibitSpaces)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("inhibit-spaces=yes");
            }

            if (this.PreserveImages)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("preserve-images=yes");
            }

            if (this.PreserveLigatures)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("preserve-ligatures=yes");
            }

            if (this.PreserveWhitespace)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("preserve-whitespace=yes");
            }

            if (this.PreserveSpans)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("preserve-spans=yes");
            }

            if (this.Dehyphenate)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("dehyphenate=yes");
            }

            if (this.UseCIDForUnknownUnicode)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("use-cid-for-unknown-unicode=yes");
            }

            if (this.IncludeCharactersOutsideMediaBox)
            {
                if (sb.Length > 0)
                {
                    sb.Append(",");
                }
                sb.Append("mediabox-clip=no");
            }

            return sb.ToString();
        }
    }


    partial class MuPDFDocument
    {
        /// <summary>
        /// Create a new document containing the specified (parts of) pages from other documents.
        /// </summary>
        /// <param name="context">The context that was used to open the documents.</param>
        /// <param name="fileName">The output file name.</param>
        /// <param name="fileType">The output file format.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
        [Obsolete("Please use MuPDFDocument.Create.Document instead, or one of the specific methods for a document type in order to specify additional options.", false)]
        public static void CreateDocument(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, bool includeAnnotations = true, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => Create.Document(context, fileName, fileType, pages, includeAnnotations);

        /// <summary>
        /// Create a new document containing the specified pages from other documents.
        /// </summary>
        /// <param name="context">The context that was used to open the documents.</param>
        /// <param name="fileName">The output file name.</param>
        /// <param name="fileType">The output file format.</param>
        /// <param name="pages">The pages to include in the document.</param>
        /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
        [Obsolete("Please use MuPDFDocument.Create.Document instead, or one of the specific methods for a document type in order to specify additional options.", false)]
        public static void CreateDocument(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, bool includeAnnotations = true, params MuPDFPage[] pages) => Create.Document(context, fileName, fileType, pages, includeAnnotations);

        /// <summary>
        /// Contains methods to create documents by combining pages from other documents.
        /// </summary>
        public static class Create
        {
            private static void CreateDocument(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, string optionString, bool includeAnnotations)
            {
                if (fileType == DocumentOutputFileTypes.SVG && pages.Count() > 1)
                {
                    //Actually, you can, but the library creates multiple files appending numbers after each name (e.g. page1.svg, page2.svg, ...), which is ugly and may have unintended consequences.
                    //If you really want to do this, you can call this method multiple times.
                    throw new ArgumentException("You cannot create an SVG document with more than one page!", nameof(pages));
                }

                IntPtr documentWriter = IntPtr.Zero;
                ExitCodes result;

                // Encode the file name in UTF-8 in unmanaged memory.
                using (UTF8EncodedString encodedFileName = new UTF8EncodedString(fileName))
                using (UTF8EncodedString encodedOptions = new UTF8EncodedString(optionString))
                {
                    //Initialise document writer.
                    result = (ExitCodes)NativeMethods.CreateDocumentWriter(context.NativeContext, encodedFileName.Address, (int)fileType, encodedOptions.Address, ref documentWriter);
                }

                switch (result)
                {
                    case ExitCodes.EXIT_SUCCESS:
                        break;
                    case ExitCodes.ERR_CANNOT_CREATE_WRITER:
                        throw new MuPDFException("Cannot create the document writer", result);
                    default:
                        throw new MuPDFException("Unknown error", result);
                }

                int i = 0;

                //Write pages.
                foreach ((MuPDFPage page, Rectangle region, float zoom) pag in pages)
                {
                    MuPDFDocument doc = pag.page.OwnerDocument;
                    int pageNum = pag.page.PageNumber;

                    if (doc.DisplayLists[pageNum] == null)
                    {
                        doc.DisplayLists[pageNum] = new MuPDFDisplayList(doc.OwnerContext, doc.Pages[pageNum], includeAnnotations);
                    }

                    Rectangle region = pag.region;
                    double zoom = pag.zoom;

                    if (pag.page.OwnerDocument.ImageXRes != 72 || pag.page.OwnerDocument.ImageYRes != 72)
                    {
                        zoom *= Math.Sqrt(pag.page.OwnerDocument.ImageXRes * pag.page.OwnerDocument.ImageYRes) / 72;
                        region = new Rectangle(region.X0 * 72 / pag.page.OwnerDocument.ImageXRes, region.Y0 * 72 / pag.page.OwnerDocument.ImageYRes, region.X1 * 72 / pag.page.OwnerDocument.ImageXRes, region.Y1 * 72 / pag.page.OwnerDocument.ImageYRes);
                    }

                    result = (ExitCodes)NativeMethods.WriteSubDisplayListAsPage(context.NativeContext, doc.DisplayLists[pageNum].NativeDisplayList, region.X0, region.Y0, region.X1, region.Y1, (float)zoom, documentWriter);

                    switch (result)
                    {
                        case ExitCodes.EXIT_SUCCESS:
                            break;
                        case ExitCodes.ERR_CANNOT_RENDER:
                            throw new MuPDFException("Cannot render page " + i.ToString(), result);
                        default:
                            throw new MuPDFException("Unknown error", result);
                    }

                    i++;
                }

                //Close and dispose the document writer.
                result = (ExitCodes)NativeMethods.FinalizeDocumentWriter(context.NativeContext, documentWriter);

                switch (result)
                {
                    case ExitCodes.EXIT_SUCCESS:
                        break;
                    case ExitCodes.ERR_CANNOT_CLOSE_DOCUMENT:
                        throw new MuPDFException("Cannot finalise the document", result);
                    default:
                        throw new MuPDFException("Unknown error", result);
                }
            }

            /// <summary>
            /// Create a new document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="fileType">The output file format.</param>
            /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void Document(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, bool includeAnnotations = true) => CreateDocument(context, fileName, fileType, pages, "", includeAnnotations);

            /// <summary>
            /// Create a new document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="fileType">The output file format.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void Document(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => Document(context, fileName, fileType, pages, true);

            /// <summary>
            /// Create a new document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="fileType">The output file format.</param>
            /// <param name="includeAnnotations">If this is <see langword="true" />, annotations (e.g. signatures) are included in the display list that is generated. Otherwise, only the page contents are included.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void Document(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, IEnumerable<MuPDFPage> pages, bool includeAnnotations = true) => Document(context, fileName, fileType, pages.Select(x => (x, x.Bounds, 1.0f)), includeAnnotations);

            /// <summary>
            /// Create a new document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="fileType">The output file format.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void Document(MuPDFContext context, string fileName, DocumentOutputFileTypes fileType, params MuPDFPage[] pages) => Document(context, fileName, fileType, pages, true);


            /// <summary>
            /// Create a new PDF document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            /// <param name="options">Options for the output format.</param>
            public static void PDFDocument(MuPDFContext context, string fileName, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, PDFCreationOptions options = default)
            {
                options = options ?? new PDFCreationOptions();
                string optionString = options.GetOptionString();
                CreateDocument(context, fileName, DocumentOutputFileTypes.PDF, pages, optionString, options.IncludeAnnotations);
            }

            /// <summary>
            /// Create a new PDF document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void PDFDocument(MuPDFContext context, string fileName, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => PDFDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new PDF document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            /// <param name="options">Options for the output format.</param>
            public static void PDFDocument(MuPDFContext context, string fileName, IEnumerable<MuPDFPage> pages, PDFCreationOptions options = default) => PDFDocument(context, fileName, pages.Select(x => (x, x.Bounds, 1.0f)), options);

            /// <summary>
            /// Create a new PDF document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void PDFDocument(MuPDFContext context, string fileName, params MuPDFPage[] pages) => PDFDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new SVG document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="page">The page to include in the document.</param>
            /// <param name="region">The area of the page that should be included in the document</param>
            /// <param name="zoom">How much the region should be scaled.</param>
            /// <param name="options">Options for the output format.</param>
            public static void SVGDocument(MuPDFContext context, string fileName, MuPDFPage page, Rectangle region, float zoom, SVGCreationOptions options = default)
            {
                options = options ?? new SVGCreationOptions();
                string optionString = options.GetOptionString();

                string originalFileName = fileName;
                //For SVG documents, the library annoyingly alters the output file name, appending a "1" just before the extension (e.g. document.svg -> document1.svg). Since users may not be expecting this, it is best to render to a temporary file and then move it to the specified location.
                fileName = Path.GetTempFileName();

                CreateDocument(context, fileName, DocumentOutputFileTypes.SVG, new (MuPDFPage, Rectangle, float)[] { (page, region, zoom) }, optionString, options.IncludeAnnotations);

                //Move the temporary file to the location specified by the user.
                //The library has altered the temporary file name by appending a "1" before the extension.
                string tempFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName) + "1" + Path.GetExtension(fileName));

                //Overwrite existing file.
                if (File.Exists(originalFileName))
                {
                    File.Delete(originalFileName);
                }

                File.Move(tempFileName, originalFileName);
            }

            /// <summary>
            /// Create a new SVG document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="page">The page to include in the document.</param>
            /// <param name="options">Options for the output format.</param>
            public static void SVGDocument(MuPDFContext context, string fileName, MuPDFPage page, SVGCreationOptions options = default) => SVGDocument(context, fileName, page, page.Bounds, 1.0f, options);

            /// <summary>
            /// Create a new CBZ document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            /// <param name="options">Options for the output format.</param>
            public static void CBZDocument(MuPDFContext context, string fileName, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, CBZCreationOptions options = default)
            {
                options = options ?? new CBZCreationOptions();
                string optionString = options.GetOptionString();
                CreateDocument(context, fileName, DocumentOutputFileTypes.CBZ, pages, optionString, options.IncludeAnnotations);
            }

            /// <summary>
            /// Create a new CBZ document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void CBZDocument(MuPDFContext context, string fileName, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => CBZDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new CBZ document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            /// <param name="options">Options for the output format.</param>
            public static void CBZDocument(MuPDFContext context, string fileName, IEnumerable<MuPDFPage> pages, CBZCreationOptions options = default) => CBZDocument(context, fileName, pages.Select(x => (x, x.Bounds, 1.0f)), options);

            /// <summary>
            /// Create a new CBZ document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void CBZDocument(MuPDFContext context, string fileName, params MuPDFPage[] pages) => CBZDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new text document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            /// <param name="options">Options for the output format.</param>
            public static void TextDocument(MuPDFContext context, string fileName, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, TXTCreationOptions options = default)
            {
                options = options ?? new TXTCreationOptions();
                string optionString = options.GetOptionString();
                CreateDocument(context, fileName, DocumentOutputFileTypes.TXT, pages, optionString, options.IncludeAnnotations);
            }

            /// <summary>
            /// Create a new text document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void TextDocument(MuPDFContext context, string fileName, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => TextDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new text document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            /// <param name="options">Options for the output format.</param>
            public static void TextDocument(MuPDFContext context, string fileName, IEnumerable<MuPDFPage> pages, TXTCreationOptions options = default) => TextDocument(context, fileName, pages.Select(x => (x, x.Bounds, 1.0f)), options);

            /// <summary>
            /// Create a new text document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void TextDocument(MuPDFContext context, string fileName, params MuPDFPage[] pages) => TextDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new structured text XML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            /// <param name="options">Options for the output format.</param>
            public static void StructuredTextDocument(MuPDFContext context, string fileName, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, TXTCreationOptions options = default)
            {
                options = options ?? new TXTCreationOptions();
                string optionString = options.GetOptionString();
                CreateDocument(context, fileName, DocumentOutputFileTypes.StructuredText, pages, optionString, options.IncludeAnnotations);
            }

            /// <summary>
            /// Create a new structured text XML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void StructuredTextDocument(MuPDFContext context, string fileName, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => StructuredTextDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new structured text XML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            /// <param name="options">Options for the output format.</param>
            public static void StructuredTextDocument(MuPDFContext context, string fileName, IEnumerable<MuPDFPage> pages, TXTCreationOptions options = default) => StructuredTextDocument(context, fileName, pages.Select(x => (x, x.Bounds, 1.0f)), options);

            /// <summary>
            /// Create a new structured text XML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void StructuredTextDocument(MuPDFContext context, string fileName, params MuPDFPage[] pages) => StructuredTextDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new HTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            /// <param name="options">Options for the output format.</param>
            public static void HTMLDocument(MuPDFContext context, string fileName, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, HTMLCreationOptions options = default)
            {
                options = options ?? new HTMLCreationOptions();
                string optionString = options.GetOptionString();
                CreateDocument(context, fileName, DocumentOutputFileTypes.HTML, pages, optionString, options.IncludeAnnotations);
            }

            /// <summary>
            /// Create a new HTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void HTMLDocument(MuPDFContext context, string fileName, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => HTMLDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new HTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            /// <param name="options">Options for the output format.</param>
            public static void HTMLDocument(MuPDFContext context, string fileName, IEnumerable<MuPDFPage> pages, HTMLCreationOptions options = default) => HTMLDocument(context, fileName, pages.Select(x => (x, x.Bounds, 1.0f)), options);

            /// <summary>
            /// Create a new HTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void HTMLDocument(MuPDFContext context, string fileName, params MuPDFPage[] pages) => HTMLDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new XHTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            /// <param name="options">Options for the output format.</param>
            public static void XHTMLDocument(MuPDFContext context, string fileName, IEnumerable<(MuPDFPage page, Rectangle region, float zoom)> pages, HTMLCreationOptions options = default)
            {
                options = options ?? new HTMLCreationOptions();
                string optionString = options.GetOptionString();
                CreateDocument(context, fileName, DocumentOutputFileTypes.XHTML, pages, optionString, options.IncludeAnnotations);
            }

            /// <summary>
            /// Create a new XHTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document. The "page" element specifies the page, the "region" element the area of the page that should be included in the document, and the "zoom" element how much the region should be scaled.</param>
            public static void XHTMLDocument(MuPDFContext context, string fileName, params (MuPDFPage page, Rectangle region, float zoom)[] pages) => XHTMLDocument(context, fileName, pages, null);

            /// <summary>
            /// Create a new XHTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            /// <param name="options">Options for the output format.</param>
            public static void XHTMLDocument(MuPDFContext context, string fileName, IEnumerable<MuPDFPage> pages, HTMLCreationOptions options = default) => XHTMLDocument(context, fileName, pages.Select(x => (x, x.Bounds, 1.0f)), options);

            /// <summary>
            /// Create a new XHTML document containing the specified (parts of) pages from other documents.
            /// </summary>
            /// <param name="context">The context that was used to open the documents.</param>
            /// <param name="fileName">The output file name.</param>
            /// <param name="pages">The pages to include in the document.</param>
            public static void XHTMLDocument(MuPDFContext context, string fileName, params MuPDFPage[] pages) => XHTMLDocument(context, fileName, pages, null);
        }
    }
}
