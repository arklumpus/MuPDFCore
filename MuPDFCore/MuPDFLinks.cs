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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MuPDFCore
{
    /// <summary>
    /// A collection of <see cref="MuPDFLink"/> objects.
    /// </summary>
    public class MuPDFLinks : IReadOnlyList<MuPDFLink>, IDisposable
    {
        private bool disposedValue;

        /// <inheritdoc/>
        public MuPDFLink this[int index] => Contents[index];

        /// <inheritdoc/>
        public int Count => Contents.Length;

        internal MuPDFPage OwnerPage { get; }
        private IntPtr FirstLinkPointer { get; }

        internal MuPDFLink[] Contents { get; }

        internal unsafe MuPDFLinks(MuPDFPage ownerPage)
        {
            this.OwnerPage = ownerPage;

            IntPtr firstLink = IntPtr.Zero;
            int countLinks = NativeMethods.CountLinks(ownerPage.OwnerDocument.OwnerContext.NativeContext, ownerPage.NativePage, ref firstLink);

            if (countLinks > 0)
            {
                this.FirstLinkPointer = firstLink;
                int[] uriLengths = new int[countLinks];
                IntPtr[] linkPointers = new IntPtr[countLinks];

                fixed (int* uriLengthsPtr = uriLengths)
                fixed (IntPtr* linkPointersPtr = linkPointers)
                {
                    NativeMethods.LoadLinks(firstLink, (IntPtr)linkPointersPtr, (IntPtr)uriLengthsPtr);
                }

                this.Contents = new MuPDFLink[countLinks];

                for (int i = 0; i < countLinks; i++)
                {
                    this.Contents[i] = new MuPDFLink(this, linkPointers[i], uriLengths[i]);
                }
            }
            else
            {
                this.Contents = new MuPDFLink[0];
                this.FirstLinkPointer = IntPtr.Zero;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<MuPDFLink> GetEnumerator()
        {
            return ((IEnumerable<MuPDFLink>)Contents).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Contents.GetEnumerator();
        }

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (this.FirstLinkPointer != IntPtr.Zero)
                {
                    NativeMethods.DisposeLinks(this.OwnerPage.OwnerDocument.OwnerContext.NativeContext, this.FirstLinkPointer);
                }
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        ~MuPDFLinks()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a link within a MuPDF document.
    /// </summary>
    public class MuPDFLink
    {
        /// <summary>
        /// The area on which users can click to activate the link.
        /// </summary>
        public Rectangle ActiveArea { get; }

        /// <summary>
        /// The link destination.
        /// </summary>
        public MuPDFLinkDestination Destination { get; }
        
        /// <summary>
        /// Whether the link is visible or hidden (e.g., because it is part of a hidden optional content group).
        /// </summary>
        public bool IsVisible => this.OwnerCollection.OwnerPage.OwnerDocument.NativePDFDocument == IntPtr.Zero || NativeMethods.IsLinkHidden(this.OwnerCollection.OwnerPage.OwnerDocument.OwnerContext.NativeContext, "View", this.NativeLink) == 0;

        private string Uri { get; }
        internal IntPtr NativeLink { get; }
        internal MuPDFLinks OwnerCollection { get; }

        internal unsafe MuPDFLink(MuPDFLinks ownerCollection, IntPtr linkPointer, int uriLength)
        {
            this.OwnerCollection = ownerCollection;
            float x0 = 0;
            float x1 = 0;
            float y0 = 0;
            float y1 = 0;

            byte[] uriBytes = new byte[uriLength];

            int isExternal = -1;
            int isSetOCGState = -1;

            int destinationType = -1;
            float x = 0;
            float y = 0;
            float w = 0;
            float h = 0;
            float zoom = 0;
            int page = -1;
            int chapter = -1;

            fixed (byte* uriPtr = uriBytes)
            {
                NativeMethods.LoadLink(ownerCollection.OwnerPage.OwnerDocument.OwnerContext.NativeContext, ownerCollection.OwnerPage.OwnerDocument.NativeDocument, linkPointer, uriLength, ownerCollection.OwnerPage.OwnerDocument.NativePDFDocument != IntPtr.Zero ? 1 : 0, ref x0, ref y0, ref x1, ref y1, (IntPtr)uriPtr, ref isExternal, ref isSetOCGState, ref destinationType, ref x, ref y, ref w, ref h, ref zoom, ref chapter, ref page);
            }

            this.ActiveArea = new Rectangle(x0, y0, x1, y1);

            string uri = Encoding.UTF8.GetString(uriBytes);

            this.Uri = uri;
            this.NativeLink = linkPointer;

            if (isExternal != 0)
            {
                this.Destination = new MuPDFExternalLinkDestination(this, uri);
            }
            else if (isSetOCGState != 0)
            {
                this.Destination = new MuPDFSetOCGStateLinkDestination(this);
            }
            else
            {
                this.Destination = new MuPDFInternalLinkDestination(this, page, chapter, x, y, w, h, zoom, destinationType);
            }
        }
    }

    /// <summary>
    /// Represents a generic link destination.
    /// </summary>
    public abstract class MuPDFLinkDestination
    {
        /// <summary>
        /// Defines the types of link destinations.
        /// </summary>
        public enum DestinationType
        {
            /// <summary>
            /// An internal link.
            /// </summary>
            Internal,

            /// <summary>
            /// An link to an external resource (e.g., a website).
            /// </summary>
            External,

            /// <summary>
            /// A link whose effect is to change the visibility state of some optional content groups (also known as layers).
            /// </summary>
            SetOCGState
        }

        /// <summary>
        /// The type of link destination.
        /// </summary>
        public abstract DestinationType Type { get; }

        internal MuPDFLink OwnerLink;
        
        internal MuPDFLinkDestination(MuPDFLink ownerLink)
        {
            this.OwnerLink = ownerLink;
        }
    }

    /// <summary>
    /// A link destination to an external resource (e.g., a website).
    /// </summary>
    public class MuPDFExternalLinkDestination : MuPDFLinkDestination
    {
        /// <summary>
        /// The Uri of the external resource.
        /// </summary>
        public string Uri { get; }

        /// <inheritdoc/>
        public override DestinationType Type => DestinationType.External;

        internal MuPDFExternalLinkDestination(MuPDFLink ownerLink, string uri) : base(ownerLink)
        {
            this.Uri = uri;
        }
    }

    /// <summary>
    /// A destination for a link whose effect is to change the visibility state of some optional content groups (also known as layers).
    /// </summary>
    public class MuPDFSetOCGStateLinkDestination : MuPDFLinkDestination
    {
        /// <inheritdoc/>
        public override DestinationType Type => DestinationType.SetOCGState;

        internal MuPDFSetOCGStateLinkDestination(MuPDFLink ownerLink) : base(ownerLink) { }

        /// <summary>
        /// Activate the link destination, thus showing/hiding optional content groups as necessary.
        /// </summary>
        public void Activate()
        {
            NativeMethods.ActivateLinkSetOCGState(this.OwnerLink.OwnerCollection.OwnerPage.OwnerDocument.OwnerContext.NativeContext, this.OwnerLink.OwnerCollection.OwnerPage.OwnerDocument.NativePDFDocument, this.OwnerLink.NativeLink);
            this.OwnerLink.OwnerCollection.OwnerPage.OwnerDocument.ClearCache();
        }
    }
    
    /// <summary>
    /// An internal link destination.
    /// </summary>
    public class MuPDFInternalLinkDestination : MuPDFLinkDestination
    {
        /// <summary>
        /// Defines internal link destination types.
        /// </summary>
        public enum InternalDestinationType
        {
            /// <summary>
            /// Display the target <see cref="Page"/>, with an appropriate magnification factor so that it completely fits within the view area. 
            /// </summary>
            Fit,
            
            /// <summary>
            /// Display the target <see cref="Page"/>, with an appropriate magnification factor so that its bounding box completely fits within the view area. 
            /// </summary>
            FitBoundingBox,

            /// <summary>
            /// Display the target <see cref="Page"/>, with an appropriate magnification factor so that the page width completely fits within the view area. 
            /// </summary>
            FitWidth,

            /// <summary>
            /// Display the target <see cref="Page"/>, with an appropriate magnification factor so that the bounding box width completely fits within the view area. 
            /// </summary>
            FitBoundingBoxWidth,

            /// <summary>
            /// Display the target <see cref="Page"/>, with an appropriate magnification factor so that the page height completely fits within the view area. 
            /// </summary>
            FitHeight,

            /// <summary>
            /// Display the target <see cref="Page"/>, with an appropriate magnification factor so that the bounding box height completely fits within the view area. 
            /// </summary>
            FitBoundingBoxHeight,

            /// <summary>
            /// Display the target <see cref="Page"/>, with an appropriate magnification factor and traslation, so that the rectangle specified by {<see cref="X"/>, <see cref="Y"/>, <see cref="Width"/>, <see cref="Height"/>} completely fits within the viewing area.
            /// </summary>
            FitRectangle,
            
            /// <summary>
            /// Display the target <see cref="Page"/> with {<see cref="X"/>, <see cref="Y"/>} on the top-left corner and the specified <see cref="Zoom"/> (which can be <c>0</c> - in this case, the zoom factor should be unchanged).
            /// </summary>
            XYZoom
        }

        /// <inheritdoc/>
        public override DestinationType Type => DestinationType.Internal;

        /// <summary>
        /// Locations within the document are referred to in terms of chapter and page, rather than just a page number. For some documents (such as epub documents with large numbers of pages broken into many chapters) this can make navigation much faster as only the required chapter needs to be decoded at a time.
        /// </summary>
        public int Chapter { get; }

        /// <summary>
        /// The page number of an internal link, relative to the specified <see cref="Chapter"/>, or -1 for external links or links with no destination.
        /// </summary>
        public int Page { get; }
        
        private int? CachedPageNumber = null;

        /// <summary>
        /// The overall page number of an internal link. This is determined on first access, and it might cause a large number of chapters to be laid out to determine it.
        /// </summary>
        public int PageNumber
        {
            get
            {
                if (CachedPageNumber == null)
                {
                    CachedPageNumber = NativeMethods.GetPageNumber(this.OwnerLink.OwnerCollection.OwnerPage.OwnerDocument.OwnerContext.NativeContext, this.OwnerLink.OwnerCollection.OwnerPage.OwnerDocument.NativeDocument, this.Chapter, this.Page);
                }

                return CachedPageNumber.Value;
            }
        }

        /// <summary>
        /// X coordinate of the link target on the specified page.
        /// </summary>
        public float X { get; }

        /// <summary>
        /// Y coordinate of the link target on the specified page.
        /// </summary>
        public float Y { get; }

        /// <summary>
        /// Width of the link target on the specified page.
        /// </summary>
        public float Width { get; }

        /// <summary>
        /// Height of the link target on the specified page.
        /// </summary>
        public float Height { get; }
        
        /// <summary>
        /// Target magnification factor.
        /// </summary>
        public float Zoom { get; }
        
        /// <summary>
        /// The type of internal link destination.
        /// </summary>
        public InternalDestinationType InternalType { get; }

        internal MuPDFInternalLinkDestination(MuPDFLink ownerLink, int page, int chapter, float x, float y, float w, float h, float zoom, int type) : base(ownerLink)
        {
            this.Page = page;
            this.Chapter = chapter;
            this.X = x;
            this.Y = y;
            this.Width = w;
            this.Height = h;
            this.Zoom = zoom;
            this.InternalType = (InternalDestinationType)type;
        }
    }
}
