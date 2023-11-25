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
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]
namespace MuPDFCore
{
    /// <summary>
    /// Exit codes returned by native methods describing various errors that can occur.
    /// </summary>
    public enum ExitCodes
    {
        /// <summary>
        /// An error occurred while creating the context object.
        /// </summary>
        ERR_CANNOT_CREATE_CONTEXT = 129,

        /// <summary>
        /// An error occurred while registering the default document handlers with the context.
        /// </summary>
        ERR_CANNOT_REGISTER_HANDLERS = 130,

        /// <summary>
        /// An error occurred while opening a file.
        /// </summary>
        ERR_CANNOT_OPEN_FILE = 131,

        /// <summary>
        /// An error occurred while determining the total number of pages in the document.
        /// </summary>
        ERR_CANNOT_COUNT_PAGES = 132,

        /// <summary>
        /// An error occurred while rendering the page.
        /// </summary>
        ERR_CANNOT_RENDER = 134,

        /// <summary>
        /// An error occurred while opening the stream.
        /// </summary>
        ERR_CANNOT_OPEN_STREAM = 135,

        /// <summary>
        /// An error occurred while loading the page.
        /// </summary>
        ERR_CANNOT_LOAD_PAGE = 136,

        /// <summary>
        /// An error occurred while computing the page bounds.
        /// </summary>
        ERR_CANNOT_COMPUTE_BOUNDS = 137,

        /// <summary>
        /// An error occurred while initialising the mutexes for the lock mechanism.
        /// </summary>
        ERR_CANNOT_INIT_MUTEX = 138,

        /// <summary>
        /// An error occurred while cloning the context.
        /// </summary>
        ERR_CANNOT_CLONE_CONTEXT = 139,

        /// <summary>
        /// An error occurred while saving the page to a raster image file.
        /// </summary>
        ERR_CANNOT_SAVE = 140,

        /// <summary>
        /// An error occurred while creating the output buffer.
        /// </summary>
        ERR_CANNOT_CREATE_BUFFER = 141,

        /// <summary>
        /// An error occurred while creating the document writer.
        /// </summary>
        ERR_CANNOT_CREATE_WRITER = 142,

        /// <summary>
        /// An error occurred while finalising the document file.
        /// </summary>
        ERR_CANNOT_CLOSE_DOCUMENT = 143,

        /// <summary>
        /// An error occurred while creating an empty structured text page.
        /// </summary>
        ERR_CANNOT_CREATE_PAGE = 144,

        /// <summary>
        /// An error occurred while populating the structured text page
        /// </summary>
        ERR_CANNOT_POPULATE_PAGE = 145,

        /// <summary>
        /// No error occurred. All is well.
        /// </summary>
        EXIT_SUCCESS = 0
    }

    /// <summary>
    /// File types supported in input by the library.
    /// </summary>
    public enum InputFileTypes
    {
        /// <summary>
        /// Portable Document Format.
        /// </summary>
        PDF = 0,

        /// <summary>
        /// XML Paper Specification document.
        /// </summary>
        XPS = 1,

        /// <summary>
        /// Comic book archive file (ZIP archive containing page scans).
        /// </summary>
        CBZ = 2,

        /// <summary>
        /// Portable Network Graphics format.
        /// </summary>
        PNG = 3,

        /// <summary>
        /// Joint Photographic Experts Group image.
        /// </summary>
        JPEG = 4,

        /// <summary>
        /// Bitmap image.
        /// </summary>
        BMP = 5,

        /// <summary>
        /// Graphics Interchange Format.
        /// </summary>
        GIF = 6,

        /// <summary>
        /// Tagged Image File Format.
        /// </summary>
        TIFF = 7,

        /// <summary>
        /// Portable aNyMap graphics format.
        /// </summary>
        PNM = 8,

        /// <summary>
        /// Portable Arbitrary Map graphics format.
        /// </summary>
        PAM = 9,

        /// <summary>
        /// Electronic PUBlication document.
        /// </summary>
        EPUB = 10,

        /// <summary>
        /// FictionBook document.
        /// </summary>
        FB2 = 11
    }

    /// <summary>
    /// Raster image file types supported in output by the library.
    /// </summary>
    public enum RasterOutputFileTypes
    {
        /// <summary>
        /// Portable aNyMap graphics format.
        /// </summary>
        PNM = 0,

        /// <summary>
        /// Portable Arbitrary Map graphics format.
        /// </summary>
        PAM = 1,

        /// <summary>
        /// Portable Network Graphics format.
        /// </summary>
        PNG = 2,

        /// <summary>
        /// PhotoShop Document format.
        /// </summary>
        PSD = 3,

        /// <summary>
        /// Joint Photographic Experts Group format, with quality level 90.
        /// </summary>
        JPEG = 4
    };

    /// <summary>
    /// Document file types supported in output by the library.
    /// </summary>
    public enum DocumentOutputFileTypes
    {
        /// <summary>
        /// Portable Document Format.
        /// </summary>
        PDF = 0,

        /// <summary>
        /// Scalable Vector Graphics.
        /// </summary>
        SVG = 1,

        /// <summary>
        /// Comic book archive format.
        /// </summary>
        CBZ = 2
    };

    /// <summary>
    /// Pixel formats supported by the library.
    /// </summary>
    public enum PixelFormats
    {
        /// <summary>
        /// 24bpp RGB format.
        /// </summary>
        RGB = 0,

        /// <summary>
        /// 32bpp RGBA format.
        /// </summary>
        RGBA = 1,

        /// <summary>
        /// 24bpp BGR format.
        /// </summary>
        BGR = 2,

