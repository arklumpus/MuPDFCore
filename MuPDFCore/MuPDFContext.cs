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
using System.Collections.Generic;

namespace MuPDFCore
{
    /// <summary>
    /// A wrapper around a MuPDF context object, which contains the exception stack and the resource cache store.
    /// </summary>
    public class MuPDFContext : IDisposable
    {
        /// <summary>
        /// Parent context for cloned contexts.
        /// </summary>
        private MuPDFContext ParentContext { get; } = null;

        /// <summary>
        /// A pointer to the native context object.
        /// </summary>
        internal readonly IntPtr NativeContext;

        /// <summary>
        /// The current size in bytes of the resource cache store. Read-only.
        /// </summary>
        public long StoreSize
        {
            get
            {
                return (long)NativeMethods.GetCurrentStoreSize(this.NativeContext);
            }
        }

        private object fontCacheLock = new object();
        private Dictionary<IntPtr, (MuPDFFont, int)> fontCache = new Dictionary<IntPtr, (MuPDFFont, int)>();

        /// <summary>
        /// Font cache dictionary.
        /// </summary>
        internal Dictionary<IntPtr, (MuPDFFont, int)> FontCache
        {
            get
            {
                return this.ParentContext == null ? fontCache : this.ParentContext.FontCache;
            }
        }

        /// <summary>
        /// Font cache dictionary lock.
        /// </summary>
        internal object FontCacheLock
        {
            get
            {
                return this.ParentContext == null ? fontCacheLock : this.ParentContext.FontCacheLock;
            }
        }

        /// <summary>
        /// The maximum size in bytes of the resource cache store. Read-only.
        /// </summary>
        public long StoreMaxSize
        {
            get
            {
                return (long)NativeMethods.GetMaxStoreSize(this.NativeContext);
            }
        }

        /// <summary>
        /// Sets the current anti-aliasing level. Changing this value will affect both
        /// the <see cref="TextAntiAliasing"/> and the <see cref="GraphicsAntiAliasing"/>.
        /// </summary>
        public int AntiAliasing
        {
            set
            {
                if (value < 0 || value > 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The anti-aliasing level must range between 0 and 8 (inclusive).");
                }

                NativeMethods.SetAALevel(this.NativeContext, value, -1, -1);
            }
        }

        /// <summary>
        /// Gets or sets the current text anti-aliasing level.
        /// </summary>
        public int TextAntiAliasing
        {
            get
            {
                NativeMethods.GetAALevel(this.NativeContext, out _, out _, out int tbr);
                return tbr;
            }

            set
            {
                if (value < 0 || value > 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The anti-aliasing level must range between 0 and 8 (inclusive).");
                }

                NativeMethods.SetAALevel(this.NativeContext, -1, -1, value);
            }
        }

        /// <summary>
        /// Gets or sets the current graphics anti-aliasing level.
        /// </summary>
        public int GraphicsAntiAliasing
        {
            get
            {
                NativeMethods.GetAALevel(this.NativeContext, out _, out int tbr, out _);
                return tbr;
            }

            set
            {
                if (value < 0 || value > 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The anti-aliasing level must range between 0 and 8 (inclusive).");
                }

                int prevTxt = this.TextAntiAliasing;
                NativeMethods.SetAALevel(this.NativeContext, -1, value, prevTxt);
            }
        }

        /// <summary>
        /// Create a new <see cref="MuPDFContext"/> instance with the specified cache store size.
        /// </summary>
        /// <param name="storeSize">The maximum size in bytes of the resource cache store. The default value is 256 MiB.</param>
        public MuPDFContext(uint storeSize = 256 << 20)
        {
            ExitCodes result = (ExitCodes)NativeMethods.CreateContext((ulong)storeSize, ref NativeContext);

            switch (result)
            {
                case ExitCodes.EXIT_SUCCESS:
                    break;
                case ExitCodes.ERR_CANNOT_CREATE_CONTEXT:
                    throw new MuPDFException("Cannot create MuPDF context", result);
                case ExitCodes.ERR_CANNOT_REGISTER_HANDLERS:
                    throw new MuPDFException("Cannot register document handlers", result);
                default:
                    throw new MuPDFException("Unknown error", result);
            }
        }

        /// <summary>
        /// Wrap an existing pointer to a native MuPDF context object.
        /// </summary>
        /// <param name="parentContext">The parent context for cloned contexts.</param>
        /// <param name="nativeContext">The pointer to the native context that should be used.</param>
        internal MuPDFContext(MuPDFContext parentContext, IntPtr nativeContext)
        {
            this.NativeContext = nativeContext;
            this.ParentContext = parentContext;
        }

        /// <summary>
        /// Evict all items from the resource cache store (freeing the memory where they were held).
        /// </summary>
        public void ClearStore()
        {
            NativeMethods.EmptyStore(this.NativeContext);
        }

        /// <summary>
        /// Evict items from the resource cache store (freeing the memory where they were held) until the the size of the store drops to the specified fraction of the current size.
        /// </summary>
        /// <param name="fraction">The fraction of the current size that constitutes the target size of the store. If this is &lt;= 0, the cache is cleared. If this is &gt;= 1, nothing happens.</param>
        public void ShrinkStore(double fraction)
        {
            if (fraction <= 0)
            {
                ClearStore();
            }
            else if (Math.Round(fraction * 100) < 100)
            {
                NativeMethods.ShrinkStore(this.NativeContext, (uint)Math.Round(fraction * 100));
            }
        }

        /// <summary>
        /// Resolve a font from the font cache, or create a new font object.
        /// </summary>
        /// <param name="nativePointer">The pointer to the font.</param>
        /// <returns>The cached font instance, or a new font object.</returns>
        internal MuPDFFont Resolve(IntPtr nativePointer)
        {
            lock (FontCacheLock)
            {
                (MuPDFFont font, int referenceCount) item;

                if (!this.FontCache.TryGetValue(nativePointer, out item) || item.font.disposedValue)
                {
                    item = (new MuPDFFont(this, nativePointer), 1);
                    this.FontCache[nativePointer] = item;
                }
                else
                {
                    item = (item.font, item.referenceCount + 1);
                    this.FontCache[nativePointer] = item;
                }

                return item.font;
            }
        }

        internal bool disposedValue;

        ///<inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && this.ParentContext == null)
                {
                    lock (fontCacheLock)
                    {
                        this.fontCache.Clear();
                    }

                    this.fontCache = null;
                }

                NativeMethods.DisposeContext(NativeContext);
                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        ~MuPDFContext()
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
}