        /// <summary>
        /// 32bpp BGRA format.
        /// </summary>
        BGRA = 3
    }

    /// <summary>
    /// Possible document encryption states.
    /// </summary>
    public enum EncryptionState
    {
        /// <summary>
        /// The document is not encrypted.
        /// </summary>
        Unencrypted = 0,

        /// <summary>
        /// The document is encrypted and a user password is necessary to render it.
        /// </summary>
        Encrypted = 1,

        /// <summary>
        /// The document is encrypted and the correct user password has been supplied.
        /// </summary>
        Unlocked = 2
    }

    /// <summary>
    /// Possible document restriction states.
    /// </summary>
    public enum RestrictionState
    {
        /// <summary>
        /// The document does not have any restrictions associated to it.
        /// </summary>
        Unrestricted = 0,

        /// <summary>
        /// Some restrictions apply to the document. An owner password is required to remove these restrictions.
        /// </summary>
        Restricted = 1,

        /// <summary>
        /// The document had some restrictions and the correct owner password has been supplied.
        /// </summary>
        Unlocked = 2
    }

    /// <summary>
    /// Document restrictions.
    /// </summary>
    public enum DocumentRestrictions
    {
        /// <summary>
        /// No operation is restricted.
        /// </summary>
        None = 0,

        /// <summary>
        /// Printing the document is restricted.
        /// </summary>
        Print = 1,

        /// <summary>
        /// Copying the document is restricted.
        /// </summary>
        Copy = 2,

        /// <summary>
        /// Editing the document is restricted.
        /// </summary>
        Edit = 4,

        /// <summary>
        /// Annotating the document is restricted.
        /// </summary>
        Annotate = 8
    }

    /// <summary>
    /// Password types.
    /// </summary>
    public enum PasswordTypes
    {
        /// <summary>
        /// No password.
        /// </summary>
        None = 0,

        /// <summary>
        /// The password corresponds to the user password.
        /// </summary>
        User = 1,

        /// <summary>
        /// The password corresponds to the owner password.
        /// </summary>
        Owner = 2
    }

    /// <summary>
    /// A struct to hold information about the current rendering process and to abort rendering as needed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Cookie
    {
        public int abort;
        public int progress;
        public ulong progress_max;
        public int errors;
        public int incomplete;
    }

    /// <summary>
    /// Holds a summary of the progress of the current rendering operation.
    /// </summary>
    public class RenderProgress
    {
        /// <summary>
        /// Holds the progress of a single thread.
        /// </summary>
        public struct ThreadRenderProgress
        {
            /// <summary>
            /// The current progress.
            /// </summary>
            public int Progress;

            /// <summary>
            /// The maximum progress. If this is 0, this value could not be determined (yet).
            /// </summary>
            public long MaxProgress;

            internal ThreadRenderProgress(int progress, ulong maxProgress)
            {
                this.Progress = progress;
                this.MaxProgress = (long)maxProgress;
            }
        }

        /// <summary>
        /// Contains the progress of all the threads used in rendering the document.
        /// </summary>
        public ThreadRenderProgress[] ThreadRenderProgresses { get; private set; }

        internal RenderProgress(ThreadRenderProgress[] threadRenderProgresses)
        {
            ThreadRenderProgresses = threadRenderProgresses;
        }
    }

    /// <summary>
    /// An <see cref="IDisposable"/> wrapper around an <see cref="IntPtr"/> that frees the allocated memory when it is disposed.
    /// </summary>
    public class DisposableIntPtr : IDisposable
    {
        /// <summary>
        /// The pointer to the unmanaged memory.
        /// </summary>
        private readonly IntPtr InternalPointer;

        /// <summary>
        /// The number of bytes that have been allocated, for adding memory pressure.
        /// </summary>
        private readonly long BytesAllocated = -1;

        /// <summary>
        /// Create a new DisposableIntPtr.
        /// </summary>
        /// <param name="pointer">The pointer that should be freed upon disposing of this object.</param>
        public DisposableIntPtr(IntPtr pointer)
        {
            this.InternalPointer = pointer;
        }

        /// <summary>
        /// Create a new DisposableIntPtr, adding memory pressure to the GC to account for the allocation of unmanaged memory.
        /// </summary>
        /// <param name="pointer">The pointer that should be freed upon disposing of this object.</param>
        /// <param name="bytesAllocated">The number of bytes that have been allocated, for adding memory pressure.</param>
        public DisposableIntPtr(IntPtr pointer, long bytesAllocated)
        {
            this.InternalPointer = pointer;
            this.BytesAllocated = bytesAllocated;

            if (BytesAllocated > 0)
            {
                GC.AddMemoryPressure(bytesAllocated);
            }
        }

        private bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Marshal.FreeHGlobal(InternalPointer);

                if (BytesAllocated > 0)
                {
                    GC.RemoveMemoryPressure(BytesAllocated);
                }

                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~DisposableIntPtr()
        {
            Dispose(disposing: false);
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// The exception that is thrown when a MuPDF operation fails.
    /// </summary>
    public class MuPDFException : Exception
    {
        /// <summary>
        /// The <see cref="ExitCodes"/> returned by the native function.
        /// </summary>
        public readonly ExitCodes ErrorCode;

        internal MuPDFException(string message, ExitCodes errorCode) : base(message)
        {
            this.ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// The exception that is thrown when an attempt is made to render an encrypted document without supplying the required password.
    /// </summary>
    public class DocumentLockedException : Exception
    {
        internal DocumentLockedException(string message) : base(message) { }
    }

    /// <summary>
    /// A class to simplify passing a string to the MuPDF C library with the correct encoding.
    /// </summary>
    internal class UTF8EncodedString : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// The address of the bytes encoding the string in unmanaged memory.
        /// </summary>
        public IntPtr Address { get; }

        /// <summary>
        /// Create a null-terminated, UTF-8 encoded string in unmanaged memory.
        /// </summary>
        /// <param name="text"></param>
        public UTF8EncodedString(string text)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(text);

            IntPtr dataHolder = Marshal.AllocHGlobal(data.Length + 1);
            Marshal.Copy(data, 0, dataHolder, data.Length);
            Marshal.WriteByte(dataHolder, data.Length, 0);

            this.Address = dataHolder;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Marshal.FreeHGlobal(Address);
                disposedValue = true;
            }
        }

        ~UTF8EncodedString()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// EventArgs for the <see cref="MuPDF.StandardOutputMessage"/> and <see cref="MuPDF.StandardErrorMessage"/> events.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// The message that has been logged.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Create a new <see cref="MessageEventArgs"/> instance.
        /// </summary>
        /// <param name="message">The message that has been logged.</param>
        public MessageEventArgs(string message)
        {
            this.Message = message;
        }
    }

    /// <summary>
    /// Contains static methods to perform setup operations.
    /// </summary>
    public static class MuPDF
    {
        private static int StdOutFD = -1;
        private static int StdErrFD = -1;

        private static TextWriter ConsoleOut;
        private static TextWriter ConsoleErr;

        private static ConsoleColor DefaultForeground;
        private static ConsoleColor DefaultBackground;

        private static string PipeName;
        private static bool CleanupRegistered = false;
        private static object CleanupLock = new object();

        /// <summary>
        /// This event is invoked when <see cref="RedirectOutput"/> has been called and the native MuPDF library writes to the standard output stream.
        /// </summary>
        public static event EventHandler<MessageEventArgs> StandardOutputMessage;

        /// <summary>
        /// This event is invoked when <see cref="RedirectOutput"/> has been called and the native MuPDF library writes to the standard error stream.
        /// </summary>
        public static event EventHandler<MessageEventArgs> StandardErrorMessage;

        /// <summary>
        /// Redirects output messages from the native MuPDF library to the <see cref="StandardOutputMessage"/> and <see cref="StandardErrorMessage"/> events. Note that this has side-effects.
        /// </summary>
        /// <returns>A <see cref="Task"/> that finishes when the output streams have been redirected.</returns>
        public static async Task RedirectOutput()
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                await RedirectOutputWindows();
            }
            else
            {
                await RedirectOutputUnix();
            }

            if (!CleanupRegistered)
            {
                AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    ResetOutput();
                };
            }
        }

        const int UnixMaxPipeLength = 107;

        private static async Task RedirectOutputUnix()
        {
            if (StdOutFD < 0 && StdErrFD < 0)
            {
                string tempPath = Path.GetTempPath();

                string pipeName = "MuPDFCore-" + Guid.NewGuid().ToString();

                pipeName = pipeName.Substring(0, Math.Min(pipeName.Length, UnixMaxPipeLength - tempPath.Length - 4));
                pipeName = Path.Combine(tempPath, pipeName);

                PipeName = pipeName;

                Task redirectOutputTask = System.Threading.Tasks.Task.Run(() =>
                {
                    NativeMethods.RedirectOutput(out StdOutFD, out StdErrFD, pipeName + "-out", pipeName + "-err");
                });

                // Start stdout pipe (this is actually a socket)
                _ = Task.Run(() =>
                {
                    using (NamedPipeClientStream client = new NamedPipeClientStream(pipeName + "-out"))
                    {
                        while (true)
                        {
                            try
                            {
                                client.Connect(100);
                                break;
                            }
                            catch { }
                        }

                        using (StreamReader reader = new StreamReader(client))
                        {
                            while (true)
                            {
                                string message = reader.ReadLine();

                                if (!string.IsNullOrEmpty(message))
                                {
                                    StandardOutputMessage?.Invoke(null, new MessageEventArgs(message));
                                }
                            }
                        }
                    }

                });

                // Start stderr pipe (this is actually a socket)
                _ = Task.Run(() =>
                {
                    using (NamedPipeClientStream client = new NamedPipeClientStream(pipeName + "-err"))
                    {
                        while (true)
                        {
                            try
                            {
                                client.Connect(100);
                                break;
                            }
                            catch { }
                        }

                        using (StreamReader reader = new StreamReader(client))
                        {
                            while (true)
                            {
                                string message = reader.ReadLine();

                                if (!string.IsNullOrEmpty(message))
                                {
                                    StandardErrorMessage?.Invoke(null, new MessageEventArgs(message));
                                }
                            }
                        }
                    }

                });

                await redirectOutputTask;

                ConsoleOut = Console.Out;
                ConsoleErr = Console.Error;

                ConsoleColor fg = Console.ForegroundColor;
                ConsoleColor bg = Console.BackgroundColor;

                Console.ResetColor();

                DefaultForeground = Console.ForegroundColor;
                DefaultBackground = Console.BackgroundColor;

                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;

                Console.SetOut(new FileDescriptorTextWriter(Console.Out.Encoding, StdOutFD));
                Console.SetError(new FileDescriptorTextWriter(Console.Error.Encoding, StdErrFD));
            }
        }

        private static async Task RedirectOutputWindows()
        {
            if (StdOutFD < 0 && StdErrFD < 0)
            {
                string pipeName = "MuPDFCore-" + Guid.NewGuid().ToString();

                Task redirectOutputTask = System.Threading.Tasks.Task.Run(() =>
                {
                    NativeMethods.RedirectOutput(out StdOutFD, out StdErrFD, "\\\\.\\pipe\\" + pipeName + "-out", "\\\\.\\pipe\\" + pipeName + "-err");
                });

                // Start stdout pipe
                _ = Task.Run(() =>
                {
                    using (NamedPipeClientStream client = new NamedPipeClientStream(pipeName + "-out"))
                    {
                        while (true)
                        {
                            try
                            {
                                client.Connect(100);
                                break;
                            }
                            catch { }
                        }

                        using (StreamReader reader = new StreamReader(client))
                        {
                            while (true)
                            {
                                string message = reader.ReadLine();

                                if (!string.IsNullOrEmpty(message))
                                {
                                    StandardOutputMessage?.Invoke(null, new MessageEventArgs(message));
                                }
                            }
                        }
                    }

                });

                // Start stderr pipe
                _ = Task.Run(() =>
                {
                    using (NamedPipeClientStream client = new NamedPipeClientStream(pipeName + "-err"))
                    {
                        while (true)
                        {
                            try
                            {
                                client.Connect(100);
                                break;
                            }
                            catch { }
                        }

                        using (StreamReader reader = new StreamReader(client))
                        {
                            while (true)
                            {
                                string message = reader.ReadLine();

                                if (!string.IsNullOrEmpty(message))
                                {
                                    StandardErrorMessage?.Invoke(null, new MessageEventArgs(message));
                                }
                            }
                        }
                    }

                });

                await redirectOutputTask;

                ConsoleOut = Console.Out;
                ConsoleErr = Console.Error;

                ConsoleColor fg = Console.ForegroundColor;
                ConsoleColor bg = Console.BackgroundColor;

                Console.ResetColor();

                DefaultForeground = Console.ForegroundColor;
                DefaultBackground = Console.BackgroundColor;

                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;

                Console.SetOut(new FileDescriptorTextWriter(Console.Out.Encoding, StdOutFD));
                Console.SetError(new FileDescriptorTextWriter(Console.Error.Encoding, StdErrFD));
            }
        }

        /// <summary>
        /// Reset the default standard output and error streams for the native MuPDF library.
        /// </summary>
        public static void ResetOutput()
        {
            lock (CleanupLock)
            {
                if (StdOutFD >= 0 && StdErrFD >= 0)
                {
                    NativeMethods.ResetOutput(StdOutFD, StdErrFD);

                    Console.SetOut(ConsoleOut);
                    Console.SetError(ConsoleErr);

                    StdOutFD = -1;
                    StdErrFD = -1;

                    if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        File.Delete(PipeName + "-out");
                        File.Delete(PipeName + "-err");
                    }
                }
            }
        }

        internal class FileDescriptorTextWriter : TextWriter
        {
            public override Encoding Encoding { get; }
            private int FileDescriptor { get; }

            public FileDescriptorTextWriter(Encoding encoding, int fileDescriptor)
            {
                this.Encoding = encoding;
                this.FileDescriptor = fileDescriptor;
            }

            public override void Write(string value)
            {
                StringBuilder sb = new StringBuilder();

                if (Console.ForegroundColor != DefaultForeground || Console.BackgroundColor != DefaultBackground)
                {
                    sb.Append("[");
                }

                if (Console.ForegroundColor != DefaultForeground)
                {
                    switch (Console.ForegroundColor)
                    {
                        case ConsoleColor.Black:
                            sb.Append("30");
                            break;
                        case ConsoleColor.DarkRed:
                            sb.Append("31");
                            break;
                        case ConsoleColor.DarkGreen:
                            sb.Append("32");
                            break;
                        case ConsoleColor.DarkYellow:
                            sb.Append("33");
                            break;
                        case ConsoleColor.DarkBlue:
                            sb.Append("34");
                            break;
                        case ConsoleColor.DarkMagenta:
                            sb.Append("35");
                            break;
                        case ConsoleColor.DarkCyan:
                            sb.Append("36");
                            break;
                        case ConsoleColor.Gray:
                            sb.Append("37");
                            break;
                        case ConsoleColor.DarkGray:
                            sb.Append("90");
                            break;
                        case ConsoleColor.Red:
                            sb.Append("91");
                            break;
                        case ConsoleColor.Green:
                            sb.Append("92");
                            break;
                        case ConsoleColor.Yellow:
                            sb.Append("93");
                            break;
                        case ConsoleColor.Blue:
                            sb.Append("94");
                            break;
                        case ConsoleColor.Magenta:
                            sb.Append("95");
                            break;
                        case ConsoleColor.Cyan:
                            sb.Append("96");
                            break;
                        case ConsoleColor.White:
                            sb.Append("97");
                            break;
                    }
                }

                if (Console.ForegroundColor != DefaultForeground && Console.BackgroundColor != DefaultBackground)
                {
                    sb.Append(";");
                }

                if (Console.BackgroundColor != DefaultBackground)
                {
                    switch (Console.BackgroundColor)
                    {
                        case ConsoleColor.Black:
                            sb.Append("40");
                            break;
                        case ConsoleColor.DarkRed:
                            sb.Append("41");
                            break;
                        case ConsoleColor.DarkGreen:
                            sb.Append("42");
                            break;
                        case ConsoleColor.DarkYellow:
                            sb.Append("43");
                            break;
                        case ConsoleColor.DarkBlue:
                            sb.Append("44");
                            break;
                        case ConsoleColor.DarkMagenta:
                            sb.Append("45");
                            break;
                        case ConsoleColor.DarkCyan:
                            sb.Append("46");
                            break;
                        case ConsoleColor.Gray:
                            sb.Append("47");
                            break;
                        case ConsoleColor.DarkGray:
                            sb.Append("100");
                            break;
                        case ConsoleColor.Red:
                            sb.Append("101");
                            break;
                        case ConsoleColor.Green:
                            sb.Append("102");
                            break;
                        case ConsoleColor.Yellow:
                            sb.Append("103");
                            break;
                        case ConsoleColor.Blue:
                            sb.Append("104");
                            break;
                        case ConsoleColor.Magenta:
                            sb.Append("105");
                            break;
                        case ConsoleColor.Cyan:
                            sb.Append("106");
                            break;
                        case ConsoleColor.White:
                            sb.Append("107");
                            break;
                    }
                }

                if (Console.ForegroundColor != DefaultForeground || Console.BackgroundColor != DefaultBackground)
                {
                    sb.Append("m");
                }

                sb.Append(value);

                if (Console.ForegroundColor != DefaultForeground || Console.BackgroundColor != DefaultBackground)
                {
                    sb.Append("[");
                }

                if (Console.ForegroundColor != DefaultForeground)
                {
                    sb.Append("39");
                }

                if (Console.ForegroundColor != DefaultForeground && Console.BackgroundColor != DefaultBackground)
                {
                    sb.Append(";");
                }

                if (Console.BackgroundColor != DefaultBackground)
                {
                    sb.Append("49");
                }

                if (Console.ForegroundColor != DefaultForeground || Console.BackgroundColor != DefaultBackground)
                {
                    sb.Append("m");
                }

                NativeMethods.WriteToFileDescriptor(FileDescriptor, sb.ToString(), sb.Length);
            }

            public override void Write(char value)
            {
                Write(value.ToString());
            }

            public override void Write(char[] buffer)
            {
                Write(new string(buffer));
            }

            public override void Write(char[] buffer, int index, int count)
            {
                Write(new string(buffer, index, count));
            }

            public override void WriteLine(string value)
            {
                Write(value);
                WriteLine();
            }
        }
    }

    /// <summary>
    /// Native methods.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Create a MuPDF context object with the specified store size.
        /// </summary>
        /// <param name="store_size">Maximum size in bytes of the resource store.</param>
        /// <param name="out_ctx">A pointer to the native context object.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateContext(ulong store_size, ref IntPtr out_ctx);

        /// <summary>
        /// Free a context and its global store.
        /// </summary>
        /// <param name="ctx">A pointer to the native context to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeContext(IntPtr ctx);

        /// <summary>
        /// Evict items from the store until the total size of the objects in the store is reduced to a given percentage of its current size.
        /// </summary>
        /// <param name="ctx">The context whose store should be shrunk.</param>
        /// <param name="perc">Fraction of current size to reduce the store to.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ShrinkStore(IntPtr ctx, uint perc);

        /// <summary>
        /// Evict every item from the store.
        /// </summary>
        /// <param name="ctx">The context whose store should be emptied.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void EmptyStore(IntPtr ctx);

        /// <summary>
        /// Get the current size of the store.
        /// </summary>
        /// <param name="ctx">The context whose store's size should be determined.</param>
        /// <returns>The current size in bytes of the store.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong GetCurrentStoreSize(IntPtr ctx);

        /// <summary>
        /// Get the maximum size of the store.
        /// </summary>
        /// <param name="ctx">The context whose store's maximum size should be determined.</param>
        /// <returns>The maximum size in bytes of the store.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern ulong GetMaxStoreSize(IntPtr ctx);

        /// <summary>
        /// Set the current antialiasing levels.
        /// </summary>
        /// <param name="ctx">The context whose antialiasing levels should be set.</param>
        /// <param name="aa">The overall antialiasing level. Ignored if &lt; 0.</param>
        /// <param name="graphics_aa">The graphics antialiasing level. Ignored if &lt; 0.</param>
        /// <param name="text_aa">The text antialiasing level. Ignored if &lt; 0.</param>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void SetAALevel(IntPtr ctx, int aa, int graphics_aa, int text_aa);

        /// <summary>
        /// Get the current antialiasing levels.
        /// </summary>
        /// <param name="ctx">The context whose antialiasing levels should be retrieved.</param>
        /// <param name="out_aa">The overall antialiasing level.</param>
        /// <param name="out_graphics_aa">The graphics antialiasing level.</param>
        /// <param name="out_text_aa">The text antialiasing level.</param>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GetAALevel(IntPtr ctx, out int out_aa, out int out_graphics_aa, out int out_text_aa);

        /// <summary>
        /// Create a display list from a page.
        /// </summary>
        /// <param name="ctx">A pointer to the context used to create the document.</param>
        /// <param name="page">A pointer to the page that should be used to create the display list.</param>
        /// <param name="annotations">An integer indicating whether annotations should be included in the display list (1) or not (any other value).</param>
        /// <param name="out_display_list">A pointer to the newly-created display list.</param>
        /// <param name="out_x0">The left coordinate of the display list's bounds.</param>
        /// <param name="out_y0">The top coordinate of the display list's bounds.</param>
        /// <param name="out_x1">The right coordinate of the display list's bounds.</param>
        /// <param name="out_y1">The bottom coordinate of the display list's bounds.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetDisplayList(IntPtr ctx, IntPtr page, int annotations, ref IntPtr out_display_list, ref float out_x0, ref float out_y0, ref float out_x1, ref float out_y1);

        /// <summary>
        /// Free a display list.
        /// </summary>
        /// <param name="ctx">The context that was used to create the display list.</param>
        /// <param name="list">The display list to dispose.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeDisplayList(IntPtr ctx, IntPtr list);

        /// <summary>
        /// Create a new document from a stream.
        /// </summary>
        /// <param name="ctx">The context to which the document will belong.</param>
        /// <param name="data">A pointer to a byte array containing the data that makes up the document.</param>
        /// <param name="data_length">The length in bytes of the data that makes up the document.</param>
        /// <param name="file_type">The type (extension) of the document.</param>
        /// <param name="get_image_resolution">If this is not 0, try opening the stream as an image and return the actual resolution (in DPI) of the image. Otherwise (or if trying to open the stream as an image fails), the returned resolution will be -1.</param>
        /// <param name="out_doc">The newly created document.</param>
        /// <param name="out_str">The newly created stream (so that it can be disposed later).</param>
        /// <param name="out_page_count">The number of pages in the document.</param>
        /// <param name="out_image_xres">If the document is an image file, the horizontal resolution of the image.</param>
        /// <param name="out_image_yres">If the document is an image file, the vertical resolution of the image.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateDocumentFromStream(IntPtr ctx, IntPtr data, ulong data_length, string file_type, int get_image_resolution, ref IntPtr out_doc, ref IntPtr out_str, ref int out_page_count, ref float out_image_xres, ref float out_image_yres);

        /// <summary>
        /// Create a new document from a file name.
        /// </summary>
        /// <param name="ctx">The context to which the document will belong.</param>
        /// <param name="file_name">The path of the file to open, UTF-8 encoded.</param>
        /// <param name="get_image_resolution">If this is not 0, try opening the file as an image and return the actual resolution (in DPI) of the image. Otherwise (or if trying to open the file as an image fails), the returned resolution will be -1.</param>
        /// <param name="out_doc">The newly created document.</param>
        /// <param name="out_page_count">The number of pages in the document.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        /// <param name="out_image_xres">If the document is an image file, the horizontal resolution of the image.</param>
        /// <param name="out_image_yres">If the document is an image file, the vertical resolution of the image.</param>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateDocumentFromFile(IntPtr ctx, IntPtr file_name, int get_image_resolution, ref IntPtr out_doc, ref int out_page_count, ref float out_image_xres, ref float out_image_yres);

        /// <summary>
        /// Free a stream and its associated resources.
        /// </summary>
        /// <param name="ctx">The context that was used while creating the stream.</param>
        /// <param name="str">The stream to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeStream(IntPtr ctx, IntPtr str);

        /// <summary>
        /// Free a document and its associated resources.
        /// </summary>
        /// <param name="ctx">The context that was used in creating the document.</param>
        /// <param name="doc">The document to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeDocument(IntPtr ctx, IntPtr doc);

        /// <summary>
        /// Render (part of) a display list to an array of bytes starting at the specified pointer.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
        /// <param name="colorFormat">The pixel data format.</param>
        /// <param name="pixel_storage">A pointer indicating where the pixel bytes will be written. There must be enough space available!</param>
        /// <param name="cookie">A pointer to a cookie object that can be used to track progress and/or abort rendering. Can be null.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int RenderSubDisplayList(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, IntPtr pixel_storage, IntPtr cookie);

        /// <summary>
        /// Load a page from a document.
        /// </summary>
        /// <param name="ctx">The context to which the document belongs.</param>
        /// <param name="doc">The document from which the page should be extracted.</param>
        /// <param name="page_number">The page number.</param>
        /// <param name="out_page">The newly extracted page.</param>
        /// <param name="out_x">The left coordinate of the page's bounds.</param>
        /// <param name="out_y">The top coordinate of the page's bounds.</param>
        /// <param name="out_w">The width of the page.</param>
        /// <param name="out_h">The height of the page.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int LoadPage(IntPtr ctx, IntPtr doc, int page_number, ref IntPtr out_page, ref float out_x, ref float out_y, ref float out_w, ref float out_h);

        /// <summary>
        /// Free a page and its associated resources.
        /// </summary>
        /// <param name="ctx">The context to which the document containing the page belongs.</param>
        /// <param name="page">The page to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposePage(IntPtr ctx, IntPtr page);

        /// <summary>
        /// Create cloned contexts that can be used in multithreaded rendering.
        /// </summary>
        /// <param name="ctx">The original context to clone.</param>
        /// <param name="count">The number of cloned contexts to create.</param>
        /// <param name="out_contexts">An array of pointers to the cloned contexts.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CloneContext(IntPtr ctx, int count, IntPtr out_contexts);

        /// <summary>
        /// Save (part of) a display list to an image file in the specified format.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
        /// <param name="colorFormat">The pixel data format.</param>
        /// <param name="file_name">The path to the output file, UTF-8 encoded.</param>
        /// <param name="output_format">An integer equivalent to <see cref="RasterOutputFileTypes"/> specifying the output format.</param>
        /// <param name="quality">Quality level for the output format (where applicable).</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int SaveImage(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, IntPtr file_name, int output_format, int quality);

        /// <summary>
        /// Write (part of) a display list to an image buffer in the specified format.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
        /// <param name="colorFormat">The pixel data format.</param>
        /// <param name="output_format">An integer equivalent to <see cref="RasterOutputFileTypes"/> specifying the output format.</param>
        /// <param name="quality">Quality level for the output format (where applicable).</param>
        /// <param name="out_buffer">The address of the buffer on which the data has been written (only useful for disposing the buffer later).</param>
        /// <param name="out_data">The address of the byte array where the data has been actually written.</param>
        /// <param name="out_length">The length in bytes of the image data.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int WriteImage(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, int output_format, int quality, ref IntPtr out_buffer, ref IntPtr out_data, ref ulong out_length);

        /// <summary>
        /// Free a native buffer and its associated resources.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="buf">The buffer to free.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeBuffer(IntPtr ctx, IntPtr buf);

        /// <summary>
        /// Create a new document writer object.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="file_name">The name of file that will hold the writer's output, UTF-8 encoded.</param>
        /// <param name="format">An integer equivalent to <see cref="DocumentOutputFileTypes"/> specifying the output format.</param>
        /// <param name="out_document_writer">A pointer to the new document writer object.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CreateDocumentWriter(IntPtr ctx, IntPtr file_name, int format, ref IntPtr out_document_writer);

        /// <summary>
        /// Render (part of) a display list as a page in the specified document writer.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list to render.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This will determine the final size of the page.</param>
        /// <param name="writ">The document writer on which the page should be written.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int WriteSubDisplayListAsPage(IntPtr ctx, IntPtr list, float x0, float y0, float x1, float y1, float zoom, IntPtr writ);

        /// <summary>
        /// Finalise a document writer, closing the file and freeing all resources.
        /// </summary>
        /// <param name="ctx">The context that was used to create the document writer.</param>
        /// <param name="writ">The document writer to finalise.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int FinalizeDocumentWriter(IntPtr ctx, IntPtr writ);

        /// <summary>
        /// Get the contents of a structured text character.
        /// </summary>
        /// <param name="character">The address of the character.</param>
        /// <param name="out_c">Unicode code point of the character.</param>
        /// <param name="out_color">An sRGB hex representation of the colour of the character.</param>
        /// <param name="out_origin_x">The x coordinate of the baseline origin of the character.</param>
        /// <param name="out_origin_y">The y coordinate of the baseline origin of the character.</param>
        /// <param name="out_size">The size in points of the character.</param>
        /// <param name="out_ll_x">The x coordinate of the lower left corner of the bounding quad.</param>
        /// <param name="out_ll_y">The y coordinate of the lower left corner of the bounding quad.</param>
        /// <param name="out_ul_x">The x coordinate of the upper left corner of the bounding quad.</param>
        /// <param name="out_ul_y">The y coordinate of the upper left corner of the bounding quad.</param>
        /// <param name="out_ur_x">The x coordinate of the upper right corner of the bounding quad.</param>
        /// <param name="out_ur_y">The y coordinate of the upper right corner of the bounding quad.</param>
        /// <param name="out_lr_x">The x coordinate of the lower right corner of the bounding quad.</param>
        /// <param name="out_lr_y">The y coordinate of the lower right corner of the bounding quad.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextChar(IntPtr character, ref int out_c, ref int out_color, ref float out_origin_x, ref float out_origin_y, ref float out_size, ref float out_ll_x, ref float out_ll_y, ref float out_ul_x, ref float out_ul_y, ref float out_ur_x, ref float out_ur_y, ref float out_lr_x, ref float out_lr_y);

        /// <summary>
        /// Get an array of structured text characters from a structured text line.
        /// </summary>
        /// <param name="line">The structured text line from which the characters should be extracted.</param>
        /// <param name="out_chars">An array of pointers to the structured text characters.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextChars(IntPtr line, IntPtr out_chars);

        /// <summary>
        /// Get the contents of a structured text line.
        /// </summary>
        /// <param name="line">The address of the line.</param>
        /// <param name="out_wmode">An integer equivalent to <see cref="MuPDFStructuredTextLine"/> representing the writing mode of the line.</param>
        /// <param name="out_x0">The left coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_y0">The top coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_x1">The right coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_y1">The bottom coordinate in page units of the bounding box of the line.</param>
        /// <param name="out_x">The x component of the normalised direction of the baseline.</param>
        /// <param name="out_y">The y component of the normalised direction of the baseline.</param>
        /// <param name="out_char_count">The number of characters in the line.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextLine(IntPtr line, ref int out_wmode, ref float out_x0, ref float out_y0, ref float out_x1, ref float out_y1, ref float out_x, ref float out_y, ref int out_char_count);

        /// <summary>
        /// Get an array of structured text lines from a structured text block.
        /// </summary>
        /// <param name="block">The structured text block from which the lines should be extracted.</param>
        /// <param name="out_lines">An array of pointers to the structured text lines.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextLines(IntPtr block, IntPtr out_lines);

        /// <summary>
        /// Get the contents of a structured text block.
        /// </summary>
        /// <param name="block">The address of the block.</param>
        /// <param name="out_type">An integer equivalent to <see cref="MuPDFStructuredTextBlock.Types"/> representing the type of the block.</param>
        /// <param name="out_x0">The left coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_y0">The top coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_x1">The right coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_y1">The bottom coordinate in page units of the bounding box of the block.</param>
        /// <param name="out_line_count">The number of lines in the block.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextBlock(IntPtr block, ref int out_type, ref float out_x0, ref float out_y0, ref float out_x1, ref float out_y1, ref int out_line_count);

        /// <summary>
        /// Get an array of structured text blocks from a structured text page.
        /// </summary>
        /// <param name="page">The structured text page from which the blocks should be extracted.</param>
        /// <param name="out_blocks">An array of pointers to the structured text blocks.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextBlocks(IntPtr page, IntPtr out_blocks);

        /// <summary>
        /// Get a structured text representation of a display list.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list whose structured text representation is sought.</param>
        /// <param name="out_page">The address of the structured text page.</param>
        /// <param name="out_stext_block_count">The number of structured text blocks in the page.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextPage(IntPtr ctx, IntPtr list, ref IntPtr out_page, ref int out_stext_block_count);

        /// <summary>
        /// Delegate defining a callback function that is invoked by the unmanaged MuPDF library to indicate OCR progress.
        /// </summary>
        /// <param name="progress">The current progress, ranging from 0 to 100.</param>
        /// <returns>This function should return 0 to indicate that the OCR process should continue, or 1 to indicate that it should be stopped.</returns>
        internal delegate int ProgressCallback(int progress);

        /// <summary>
        /// Get a structured text representation of a display list, using the Tesseract OCR engine.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="list">The display list whose structured text representation is sought.</param>
        /// <param name="out_page">The address of the structured text page.</param>
        /// <param name="out_stext_block_count">The number of structured text blocks in the page.</param>
        /// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the image that is passed to Tesseract.</param>
        /// <param name="x0">The left coordinate in page units of the region of the display list that should be analysed.</param>
        /// <param name="y0">The top coordinate in page units of the region of the display list that should be analysed.</param>
        /// <param name="x1">The right coordinate in page units of the region of the display list that should be analysed.</param>
        /// <param name="y1">The bottom coordinate in page units of the region of the display list that should be analysed.</param>
        /// <param name="prefix">A string value that will be used as an argument for the <c>putenv</c> function. If this is <see langword="null"/>, the <c>putenv</c> function is not invoked. Usually used to set the value of the <c>TESSDATA_PREFIX</c> environment variable.</param>
        /// <param name="language">The name of the language model file to use for the OCR.</param>
        /// <param name="callback">A progress callback function. This function will be called with an integer parameter ranging from 0 to 100 to indicate OCR progress, and should return 0 to continue or 1 to abort the OCR process.</param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetStructuredTextPageWithOCR(IntPtr ctx, IntPtr list, ref IntPtr out_page, ref int out_stext_block_count, float zoom, float x0, float y0, float x1, float y1, string prefix, string language, [MarshalAs(UnmanagedType.FunctionPtr)] ProgressCallback callback);

        /// <summary>
        /// Free a native structured text page and its associated resources.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="page"></param>
        /// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int DisposeStructuredTextPage(IntPtr ctx, IntPtr page);

        /// <summary>
        /// Redirect the standard output and standard error to named pipes with the specified names. On Windows, these are actually named pipes; on Linux and macOS, these are Unix sockets (matching the behaviour of System.IO.Pipes). Note that this has side-effects.
        /// </summary>
        /// <param name="stdoutFD">When the method returns, this variable will contain the file descriptor corresponding to the "real" stdout.</param>
        /// <param name="stderrFD">When the method returns, this variable will contain the file descriptor corresponding to the "real" stderr.</param>
        /// <param name="stdoutPipeName">The name of the pipe where stdout will be redirected. On Windows, this should be of the form "\\.\pipe\xxx", while on Linux and macOS it should be an absolute file path (maximum length 107/108 characters).</param>
        /// <param name="stderrPipeName">The name of the pipe where stderr will be redirected. On Windows, this should be of the form "\\.\pipe\xxx", while on Linux and macOS it should be an absolute file path (maximum length 107/108 characters).</param>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void RedirectOutput(out int stdoutFD, out int stderrFD, string stdoutPipeName, string stderrPipeName);

        /// <summary>
        /// Write the specified <paramref name="text"/> to a file descriptor. Use 1 for stdout and 2 for stderr (which may have been redirected).
        /// </summary>
        /// <param name="fileDescriptor">The file descriptor on which to write.</param>
        /// <param name="text">The text to write.</param>
        /// <param name="length">The length of the text to write (i.e., text.Length).</param>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void WriteToFileDescriptor(int fileDescriptor, string text, int length);

        /// <summary>
        /// Reset the standard output and standard error (or redirect them to the specified file descriptors, theoretically). Use with the <paramref name="stdoutFD"/> and <paramref name="stderrFD"/> returned by <see cref="RedirectOutput"/> to undo what it did.
        /// </summary>
        /// <param name="stdoutFD">The file descriptor corresponding to the "real" stdout.</param>
        /// <param name="stderrFD">The file descriptor corresponding to the "real" stderr.</param>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ResetOutput(int stdoutFD, int stderrFD);

        /// <summary>
        /// Unlocks a document with a password.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="doc">The document that needs to be unlocked.</param>
        /// <param name="password">The password to unlock the document.</param>
        /// <returns>0 if the document could not be unlocked, 1 if the document did not require unlocking in the first place, 2 if the document was unlocked using the user password and 4 if the document was unlocked using the owner password.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int UnlockWithPassword(IntPtr ctx, IntPtr doc, string password);

        /// <summary>
        /// Checks whether a password is required to open the document.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="doc">The document that needs to be checked.</param>
        /// <returns>0 if a password is not needed, 1 if a password is needed.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int CheckIfPasswordNeeded(IntPtr ctx, IntPtr doc);

        /// <summary>
        /// Returns the current permissions for the document. Note that these are not actually enforced.
        /// </summary>
        /// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
        /// <param name="doc">The document whose permissions need to be checked.</param>
        /// <returns>An integer with bit 0 set if the document can be printed, bit 1 set if it can be copied, bit 2 set if it can be edited, and bit 3 set if it can be annotated.</returns>
        [DllImport("MuPDFWrapper", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int GetPermissions(IntPtr ctx, IntPtr doc);
    }
}
